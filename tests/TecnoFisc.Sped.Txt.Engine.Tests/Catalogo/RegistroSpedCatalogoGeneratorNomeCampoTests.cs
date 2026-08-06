using System.Reflection;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.SourceGenerators;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Catalogo;

public sealed class RegistroSpedCatalogoGeneratorNomeCampoTests
{
    [Fact]
    public void AliasExplicito_CatalogosGeradoEReflexivoConcordamComMetadadoErroEWirePosicional()
    {
        const string source = """
            using TecnoFisc.Sped.Txt.Engine.Abstracoes;
            using TecnoFisc.Sped.Txt.Engine.Atributos;

            namespace AliasGerado;

            [RegistroSped(Codigo = "A100", Nivel = 1, Bloco = "A")]
            public sealed class RegistroA100 : RegistroSped
            {
                public override string Codigo => "A100";

                [CampoSped(Ordem = 2, Nome = "CODIGO")]
                public int CampoCodigo { get; set; }
            }
            """;

        Assembly assembly = CompilarComGerador(source, out var diagnosticos);
        diagnosticos.Should().NotContain(diagnostico => diagnostico.Severity == DiagnosticSeverity.Error);

        var gerado = CriarCatalogoGerado(assembly);
        var reflexivo = CatalogoBuilder.BuildFromAssembly(assembly);
        gerado.TentarObter("A100", out var metaGerado).Should().BeTrue();
        reflexivo.TentarObter("A100", out var metaReflexivo).Should().BeTrue();

        metaGerado!.Campos.Should().ContainSingle().Which.Nome.Should().Be("CODIGO");
        metaReflexivo!.Campos.Should().ContainSingle().Which.Nome.Should().Be("CODIGO");
        metaGerado.Campos[0].Tipo.Should().Be(metaReflexivo.Campos[0].Tipo);
        metaGerado.Campos[0].Ordem.Should().Be(metaReflexivo.Campos[0].Ordem);

        var registro = metaGerado.Fabrica();
        metaGerado.Campos[0].Definidor(registro, "0007");
        metaGerado.Campos[0].Serializar(registro).Should().Be("7");

        var invalido = new LeitorSpedTxt(gerado).ParseLinha("|A100|INVALIDO|");
        invalido.Sucesso.Should().BeTrue();
        invalido.Valor.ErrosDeFormato.Should().ContainSingle()
            .Which.Campo.Should().Be("CODIGO");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(" CODIGO ")]
    [InlineData("COD|IGO")]
    [InlineData("COD\nIGO")]
    [InlineData("1CODIGO")]
    public void AliasExplicitoInvalido_ProduzDiagnosticoSemEmitirCatalogo(string alias)
    {
        string source = FonteComCampos($"[CampoSped(Ordem = 2, Nome = {Literal(alias)})] public string? Campo {{ get; set; }}");

        _ = CompilarComGerador(source, out var diagnosticos, exigirEmissao: false);

        diagnosticos.Should().ContainSingle(diagnostico =>
            diagnostico.Id == "TFSPED001" && diagnostico.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void AliasEfetivoDuplicado_ProduzDiagnosticoSemEmitirCatalogo()
    {
        string source = FonteComCampos("""
            [CampoSped(Ordem = 2, Nome = "CODIGO")] public string? Primeiro { get; set; }
            [CampoSped(Ordem = 3, Nome = "CODIGO")] public string? Segundo { get; set; }
            """);

        _ = CompilarComGerador(source, out var diagnosticos, exigirEmissao: false);

        diagnosticos.Should().ContainSingle(diagnostico =>
            diagnostico.Id == "TFSPED002" && diagnostico.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void NomeNuloOuOmitido_PreservaNomeDaPropriedade()
    {
        string source = FonteComCampos("""
            [CampoSped(Ordem = 2, Nome = null)] public string? CampoNulo { get; set; }
            [CampoSped(Ordem = 3)] public string? CampoOmitido { get; set; }
            """);

        Assembly assembly = CompilarComGerador(source, out var diagnosticos);
        diagnosticos.Should().NotContain(diagnostico => diagnostico.Severity == DiagnosticSeverity.Error);

        var gerado = CriarCatalogoGerado(assembly);
        gerado.TentarObter("A100", out var metadados).Should().BeTrue();
        metadados!.Campos.Select(campo => campo.Nome).Should().Equal("CampoNulo", "CampoOmitido");
    }

    private static string FonteComCampos(string campos)
        => $$"""
            using TecnoFisc.Sped.Txt.Engine.Abstracoes;
            using TecnoFisc.Sped.Txt.Engine.Atributos;

            namespace AliasGerado;

            [RegistroSped(Codigo = "A100", Nivel = 1, Bloco = "A")]
            public sealed class RegistroA100 : RegistroSped
            {
                public override string Codigo => "A100";
                {{campos}}
            }
            """;

    private static string Literal(string valor)
        => "\"" + valor
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";

    private static Assembly CompilarComGerador(
        string source,
        out ImmutableArray<Diagnostic> diagnosticos,
        bool exigirEmissao = true)
    {
        string nomeAssembly = "AliasGerado" + Guid.NewGuid().ToString("N");
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var references = Referencias().ToArray();
        var compilation = CSharpCompilation.Create(
            nomeAssembly,
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        compilation.GetDiagnostics().Should().NotContain(diagnostico => diagnostico.Severity == DiagnosticSeverity.Error);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new RegistroSpedCatalogoGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var generatorDiagnostics);
        diagnosticos = generatorDiagnostics;

        if (!exigirEmissao || diagnosticos.Any(diagnostico => diagnostico.Severity == DiagnosticSeverity.Error))
            return typeof(RegistroSpedCatalogoGeneratorNomeCampoTests).Assembly;

        using var stream = new MemoryStream();
        var emit = output.Emit(stream);
        emit.Diagnostics.Should().NotContain(diagnostico => diagnostico.Severity == DiagnosticSeverity.Error);
        emit.Success.Should().BeTrue();
        return Assembly.Load(stream.ToArray());
    }

    private static IEnumerable<MetadataReference> Referencias()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        foreach (string caminho in trusted.Split(Path.PathSeparator))
            yield return MetadataReference.CreateFromFile(caminho);

        yield return MetadataReference.CreateFromFile(typeof(RegistroSped).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(Cnpj).Assembly.Location);
    }

    private static IRegistroSpedCatalogo CriarCatalogoGerado(Assembly assembly)
    {
        Type tipo = assembly.GetType($"{assembly.GetName().Name}.Generated.CatalogoSpedGerado")!;
        return (IRegistroSpedCatalogo)Activator.CreateInstance(tipo)!;
    }
}
