using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TecnoFisc.Sped.Core.SourceGenerators;

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
    private const string AttributeRegistro = "TecnoFisc.Sped.Core.Atributos.RegistroSpedAttribute";
    private const string AttributeCampo = "TecnoFisc.Sped.Core.Atributos.CampoSpedAttribute";

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

            string fonte = GerarCatalogo(lista, asmName);
            spc.AddSource("CatalogoSpedGerado.g.cs", SourceText.From(fonte, Encoding.UTF8));
        });
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
        foreach (KeyValuePair<string, TypedConstant> arg in atributo.NamedArguments)
        {
            switch (arg.Key)
            {
                case "Codigo": codigo = arg.Value.Value as string; break;
                case "Nivel": nivel = arg.Value.Value is int n ? n : 0; break;
                case "Bloco": bloco = arg.Value.Value as string; break;
                case "IntroduzidoEm": introduzidoEm = arg.Value.Value is int iv ? iv : 0; break;
            }
        }
        if (string.IsNullOrEmpty(codigo) || string.IsNullOrEmpty(bloco))
            return null;

        ImmutableArray<InfoCampo> campos = ColetarCampos(tipo);

        string tipoFq = tipo.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return new InfoRegistro(tipoFq, codigo!, nivel, bloco!, campos, introduzidoEm);
    }

    private static ImmutableArray<InfoCampo> ColetarCampos(INamedTypeSymbol tipoRegistro)
    {
        var lista = new List<InfoCampo>();

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
                string? formato = null;
                foreach (KeyValuePair<string, TypedConstant> arg in campoAttr.NamedArguments)
                {
                    switch (arg.Key)
                    {
                        case "Ordem": ordem = arg.Value.Value is int o ? o : 0; break;
                        case "Tamanho": tamanho = arg.Value.Value is int t ? t : 0; break;
                        case "Decimais": decimais = arg.Value.Value is int d ? d : 0; break;
                        case "Obrigatorio": obrigatorio = arg.Value.Value is bool b && b; break;
                        case "Formato": formato = arg.Value.Value as string; break;
                        case "DesdeVersao": desdeVersao = arg.Value.Value is int dv ? dv : 0; break;
                    }
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
                string tipoFqDeclarado = propriedade.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                lista.Add(new InfoCampo(
                    propriedade.Name,
                    ordem,
                    tipoFq,
                    tipoFqDeclarado,
                    categoria,
                    nullable,
                    underlyingPrimitivo,
                    tamanho,
                    decimais,
                    obrigatorio,
                    formato,
                    desdeVersao));
            }
        }

        return lista
            .GroupBy(c => c.Ordem)
            .Select(g => g.First())
            .OrderBy(c => c.Ordem)
            .ToImmutableArray();
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

        if (EhValueObjectConhecido(fq))
            return CategoriaCampo.ValueObject;

        return CategoriaCampo.NaoSuportado;
    }

    private static bool EhValueObjectConhecido(string fqName) => fqName switch
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
        sb.AppendLine("using TecnoFisc.Sped.Core.Abstracoes;");
        sb.AppendLine("using TecnoFisc.Sped.Core.Catalogo;");
        sb.AppendLine("using TecnoFisc.Sped.Core.Gerador;");
        sb.AppendLine("using TecnoFisc.Sped.Core.Parser;");
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

        ImmutableArray<InfoRegistro> ordenados = registros
            .OrderBy(static r => r.Codigo, System.StringComparer.Ordinal)
            .ToImmutableArray();

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
        sb.Append('"').Append(campo.Nome).Append("\", ");
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
        string under = c.UnderlyingPrimitivo ?? "int";
        if (c.Nullable)
        {
            sb.Append("        if (valor.IsEmpty) { alvo.").Append(c.Nome).AppendLine(" = null; return; }");
            sb.Append("        ").Append(under).Append(" bruto = ").Append(under)
                .AppendLine(".Parse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture);");
            sb.Append("        alvo.").Append(c.Nome).Append(" = (").Append(c.TipoFq).AppendLine(")bruto;");
        }
        else
        {
            sb.AppendLine("        if (valor.IsEmpty)");
            sb.AppendLine("        {");
            sb.Append("            alvo.").Append(c.Nome).AppendLine(" = default;");
            sb.AppendLine("            return;");
            sb.AppendLine("        }");
            sb.Append("        ").Append(under).Append(" bruto = ").Append(under)
                .AppendLine(".Parse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture);");
            sb.Append("        alvo.").Append(c.Nome).Append(" = (").Append(c.TipoFq).AppendLine(")bruto;");
        }
    }

    private static void EmitirSetterValueObject(StringBuilder sb, InfoCampo c)
    {
        if (c.Nullable)
        {
            sb.Append("        if (valor.IsEmpty) { alvo.").Append(c.Nome).AppendLine(" = null; return; }");
            sb.Append("        alvo.").Append(c.Nome).Append(" = ").Append(c.TipoFq).AppendLine(".Criar(valor);");
        }
        else
        {
            sb.Append("        alvo.").Append(c.Nome).Append(" = valor.IsEmpty ? default : ")
                .Append(c.TipoFq).AppendLine(".Criar(valor);");
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
        int Ordem,
        string TipoFq,
        string TipoDeclaradoFq,
        CategoriaCampo Categoria,
        bool Nullable,
        string? UnderlyingPrimitivo,
        int Tamanho,
        int Decimais,
        bool Obrigatorio,
        string? Formato,
        int DesdeVersao);

    private readonly record struct InfoRegistro(
        string TipoFullyQualified,
        string Codigo,
        int Nivel,
        string Bloco,
        ImmutableArray<InfoCampo> Campos,
        int IntroduzidoEm);
}
