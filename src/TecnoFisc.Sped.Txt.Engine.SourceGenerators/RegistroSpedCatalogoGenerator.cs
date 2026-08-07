using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TecnoFisc.Sped.Txt.Engine.SourceGenerators;

/// <summary>
/// Gera, em compile-time, a classe <c>CatalogoSpedGerado</c> de cada projeto que declara
/// registros decorados com <c>[RegistroSped]</c>. Cada propriedade marcada com
/// <c>[CampoSped]</c> vira um par de helpers privados (parse + serialize) emitidos com casts
/// diretos para o tipo concreto — sem reflexão, sem <c>Expression.Compile</c> e sem boxing
/// para os tipos cobertos (string, inteiros, decimal, DateOnly, bool, char, enums e value
/// objects fiscais conhecidos).
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class RegistroSpedCatalogoGenerator : IIncrementalGenerator
{
    private const string AttributeRegistro = "TecnoFisc.Sped.Txt.Engine.Atributos.RegistroSpedAttribute";
    private const string AttributeCampo = "TecnoFisc.Sped.Txt.Engine.Atributos.CampoSpedAttribute";
    private const string AttributeSpedValor = "TecnoFisc.Sped.Txt.Engine.Atributos.SpedValorAttribute";
    private const string AttributeFlags = "System.FlagsAttribute";

    private static readonly DiagnosticDescriptor InvalidFieldNameDiagnostic = new(
        id: "TFSPED001",
        title: "Nome de campo SPED inválido",
        messageFormat: "O alias Nome '{1}' em '{0}' deve ser um identificador Latino-1 sem espaços",
        category: "TecnoFisc.Sped.Txt.Engine",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DuplicateFieldNameDiagnostic = new(
        id: "TFSPED002",
        title: "Nome de campo SPED duplicado",
        messageFormat: "O nome de campo SPED '{1}' está duplicado em '{0}'",
        category: "TecnoFisc.Sped.Txt.Engine",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Espelha <c>CatalogoBuilder.ValidarVigenciaCrescente</c>: <c>DesdeVersao</c> precisa ser
    /// não-decrescente ao longo da posição dos campos, porque o mapeamento posicional do leitor
    /// (<c>LeitorSpedTxt.InterpretarLinha</c>, <c>indice = posicaoCampo - 2</c>) só é correto
    /// quando um campo versionado fica no fim do registro.
    /// </summary>
    private static readonly DiagnosticDescriptor NonIncreasingVigenciaDiagnostic = new(
        id: "TFSPED003",
        title: "Vigência de campo fora de ordem",
        messageFormat: "Campo '{0}' viola vigência não-decrescente: {1}",
        category: "TecnoFisc.Sped.Txt.Engine",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<InfoRegistro> registros = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeRegistro,
                predicate: static (n, _) => n is ClassDeclarationSyntax,
                transform: static (ctx, _) => ExtrairInfoRegistro(ctx))
            .Where(static info => info.HasValue)
            .Select(static (info, _) => info!.Value);

        IncrementalValueProvider<ImmutableArray<InfoRegistro>> coletados = registros.Collect();
        IncrementalValueProvider<string> assemblyName = context.CompilationProvider
            .Select(static (c, _) => c.AssemblyName ?? "Generated");

        var combinado = coletados.Combine(assemblyName);

        context.RegisterSourceOutput(combinado, static (spc, dados) =>
        {
            var (lista, asmName) = dados;
            if (lista.IsDefaultOrEmpty)
                return;

            foreach (InfoRegistro recordInfo in lista)
            {
                foreach (FieldError error in recordInfo.Errors)
                {
                    DiagnosticDescriptor descriptor = error.Kind switch
                    {
                        FieldErrorKind.InvalidName => InvalidFieldNameDiagnostic,
                        FieldErrorKind.DuplicateName => DuplicateFieldNameDiagnostic,
                        FieldErrorKind.NonIncreasingVigencia => NonIncreasingVigenciaDiagnostic,
                        _ => throw new System.InvalidOperationException(
                            $"FieldErrorKind desconhecido ({error.Kind}) — adicionar o descriptor correspondente."),
                    };
                    spc.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        error.Location,
                        error.Property,
                        error.Name));
                }
            }

            // Os diagnósticos acima seguem falhando o build (comportamento desejado). Mas o
            // catálogo/visitor precisam ser emitidos de qualquer forma: um único diagnóstico de
            // campo não pode suprimir os tipos gerados do assembly inteiro, senão o build real
            // afoga o diagnóstico acionável sob uma cascata de CS0246 (achado 6 do PR 531).
            string fonte = GerarCatalogo(lista, asmName);
            spc.AddSource("CatalogoSpedGerado.g.cs", SourceText.From(fonte, Encoding.UTF8));

            string visitor = GerarVisitor(lista, asmName);
            spc.AddSource("RegistroSpedVisitor.g.cs", SourceText.From(visitor, Encoding.UTF8));
        });
    }

    /// <summary>
    /// Emite a interface <c>IRegistroSpedVisitor</c> (com um overload <c>VisitAsync</c> default
    /// por tipo concreto de registro do assembly) e a extension <c>DispatchAsync</c> que faz o
    /// despacho do <c>IAsyncEnumerable&lt;RegistroSped&gt;</c> para o overload correto via
    /// switch resolvido em compile-time. Sem reflection, sem boxing, sem chained <c>if</c>.
    /// </summary>
    private static string GerarVisitor(ImmutableArray<InfoRegistro> registros, string assemblyName)
    {
        ImmutableArray<InfoRegistro> ordenados = OrdenarRegistros(registros);

        var sb = new StringBuilder(capacity: 32 * 1024);
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using TecnoFisc.Sped.Txt.Engine.Abstracoes;");
        sb.AppendLine();
        sb.Append("namespace ").Append(assemblyName).AppendLine(".Generated;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Visitor tipado para os registros declarados em ");
        sb.Append("/// <c>").Append(assemblyName).AppendLine("</c>. Cada overload <c>VisitAsync</c>");
        sb.AppendLine("/// recebe o tipo concreto do registro; a implementação default não faz nada,");
        sb.AppendLine("/// então o consumidor pode sobrescrever apenas os tipos que importam para o");
        sb.AppendLine("/// caso de uso (persistência seletiva, ETL parcial, etc.). <c>VisitUnknownAsync</c>");
        sb.AppendLine("/// é chamado para registros que não pertencem a este assembly.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public interface IRegistroSpedVisitor");
        sb.AppendLine("{");

        foreach (InfoRegistro reg in ordenados)
        {
            sb.Append("    ValueTask VisitAsync(").Append(reg.TipoFullyQualified)
                .AppendLine(" registro, CancellationToken cancellationToken) => default;");
        }

        sb.AppendLine("    ValueTask VisitUnknownAsync(RegistroSped registro, CancellationToken cancellationToken) => default;");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Extension de despacho para o <see cref=\"IRegistroSpedVisitor\"/> gerado.");
        sb.AppendLine("/// Itera o stream e roteia cada registro para o overload correto via switch");
        sb.AppendLine("/// resolvido em compile-time — sem reflection.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class RegistroSpedVisitorExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Despacha cada registro do <paramref name=\"origem\"/> para o overload");
        sb.AppendLine("    /// correto do <paramref name=\"visitor\"/>. Registros de outros assemblies");
        sb.AppendLine("    /// (ou descendentes que não casam pelo tipo exato) caem em");
        sb.AppendLine("    /// <see cref=\"IRegistroSpedVisitor.VisitUnknownAsync\"/>.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static async Task DispatchAsync(");
        sb.AppendLine("        this IAsyncEnumerable<RegistroSped> origem,");
        sb.AppendLine("        IRegistroSpedVisitor visitor,");
        sb.AppendLine("        CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        ArgumentNullException.ThrowIfNull(origem);");
        sb.AppendLine("        ArgumentNullException.ThrowIfNull(visitor);");
        sb.AppendLine();
        sb.AppendLine("        await foreach (var registro in origem.WithCancellation(cancellationToken).ConfigureAwait(false))");
        sb.AppendLine("        {");
        sb.AppendLine("            switch (registro)");
        sb.AppendLine("            {");

        foreach (InfoRegistro reg in ordenados)
        {
            sb.Append("                case ").Append(reg.TipoFullyQualified)
                .AppendLine(" r:");
            sb.AppendLine("                    await visitor.VisitAsync(r, cancellationToken).ConfigureAwait(false);");
            sb.AppendLine("                    break;");
        }

        sb.AppendLine("                default:");
        sb.AppendLine("                    await visitor.VisitUnknownAsync(registro, cancellationToken).ConfigureAwait(false);");
        sb.AppendLine("                    break;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static InfoRegistro? ExtrairInfoRegistro(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol tipo || tipo.IsAbstract)
            return null;

        AttributeData? atributo = ctx.Attributes.FirstOrDefault();
        if (atributo is null)
            return null;

        string? codigo = null;
        string? bloco = null;
        int nivel = 0;
        int introduzidoEm = 0;
        string? tokenFimArquivo = null;
        foreach (KeyValuePair<string, TypedConstant> arg in atributo.NamedArguments)
        {
            switch (arg.Key)
            {
                case "Codigo": codigo = arg.Value.Value as string; break;
                case "Nivel": nivel = arg.Value.Value is int n ? n : 0; break;
                case "Bloco": bloco = arg.Value.Value as string; break;
                case "IntroduzidoEm": introduzidoEm = arg.Value.Value is int iv ? iv : 0; break;
                case "TokenFimArquivo": tokenFimArquivo = arg.Value.Value as string; break;
            }
        }
        if (string.IsNullOrEmpty(codigo) || string.IsNullOrEmpty(bloco))
            return null;

        FieldCollectionResult fieldsResult = ColetarCampos(tipo);

        string tipoFq = tipo.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return new InfoRegistro(
            tipoFq,
            codigo!,
            nivel,
            bloco!,
            fieldsResult.Fields,
            introduzidoEm,
            tokenFimArquivo,
            fieldsResult.Errors);
    }

    private static FieldCollectionResult ColetarCampos(INamedTypeSymbol tipoRegistro)
    {
        var lista = new List<InfoCampo>();
        var fieldNames = new HashSet<string>(System.StringComparer.Ordinal);
        var locationPorOrdem = new Dictionary<int, Location?>();
        ImmutableArray<FieldError>.Builder errors = ImmutableArray.CreateBuilder<FieldError>();

        // Atravessa toda a hierarquia (heranças entre registros existem em alguns layouts).
        for (INamedTypeSymbol? atual = tipoRegistro; atual is not null; atual = atual.BaseType)
        {
            foreach (ISymbol membro in atual.GetMembers())
            {
                if (membro is not IPropertySymbol propriedade)
                    continue;

                AttributeData? campoAttr = propriedade.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == AttributeCampo);
                if (campoAttr is null)
                    continue;

                int ordem = 0, tamanho = 0, decimais = 0, desdeVersao = 0;
                bool obrigatorio = false;
                bool capturaTudo = false;
                bool campoArquivo = false;
                string? formato = null;
                string? explicitName = null;
                foreach (KeyValuePair<string, TypedConstant> arg in campoAttr.NamedArguments)
                {
                    switch (arg.Key)
                    {
                        case "Ordem": ordem = arg.Value.Value is int o ? o : 0; break;
                        case "Tamanho": tamanho = arg.Value.Value is int t ? t : 0; break;
                        case "Decimais": decimais = arg.Value.Value is int d ? d : 0; break;
                        case "Obrigatorio": obrigatorio = arg.Value.Value is bool b && b; break;
                        case "Formato": formato = arg.Value.Value as string; break;
                        case "Nome": explicitName = arg.Value.Value as string; break;
                        case "DesdeVersao": desdeVersao = arg.Value.Value is int dv ? dv : 0; break;
                        case "CapturaTudo": capturaTudo = arg.Value.Value is bool ct && ct; break;
                        case "CampoArquivo": campoArquivo = arg.Value.Value is bool ca && ca; break;
                    }
                }

                string fieldName = string.IsNullOrEmpty(explicitName) ? propriedade.Name : explicitName!;
                Location? location = propriedade.Locations.FirstOrDefault();
                if (!locationPorOrdem.ContainsKey(ordem))
                    locationPorOrdem[ordem] = location;
                string fullyQualifiedProperty = propriedade.ContainingType.ToDisplayString() + "." + propriedade.Name;
                if (!string.IsNullOrEmpty(explicitName) && !IsFieldNameValid(explicitName!))
                {
                    errors.Add(new FieldError(
                        FieldErrorKind.InvalidName,
                        location,
                        fullyQualifiedProperty,
                        EscapeFieldName(explicitName!)));
                    // Alias inválido não tem fallback textual seguro para emitir no catálogo —
                    // usa o nome CLR da propriedade, que sempre é um identificador válido.
                    fieldName = propriedade.Name;
                }
                else if (!fieldNames.Add(fieldName))
                {
                    errors.Add(new FieldError(
                        FieldErrorKind.DuplicateName,
                        location,
                        fullyQualifiedProperty,
                        fieldName));
                }

                ITypeSymbol tipoPropriedade = propriedade.Type;
                bool nullable = false;
                if (tipoPropriedade is INamedTypeSymbol named && named.IsGenericType
                    && named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
                {
                    nullable = true;
                    tipoPropriedade = named.TypeArguments[0];
                }

                CategoriaCampo categoria = ClassificarTipo(tipoPropriedade, out string? underlyingPrimitivo);
                string tipoFq = tipoPropriedade.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                string tipoNome = tipoPropriedade.Name;
                string tipoFqDeclarado = propriedade.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                ImmutableArray<string> enumValoresSped = categoria == CategoriaCampo.Enum
                    && tipoPropriedade is INamedTypeSymbol enumTipo
                    ? ColetarMapeamentoSpedValor(enumTipo)
                    : ImmutableArray<string>.Empty;
                bool enumFlags = categoria == CategoriaCampo.Enum
                    && tipoPropriedade.GetAttributes()
                        .Any(a => a.AttributeClass?.ToDisplayString() == AttributeFlags);

                lista.Add(new InfoCampo(
                    propriedade.Name,
                    fieldName,
                    ordem,
                    tipoFq,
                    tipoNome,
                    tipoFqDeclarado,
                    categoria,
                    nullable,
                    underlyingPrimitivo,
                    tamanho,
                    decimais,
                    obrigatorio,
                    formato,
                    desdeVersao,
                    capturaTudo,
                    campoArquivo,
                    enumValoresSped,
                    enumFlags));
            }
        }

        ImmutableArray<InfoCampo> fields = lista
            .GroupBy(c => c.Ordem)
            .Select(g => g.First())
            .OrderBy(c => c.Ordem)
            .ToImmutableArray();

        // DesdeVersao precisa ser não-decrescente ao longo da posição — ver NonIncreasingVigenciaDiagnostic.
        for (int i = 1; i < fields.Length; i++)
        {
            InfoCampo anterior = fields[i - 1];
            InfoCampo atual = fields[i];
            if (atual.DesdeVersao < anterior.DesdeVersao)
            {
                string fullyQualifiedProperty = tipoRegistro.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + "." + atual.Nome;
                Location? location = locationPorOrdem.TryGetValue(atual.Ordem, out Location? loc) ? loc : null;
                errors.Add(new FieldError(
                    FieldErrorKind.NonIncreasingVigencia,
                    location,
                    fullyQualifiedProperty,
                    $"DesdeVersao={atual.DesdeVersao} vem depois de '{anterior.FieldName}' (DesdeVersao=" +
                    $"{anterior.DesdeVersao}); campo versionado só pode ficar no fim do registro, senão o " +
                    "mapeamento posicional do leitor desalinha silenciosamente."));
            }
        }

        return new FieldCollectionResult(fields, errors.ToImmutable());
    }

    private static bool IsFieldNameValid(string name)
    {
        if (name.Length == 0 || !IsFieldNameStart(name[0]))
            return false;

        for (int i = 1; i < name.Length; i++)
        {
            char character = name[i];
            if (!IsFieldNameStart(character) && (character < '0' || character > '9'))
                return false;
        }

        return true;
    }

    private static bool IsFieldNameStart(char character)
        => character == '_' ||
           (character >= 'A' && character <= 'Z') ||
           (character >= 'a' && character <= 'z') ||
           (character >= '\u00C0' && character <= '\u00FF' && char.IsLetter(character));

    private static string EscapeFieldName(string name)
        => name
            .Replace("\\", "\\\\")
            .Replace("\t", "\\t")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");

    /// <summary>
    /// Coleta membros do enum decorados com <c>[SpedValor("X")]</c> no formato
    /// <c>"NomeMembro\0ValorSped"</c>. Vazio se nenhum membro tem o atributo — nesse caso o
    /// gerador trata o campo como enum integral (parse via <c>int.Parse</c>).
    /// </summary>
    private static ImmutableArray<string> ColetarMapeamentoSpedValor(INamedTypeSymbol enumSym)
    {
        if (enumSym.TypeKind != TypeKind.Enum)
            return ImmutableArray<string>.Empty;

        ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>();
        foreach (ISymbol membro in enumSym.GetMembers())
        {
            if (membro is not IFieldSymbol campo || !campo.IsConst)
                continue;

            AttributeData? attr = campo.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == AttributeSpedValor);
            if (attr is null || attr.ConstructorArguments.Length == 0)
                continue;

            if (attr.ConstructorArguments[0].Value is not string valor)
                continue;

            builder.Add(campo.Name + "\0" + valor);
        }
        return builder.ToImmutable();
    }

    private static CategoriaCampo ClassificarTipo(ITypeSymbol tipo, out string? underlyingPrimitivo)
    {
        underlyingPrimitivo = null;

        switch (tipo.SpecialType)
        {
            case SpecialType.System_String: return CategoriaCampo.StringCampo;
            case SpecialType.System_Int32: return CategoriaCampo.Int32;
            case SpecialType.System_Int64: return CategoriaCampo.Int64;
            case SpecialType.System_Int16: return CategoriaCampo.Int16;
            case SpecialType.System_Decimal: return CategoriaCampo.Decimal;
            case SpecialType.System_Boolean: return CategoriaCampo.Bool;
            case SpecialType.System_Char: return CategoriaCampo.Char;
        }

        string fq = tipo.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (fq == "global::System.DateOnly")
            return CategoriaCampo.DateOnly;

        if (tipo.TypeKind == TypeKind.Enum && tipo is INamedTypeSymbol enumSym)
        {
            underlyingPrimitivo = enumSym.EnumUnderlyingType?.SpecialType switch
            {
                SpecialType.System_Byte => "byte",
                SpecialType.System_SByte => "sbyte",
                SpecialType.System_Int16 => "short",
                SpecialType.System_UInt16 => "ushort",
                SpecialType.System_Int32 => "int",
                SpecialType.System_UInt32 => "uint",
                SpecialType.System_Int64 => "long",
                SpecialType.System_UInt64 => "ulong",
                _ => "int",
            };
            return CategoriaCampo.Enum;
        }

        if (IsKnownValueObject(fq))
            return CategoriaCampo.ValueObject;

        return CategoriaCampo.NaoSuportado;
    }

    private static bool IsKnownValueObject(string fqName) => fqName switch
    {
        "global::TecnoFisc.Sped.Core.ValueObjects.Cnpj" => true,
        "global::TecnoFisc.Sped.Core.ValueObjects.Cpf" => true,
        "global::TecnoFisc.Sped.Core.ValueObjects.Cfop" => true,
        "global::TecnoFisc.Sped.Core.ValueObjects.Ncm" => true,
        "global::TecnoFisc.Sped.Core.ValueObjects.ChaveAcesso" => true,
        "global::TecnoFisc.Sped.Core.ValueObjects.InscricaoEstadual" => true,
        "global::TecnoFisc.Sped.Core.ValueObjects.ModeloDocumento" => true,
        "global::TecnoFisc.Sped.Core.ValueObjects.GeneroItem" => true,
        _ => false,
    };

    private static string GerarCatalogo(ImmutableArray<InfoRegistro> registros, string assemblyName)
    {
        var sb = new StringBuilder(capacity: 64 * 1024);
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Globalization;");
        sb.AppendLine("using TecnoFisc.Sped.Txt.Engine.Abstracoes;");
        sb.AppendLine("using TecnoFisc.Sped.Txt.Engine.Catalogo;");
        sb.AppendLine("using TecnoFisc.Sped.Txt.Engine.Gerador;");
        sb.AppendLine("using TecnoFisc.Sped.Txt.Engine.Parser;");
        sb.AppendLine();
        sb.Append("namespace ").Append(assemblyName).AppendLine(".Generated;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Catálogo de registros SPED gerado em compile-time. Cada campo tem helpers");
        sb.AppendLine("/// privados de parse/serialize com casts diretos para o tipo concreto, sem");
        sb.AppendLine("/// reflexão e sem alocação de string por campo no caminho gerado.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public sealed class CatalogoSpedGerado : CatalogoSpedBase");
        sb.AppendLine("{");
        sb.AppendLine("    public CatalogoSpedGerado() : base(BuildRegistros()) { }");
        sb.AppendLine();
        sb.AppendLine("    private static Dictionary<string, MetadadosRegistro> BuildRegistros()");
        sb.AppendLine("    {");
        sb.Append("        var registros = new Dictionary<string, MetadadosRegistro>(")
            .Append(registros.Length.ToString(CultureInfo.InvariantCulture))
            .AppendLine(", StringComparer.Ordinal);");

        ImmutableArray<InfoRegistro> ordenados = OrdenarRegistros(registros);

        foreach (InfoRegistro reg in ordenados)
        {
            sb.Append("        registros.Add(\"").Append(reg.Codigo).Append("\", Build_")
                .Append(reg.Codigo).AppendLine("());");
        }

        sb.AppendLine("        return registros;");
        sb.AppendLine("    }");

        foreach (InfoRegistro reg in ordenados)
            EmitirRegistro(sb, reg);

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static ImmutableArray<InfoRegistro> OrdenarRegistros(IEnumerable<InfoRegistro> registros)
        => registros
            .OrderBy(static registro => CategoriaOrdemBloco(registro.Bloco))
            .ThenBy(static registro => registro.Bloco, System.StringComparer.Ordinal)
            .ThenBy(static registro => registro.Codigo, System.StringComparer.Ordinal)
            .ToImmutableArray();

    private static int CategoriaOrdemBloco(string bloco)
    {
        if (string.Equals(bloco, "0", System.StringComparison.Ordinal))
            return 0;

        if (bloco.Length == 1 && bloco[0] >= 'A' && bloco[0] <= 'Z')
            return 1;

        if (bloco.Length == 1 && bloco[0] >= '1' && bloco[0] <= '8')
            return 2;

        if (string.Equals(bloco, "9", System.StringComparison.Ordinal))
            return 3;

        return 4;
    }

    private static void EmitirRegistro(StringBuilder sb, InfoRegistro reg)
    {
        sb.AppendLine();
        sb.Append("    private static MetadadosRegistro Build_").Append(reg.Codigo).AppendLine("()");
        sb.AppendLine("    {");
        sb.AppendLine("        return new MetadadosRegistro(");
        sb.Append("            \"").Append(reg.Codigo).Append("\", ")
            .Append(reg.Nivel.ToString(CultureInfo.InvariantCulture)).Append(", \"")
            .Append(reg.Bloco).AppendLine("\",");
        sb.Append("            typeof(").Append(reg.TipoFullyQualified).AppendLine("),");
        sb.Append("            static () => new ").Append(reg.TipoFullyQualified).AppendLine("(),");
        sb.AppendLine("            new MetadadosCampo[]");
        sb.AppendLine("            {");

        for (int i = 0; i < reg.Campos.Length; i++)
        {
            InfoCampo campo = reg.Campos[i];
            EmitirCampo(sb, reg, campo, ultimo: i == reg.Campos.Length - 1);
        }

        sb.Append("            }");
        if (reg.IntroduzidoEm != 0)
        {
            sb.Append(',').AppendLine();
            sb.Append("            introduzidoEm: ").Append(reg.IntroduzidoEm.ToString(CultureInfo.InvariantCulture));
        }
        if (reg.TokenFimArquivo is not null)
        {
            sb.Append(',').AppendLine();
            sb.Append("            tokenFimArquivo: \"").Append(EscaparLiteral(reg.TokenFimArquivo)).Append('"');
        }
        sb.AppendLine(");");
        sb.AppendLine("    }");

        foreach (InfoCampo c in reg.Campos)
        {
            EmitirHelperSet(sb, reg, c);
            EmitirHelperSer(sb, reg, c);
        }
    }

    private static void EmitirCampo(StringBuilder sb, InfoRegistro reg, InfoCampo campo, bool ultimo)
    {
        sb.Append("                new MetadadosCampo(");
        sb.Append('"').Append(EscaparLiteral(campo.FieldName)).Append("\", ");
        sb.Append(campo.Ordem.ToString(CultureInfo.InvariantCulture)).Append(", ");
        sb.Append("typeof(").Append(campo.TipoDeclaradoFq).Append("), ");
        sb.Append(campo.Tamanho.ToString(CultureInfo.InvariantCulture)).Append(", ");
        sb.Append(campo.Decimais.ToString(CultureInfo.InvariantCulture)).Append(", ");
        sb.Append(campo.Obrigatorio ? "true" : "false").Append(", ");
        if (campo.Formato is null)
            sb.Append("null, ");
        else
            sb.Append('"').Append(campo.Formato).Append("\", ");
        sb.Append("Set_").Append(reg.Codigo).Append('_').Append(campo.Nome).Append(", ");
        sb.Append("Ser_").Append(reg.Codigo).Append('_').Append(campo.Nome);
        if (campo.DesdeVersao != 0)
            sb.Append(", desdeVersao: ").Append(campo.DesdeVersao.ToString(CultureInfo.InvariantCulture));
        if (campo.CapturaTudo)
            sb.Append(", capturaTudo: true");
        if (campo.CampoArquivo)
            sb.Append(", campoArquivo: true");
        if (PrecisaSetterEstrito(campo))
        {
            sb.Append(", definidorEstrito: Set_").Append(reg.Codigo).Append('_')
                .Append(campo.Nome).Append("_Estrito");
        }
        sb.Append(')');
        sb.AppendLine(ultimo ? "" : ",");
    }

    private static void EmitirHelperSet(StringBuilder sb, InfoRegistro reg, InfoCampo c)
    {
        sb.Append("    private static void Set_").Append(reg.Codigo).Append('_').Append(c.Nome)
            .AppendLine("(RegistroSped registro, ReadOnlySpan<char> valor)");
        sb.AppendLine("    {");
        sb.Append("        var alvo = (").Append(reg.TipoFullyQualified).AppendLine(")registro;");

        switch (c.Categoria)
        {
            case CategoriaCampo.StringCampo:
                sb.Append("        alvo.").Append(c.Nome).AppendLine(" = valor.IsEmpty ? null : valor.ToString();");
                break;

            case CategoriaCampo.Int32:
                EmitirSetterPrimitivoNumerico(sb, c, "int", "0");
                break;
            case CategoriaCampo.Int64:
                EmitirSetterPrimitivoNumerico(sb, c, "long", "0L");
                break;
            case CategoriaCampo.Int16:
                EmitirSetterPrimitivoNumerico(sb, c, "short", "(short)0");
                break;

            case CategoriaCampo.Decimal:
                EmitirSetterDecimal(sb, c);
                break;

            case CategoriaCampo.DateOnly:
                EmitirSetterDateOnly(sb, c);
                break;

            case CategoriaCampo.Bool:
                EmitirSetterBool(sb, c);
                break;

            case CategoriaCampo.Char:
                EmitirSetterChar(sb, c);
                break;

            case CategoriaCampo.Enum:
                EmitirSetterEnum(sb, c);
                break;

            case CategoriaCampo.ValueObject:
                EmitirSetterValueObject(sb, c);
                break;

            case CategoriaCampo.NaoSuportado:
            default:
                EmitirSetterFallbackReflexivo(sb, c);
                break;
        }

        if (PrecisaSetterEstrito(c))
        {
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.Append("    private static void Set_").Append(reg.Codigo).Append('_').Append(c.Nome)
                .AppendLine("_Estrito(RegistroSped registro, ReadOnlySpan<char> valor)");
            sb.AppendLine("    {");
            sb.Append("        var alvo = (").Append(reg.TipoFullyQualified).AppendLine(")registro;");
            EmitirSetterEnumEstrito(sb, c);
        }

        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void EmitirSetterPrimitivoNumerico(StringBuilder sb, InfoCampo c, string tipoCs, string padrao)
    {
        if (c.Nullable)
        {
            sb.Append("        if (valor.IsEmpty) { alvo.").Append(c.Nome).AppendLine(" = null; return; }");
            sb.Append("        alvo.").Append(c.Nome).Append(" = ").Append(tipoCs)
                .AppendLine(".Parse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture);");
        }
        else
        {
            sb.Append("        alvo.").Append(c.Nome).Append(" = valor.IsEmpty ? ").Append(padrao)
                .Append(" : ").Append(tipoCs)
                .AppendLine(".Parse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture);");
        }
    }

    private static void EmitirSetterDecimal(StringBuilder sb, InfoCampo c)
    {
        if (c.Nullable)
        {
            sb.Append("        if (valor.IsEmpty) { alvo.").Append(c.Nome).AppendLine(" = null; return; }");
            sb.Append("        alvo.").Append(c.Nome).AppendLine(" = ParseadoresPrimitivos.ParaDecimal(valor);");
        }
        else
        {
            sb.Append("        alvo.").Append(c.Nome)
                .AppendLine(" = valor.IsEmpty ? 0m : ParseadoresPrimitivos.ParaDecimal(valor);");
        }
    }

    private static void EmitirSetterDateOnly(StringBuilder sb, InfoCampo c)
    {
        string formato = c.Formato is null ? "null" : "\"" + c.Formato + "\"";
        if (c.Nullable)
        {
            sb.Append("        if (valor.IsEmpty) { alvo.").Append(c.Nome).AppendLine(" = null; return; }");
            sb.Append("        alvo.").Append(c.Nome).Append(" = ParseadoresPrimitivos.DataComFormato(valor, ")
                .Append(formato).AppendLine(");");
        }
        else
        {
            sb.Append("        alvo.").Append(c.Nome).Append(" = valor.IsEmpty ? default : ParseadoresPrimitivos.DataComFormato(valor, ")
                .Append(formato).AppendLine(");");
        }
    }

    private static void EmitirSetterBool(StringBuilder sb, InfoCampo c)
    {
        if (c.Nullable)
        {
            sb.Append("        if (valor.IsEmpty) { alvo.").Append(c.Nome).AppendLine(" = null; return; }");
            sb.Append("        alvo.").Append(c.Nome).AppendLine(" = bool.Parse(valor);");
        }
        else
        {
            sb.Append("        alvo.").Append(c.Nome).AppendLine(" = !valor.IsEmpty && bool.Parse(valor);");
        }
    }

    private static void EmitirSetterChar(StringBuilder sb, InfoCampo c)
    {
        sb.AppendLine("        if (valor.IsEmpty)");
        sb.AppendLine("        {");
        sb.Append("            alvo.").Append(c.Nome).Append(" = ").Append(c.Nullable ? "null" : "default")
            .AppendLine(";");
        sb.AppendLine("            return;");
        sb.AppendLine("        }");
        sb.AppendLine("        if (valor.Length != 1)");
        sb.AppendLine("            throw new FormatException(\"Esperado 1 caractere, recebeu \" + valor.Length + \" em '\" + valor.ToString() + \"'.\");");
        sb.Append("        alvo.").Append(c.Nome).AppendLine(" = valor[0];");
    }

    private static void EmitirSetterEnum(StringBuilder sb, InfoCampo c)
    {
        if (!c.EnumValoresSped.IsDefaultOrEmpty)
        {
            EmitirSetterEnumTextual(sb, c);
            return;
        }

        string under = c.UnderlyingPrimitivo ?? "int";
        if (c.Nullable)
        {
            sb.Append("        if (valor.IsEmpty) { alvo.").Append(c.Nome).AppendLine(" = null; return; }");
        }
        else
        {
            sb.AppendLine("        if (valor.IsEmpty)");
            sb.AppendLine("        {");
            sb.Append("            alvo.").Append(c.Nome).AppendLine(" = default;");
            sb.AppendLine("            return;");
            sb.AppendLine("        }");
        }
        sb.Append("        ").Append(under).Append(" bruto = ").Append(under)
            .AppendLine(".Parse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture);");
        sb.Append("        alvo.").Append(c.Nome).Append(" = (").Append(c.TipoFq).AppendLine(")bruto;");
    }

    /// <summary>
    /// Emite o corpo do setter estrito de enum fechado: mesma conversão do permissivo, mais a
    /// checagem de domínio. Só é chamado quando <see cref="PrecisaSetterEstrito"/> for true.
    /// </summary>
    private static void EmitirSetterEnumEstrito(StringBuilder sb, InfoCampo c)
    {
        string under = c.UnderlyingPrimitivo ?? "int";
        if (c.Nullable)
        {
            sb.Append("        if (valor.IsEmpty) { alvo.").Append(c.Nome).AppendLine(" = null; return; }");
        }
        else
        {
            sb.AppendLine("        if (valor.IsEmpty)");
            sb.AppendLine("        {");
            sb.Append("            alvo.").Append(c.Nome).AppendLine(" = default;");
            sb.AppendLine("            return;");
            sb.AppendLine("        }");
        }
        sb.Append("        ").Append(under).Append(" bruto = ").Append(under)
            .AppendLine(".Parse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture);");
        sb.Append("        ").Append(c.TipoFq).Append(" convertido = (").Append(c.TipoFq).AppendLine(")bruto;");
        sb.AppendLine("        if (!Enum.IsDefined(convertido))");
        sb.Append("            throw new FormatException(\"Valor '\" + valor.ToString() + \"' não é válido para ")
            .Append(EscaparLiteral(c.TipoNome)).AppendLine(".\");");
        sb.Append("        alvo.").Append(c.Nome).AppendLine(" = convertido;");
    }

    /// <summary>
    /// Só enum fechado sem <c>[SpedValor]</c> e sem <c>[Flags]</c> tem domínio a validar: os
    /// enums textuais já rejeitam token desconhecido no setter permissivo, e um <c>[Flags]</c>
    /// não tem domínio fechado (combinações de bits são válidas mesmo sem membro nomeado) —
    /// espelha <c>CatalogoBuilder.PrecisaDefinidorEstrito</c>, que exclui os mesmos dois casos
    /// para que o catálogo gerado e o reflexivo concordem.
    /// </summary>
    private static bool PrecisaSetterEstrito(InfoCampo c)
        => c.Categoria == CategoriaCampo.Enum && !c.EnumFlags && c.EnumValoresSped.IsDefaultOrEmpty;

    /// <summary>
    /// Emite setter para enum cujos membros declaram <c>[SpedValor("...")]</c> — usado quando
    /// o leiaute representa o valor como texto (ex.: <c>"S"</c>/<c>"N"</c>) e não como inteiro.
    /// </summary>
    private static void EmitirSetterEnumTextual(StringBuilder sb, InfoCampo c)
    {
        sb.AppendLine("        if (valor.IsEmpty)");
        sb.AppendLine("        {");
        if (c.Nullable)
            sb.Append("            alvo.").Append(c.Nome).AppendLine(" = null;");
        else
            sb.Append("            alvo.").Append(c.Nome).AppendLine(" = default;");
        sb.AppendLine("            return;");
        sb.AppendLine("        }");

        foreach (string entrada in c.EnumValoresSped)
        {
            int sep = entrada.IndexOf('\0');
            string membro = entrada.Substring(0, sep);
            string valorSped = entrada.Substring(sep + 1);
            sb.Append("        if (valor.SequenceEqual(\"").Append(EscaparLiteral(valorSped)).Append("\".AsSpan()))");
            sb.AppendLine();
            sb.AppendLine("        {");
            sb.Append("            alvo.").Append(c.Nome).Append(" = ").Append(c.TipoFq).Append('.').Append(membro).AppendLine(";");
            sb.AppendLine("            return;");
            sb.AppendLine("        }");
        }

        sb.Append("        throw new FormatException(\"Valor '\" + valor.ToString() + \"' não é válido para ")
            .Append(EscaparLiteral(c.TipoNome)).AppendLine(".\");");
    }

    private static string EscaparLiteral(string valor) =>
        valor.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static void EmitirSetterValueObject(StringBuilder sb, InfoCampo c)
    {
        if (c.Nullable)
        {
            sb.Append("        if (valor.IsEmpty) { alvo.").Append(c.Nome).AppendLine(" = null; return; }");
            sb.Append("        alvo.").Append(c.Nome).Append(" = ").Append(c.TipoFq).AppendLine(".Create(valor);");
        }
        else
        {
            sb.Append("        alvo.").Append(c.Nome).Append(" = valor.IsEmpty ? default : ")
                .Append(c.TipoFq).AppendLine(".Create(valor);");
        }
    }

    private static void EmitirSetterFallbackReflexivo(StringBuilder sb, InfoCampo c)
    {
        // Fallback para tipos não cobertos pelo gerador: paga string-alloc + boxing via
        // ConversoresPrimitivosCatalogo. Deve ser raro — se aparece em produção, registrar
        // o conversor explicitamente ou estender o gerador.
        sb.AppendLine("        string texto = valor.IsEmpty ? string.Empty : valor.ToString();");
        sb.Append("        if (!ConversoresPrimitivosCatalogo.TentarObter(typeof(")
            .Append(c.TipoDeclaradoFq).AppendLine("), out var conv))");
        sb.Append("            throw new System.NotSupportedException(\"Conversor não registrado para tipo ")
            .Append(c.TipoFq).AppendLine(".\");");
        sb.AppendLine("        object? bruto = conv(texto);");
        sb.Append("        alvo.").Append(c.Nome).Append(" = (").Append(c.TipoDeclaradoFq).AppendLine(")bruto!;");
    }

    private static void EmitirHelperSer(StringBuilder sb, InfoRegistro reg, InfoCampo c)
    {
        sb.Append("    private static string Ser_").Append(reg.Codigo).Append('_').Append(c.Nome)
            .AppendLine("(RegistroSped registro)");
        sb.AppendLine("    {");
        sb.Append("        var alvo = (").Append(reg.TipoFullyQualified).AppendLine(")registro;");

        switch (c.Categoria)
        {
            case CategoriaCampo.StringCampo:
                sb.Append("        return alvo.").Append(c.Nome).AppendLine(" ?? string.Empty;");
                break;

            case CategoriaCampo.Int32:
            case CategoriaCampo.Int64:
            case CategoriaCampo.Int16:
                EmitirSerializadorPrimitivoNumerico(sb, c);
                break;

            case CategoriaCampo.Decimal:
                EmitirSerializadorDecimal(sb, c);
                break;

            case CategoriaCampo.DateOnly:
                EmitirSerializadorDateOnly(sb, c);
                break;

            case CategoriaCampo.Bool:
                EmitirSerializadorBool(sb, c);
                break;

            case CategoriaCampo.Char:
                EmitirSerializadorChar(sb, c);
                break;

            case CategoriaCampo.Enum:
                EmitirSerializadorEnum(sb, c);
                break;

            case CategoriaCampo.ValueObject:
                EmitirSerializadorValueObject(sb, c);
                break;

            case CategoriaCampo.NaoSuportado:
            default:
                sb.Append("        var v = alvo.").Append(c.Nome).AppendLine(";");
                sb.AppendLine("        return v is null ? string.Empty : v.ToString() ?? string.Empty;");
                break;
        }

        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void EmitirSerializadorPrimitivoNumerico(StringBuilder sb, InfoCampo c)
    {
        if (c.Nullable)
        {
            sb.Append("        var v = alvo.").Append(c.Nome).AppendLine(";");
            sb.AppendLine("        return v is null ? string.Empty : v.Value.ToString(CultureInfo.InvariantCulture);");
        }
        else
        {
            sb.Append("        return alvo.").Append(c.Nome).AppendLine(".ToString(CultureInfo.InvariantCulture);");
        }
    }

    private static void EmitirSerializadorDecimal(StringBuilder sb, InfoCampo c)
    {
        if (c.Nullable)
        {
            sb.Append("        var v = alvo.").Append(c.Nome).AppendLine(";");
            sb.Append("        return v is null ? string.Empty : SerializadoresPrimitivos.DeDecimal(v.Value, ")
                .Append(c.Decimais.ToString(CultureInfo.InvariantCulture)).AppendLine(");");
        }
        else
        {
            sb.Append("        return SerializadoresPrimitivos.DeDecimal(alvo.").Append(c.Nome)
                .Append(", ").Append(c.Decimais.ToString(CultureInfo.InvariantCulture)).AppendLine(");");
        }
    }

    private static void EmitirSerializadorDateOnly(StringBuilder sb, InfoCampo c)
    {
        string formato = c.Formato is null ? "null" : "\"" + c.Formato + "\"";
        if (c.Nullable)
        {
            sb.Append("        var v = alvo.").Append(c.Nome).AppendLine(";");
            sb.Append("        return v is null ? string.Empty : SerializadoresPrimitivos.DataComFormato(v.Value, ")
                .Append(formato).AppendLine(");");
        }
        else
        {
            sb.Append("        return SerializadoresPrimitivos.DataComFormato(alvo.").Append(c.Nome)
                .Append(", ").Append(formato).AppendLine(");");
        }
    }

    private static void EmitirSerializadorBool(StringBuilder sb, InfoCampo c)
    {
        if (c.Nullable)
        {
            sb.Append("        var v = alvo.").Append(c.Nome).AppendLine(";");
            sb.AppendLine("        return v is null ? string.Empty : (v.Value ? \"true\" : \"false\");");
        }
        else
        {
            sb.Append("        return alvo.").Append(c.Nome).AppendLine(" ? \"true\" : \"false\";");
        }
    }

    private static void EmitirSerializadorChar(StringBuilder sb, InfoCampo c)
    {
        if (c.Nullable)
        {
            sb.Append("        var v = alvo.").Append(c.Nome).AppendLine(";");
            sb.AppendLine("        return v is null ? string.Empty : v.Value.ToString();");
        }
        else
        {
            sb.Append("        return alvo.").Append(c.Nome).AppendLine(".ToString();");
        }
    }

    private static void EmitirSerializadorEnum(StringBuilder sb, InfoCampo c)
    {
        if (!c.EnumValoresSped.IsDefaultOrEmpty)
        {
            EmitirSerializadorEnumTextual(sb, c);
            return;
        }

        string under = c.UnderlyingPrimitivo ?? "int";
        string padFormat = c.Tamanho > 0
            ? ".PadLeft(" + c.Tamanho.ToString(CultureInfo.InvariantCulture) + ", '0')"
            : string.Empty;

        if (c.Nullable)
        {
            sb.Append("        var v = alvo.").Append(c.Nome).AppendLine(";");
            sb.AppendLine("        if (v is null) return string.Empty;");
            sb.Append("        return ((").Append(under).AppendLine(")v.Value).ToString(CultureInfo.InvariantCulture)" + padFormat + ";");
        }
        else
        {
            sb.Append("        return ((").Append(under).Append(")alvo.").Append(c.Nome).AppendLine(").ToString(CultureInfo.InvariantCulture)" + padFormat + ";");
        }
    }

    /// <summary>
    /// Emite serializador via switch para enum cujos membros declaram <c>[SpedValor("...")]</c>.
    /// </summary>
    private static void EmitirSerializadorEnumTextual(StringBuilder sb, InfoCampo c)
    {
        if (c.Nullable)
        {
            sb.Append("        var v = alvo.").Append(c.Nome).AppendLine(";");
            sb.AppendLine("        if (v is null) return string.Empty;");
            sb.AppendLine("        return v.Value switch");
        }
        else
        {
            sb.Append("        return alvo.").Append(c.Nome).AppendLine(" switch");
        }
        sb.AppendLine("        {");
        foreach (string entrada in c.EnumValoresSped)
        {
            int sep = entrada.IndexOf('\0');
            string membro = entrada.Substring(0, sep);
            string valorSped = entrada.Substring(sep + 1);
            sb.Append("            ").Append(c.TipoFq).Append('.').Append(membro)
                .Append(" => \"").Append(EscaparLiteral(valorSped)).AppendLine("\",");
        }
        sb.Append("            _ => throw new InvalidOperationException(\"Valor enum não mapeado em ")
            .Append(c.TipoFq).AppendLine(".\"),");
        sb.AppendLine("        };");
    }

    private static void EmitirSerializadorValueObject(StringBuilder sb, InfoCampo c)
    {
        if (c.Nullable)
        {
            sb.Append("        var v = alvo.").Append(c.Nome).AppendLine(";");
            sb.AppendLine("        return v is null ? string.Empty : v.Value.ToString() ?? string.Empty;");
        }
        else
        {
            sb.Append("        return alvo.").Append(c.Nome).AppendLine(".ToString() ?? string.Empty;");
        }
    }

    private enum CategoriaCampo
    {
        StringCampo,
        Int32,
        Int64,
        Int16,
        Decimal,
        DateOnly,
        Bool,
        Char,
        Enum,
        ValueObject,
        NaoSuportado,
    }

    private readonly record struct InfoCampo(
        string Nome,
        string FieldName,
        int Ordem,
        string TipoFq,
        string TipoNome,
        string TipoDeclaradoFq,
        CategoriaCampo Categoria,
        bool Nullable,
        string? UnderlyingPrimitivo,
        int Tamanho,
        int Decimais,
        bool Obrigatorio,
        string? Formato,
        int DesdeVersao,
        bool CapturaTudo,
        bool CampoArquivo,
        ImmutableArray<string> EnumValoresSped,
        bool EnumFlags);

    private readonly record struct InfoRegistro(
        string TipoFullyQualified,
        string Codigo,
        int Nivel,
        string Bloco,
        ImmutableArray<InfoCampo> Campos,
        int IntroduzidoEm,
        string? TokenFimArquivo,
        ImmutableArray<FieldError> Errors);

    private readonly record struct FieldCollectionResult(
        ImmutableArray<InfoCampo> Fields,
        ImmutableArray<FieldError> Errors);

    private readonly record struct FieldError(
        FieldErrorKind Kind,
        Location? Location,
        string Property,
        string Name);

    private enum FieldErrorKind
    {
        InvalidName,
        DuplicateName,
        NonIncreasingVigencia,
    }
}
