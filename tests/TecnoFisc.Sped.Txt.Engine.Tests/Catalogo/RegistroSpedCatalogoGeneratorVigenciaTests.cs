using System.Collections.Immutable;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.SourceGenerators;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Catalogo;

/// <summary>
/// Prova a invariante da qual o mapeamento posicional de <c>LeitorSpedTxt.InterpretarLinha</c>
/// depende (achado 4 do PR 531, revisão da task 5): <c>DesdeVersao</c> precisa ser não-decrescente
/// ao longo da posição dos campos — um campo versionado só pode ficar no fim do registro, nunca
/// seguido por um campo sempre presente (<c>DesdeVersao = 0</c>) ou por um campo de versão
/// anterior. Cobre os dois lados que precisam concordar (regra dos "três lugares" do catálogo):
/// <see cref="CatalogoBuilder"/> (reflexivo, lança em runtime) e
/// <see cref="RegistroSpedCatalogoGenerator"/> (compile-time, diagnóstico <c>TFSPED003</c>).
/// Também cobre a mesma paridade gerado × reflexivo para <c>DescontinuadoEm</c>
/// (<see cref="DescontinuadoEmDeclarado_CompilaSemDiagnosticoEAmbosCatalogosConcordam"/>) —
/// achado de follow-up do PR 531 sobre esta mesma classe de drift.
/// </summary>
public sealed class RegistroSpedCatalogoGeneratorVigenciaTests
{
    [Fact]
    public void DesdeVersaoNaoDecrescente_CompilaSemDiagnosticoEAmbosCatalogosConcordam()
    {
        string source = FonteComCampos("""
            [CampoSped(Ordem = 2)] public string? Antes { get; set; }
            [CampoSped(Ordem = 3, DesdeVersao = 5)] public string? Meio { get; set; }
            [CampoSped(Ordem = 4, DesdeVersao = 10)] public string? Fim { get; set; }
            """);

        Assembly assembly = CompilarComGerador(source, out var diagnosticos);
        diagnosticos.Should().NotContain(diagnostico => diagnostico.Severity == DiagnosticSeverity.Error);

        var gerado = CriarCatalogoGerado(assembly);
        var reflexivo = CatalogoBuilder.BuildFromAssembly(assembly);
        gerado.TentarObter("A100", out var metaGerado).Should().BeTrue();
        reflexivo.TentarObter("A100", out var metaReflexivo).Should().BeTrue();

        metaGerado!.Campos.Select(campo => campo.DesdeVersao).Should().Equal(0, 5, 10);
        metaReflexivo!.Campos.Select(campo => campo.DesdeVersao).Should().Equal(0, 5, 10);
    }

    /// <summary>
    /// Paridade gerado × reflexivo para <c>DescontinuadoEm</c> (achado de follow-up do PR 531): o
    /// <see cref="RegistroSpedCatalogoGenerator"/> passou a ler <c>[Descontinuado]</c> depois de
    /// um período em que só o <see cref="CatalogoBuilder"/> reflexivo lia, deixando o catálogo
    /// gerado — o que os parsers padrão realmente instanciam — com <c>DescontinuadoEm == 0</c> em
    /// produção (Registro0210/Registro1600 do EFD ICMS-IPI). A cobertura anterior desse bug era só
    /// indireta, via testes específicos do EFD ICMS-IPI; este teste prova a paridade na fonte, no
    /// próprio projeto do gerador, para qualquer registro futuro que declare o atributo.
    /// </summary>
    [Fact]
    public void DescontinuadoEmDeclarado_CompilaSemDiagnosticoEAmbosCatalogosConcordam()
    {
        string source = """
            using TecnoFisc.Sped.Core.Atributos;
            using TecnoFisc.Sped.Txt.Engine.Abstracoes;
            using TecnoFisc.Sped.Txt.Engine.Atributos;

            namespace VigenciaGerada;

            [RegistroSped(Codigo = "A100", Nivel = 1, Bloco = "A")]
            [Descontinuado(EmVersao = 11)]
            public sealed class RegistroA100 : RegistroSped
            {
                public override string Codigo => "A100";

                [CampoSped(Ordem = 2)] public string? Campo { get; set; }
            }
            """;

        Assembly assembly = CompilarComGerador(source, out var diagnosticos);
        diagnosticos.Should().NotContain(diagnostico => diagnostico.Severity == DiagnosticSeverity.Error);

        var gerado = CriarCatalogoGerado(assembly);
        var reflexivo = CatalogoBuilder.BuildFromAssembly(assembly);
        gerado.TentarObter("A100", out var metaGerado).Should().BeTrue();
        reflexivo.TentarObter("A100", out var metaReflexivo).Should().BeTrue();

        metaGerado!.DescontinuadoEm.Should().Be(11);
        metaReflexivo!.DescontinuadoEm.Should().Be(metaGerado.DescontinuadoEm);
    }

    [Fact]
    public void CampoSemprePresenteAposCampoVersionado_ProduzDiagnosticoTFSPED003SemEmitirCatalogo()
    {
        // Meio (DesdeVersao=5) seguido de Fim (DesdeVersao=0, "sempre presente") é exatamente o
        // desenho que a Task 5 reverteu: um arquivo de versão < 5 omitiria fisicamente a coluna
        // de Meio, e a coluna de Fim desalinharia em silêncio sob o mapeamento posicional.
        string source = FonteComCampos("""
            [CampoSped(Ordem = 2)] public string? Antes { get; set; }
            [CampoSped(Ordem = 3, DesdeVersao = 5)] public string? Meio { get; set; }
            [CampoSped(Ordem = 4)] public string? Fim { get; set; }
            """);

        Assembly assembly = CompilarComGerador(source, out var diagnosticos, exigirCatalogoGerado: false);

        diagnosticos.Should().ContainSingle(diagnostico =>
            diagnostico.Id == "TFSPED003" && diagnostico.Severity == DiagnosticSeverity.Error);

        var act = () => CatalogoBuilder.BuildFromAssembly(assembly);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*DesdeVersao*não-decrescente*");
    }

    [Fact]
    public void CampoComVersaoAnteriorAposCampoMaisRecente_ProduzDiagnosticoTFSPED003SemEmitirCatalogo()
    {
        // Duas colunas versionadas fora de ordem cronológica (15 antes de 10) também violam a
        // invariante: o Guia Prático só acrescenta colunas novas ao final de uma revisão, então
        // versões decrescentes ao longo da posição indicam erro de modelagem, não leiaute real.
        string source = FonteComCampos("""
            [CampoSped(Ordem = 2, DesdeVersao = 15)] public string? Recente { get; set; }
            [CampoSped(Ordem = 3, DesdeVersao = 10)] public string? Antigo { get; set; }
            """);

        Assembly assembly = CompilarComGerador(source, out var diagnosticos, exigirCatalogoGerado: false);

        diagnosticos.Should().ContainSingle(diagnostico =>
            diagnostico.Id == "TFSPED003" && diagnostico.Severity == DiagnosticSeverity.Error);

        var act = () => CatalogoBuilder.BuildFromAssembly(assembly);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*DesdeVersao*não-decrescente*");
    }

    private static string FonteComCampos(string campos)
        => $$"""
            using TecnoFisc.Sped.Txt.Engine.Abstracoes;
            using TecnoFisc.Sped.Txt.Engine.Atributos;

            namespace VigenciaGerada;

            [RegistroSped(Codigo = "A100", Nivel = 1, Bloco = "A")]
            public sealed class RegistroA100 : RegistroSped
            {
                public override string Codigo => "A100";
                {{campos}}
            }
            """;

    private static Assembly CompilarComGerador(
        string source,
        out ImmutableArray<Diagnostic> diagnosticos,
        bool exigirCatalogoGerado = true)
    {
        string nomeAssembly = "VigenciaGerada" + Guid.NewGuid().ToString("N");
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

        bool hasErrors = diagnosticos.Any(diagnostico => diagnostico.Severity == DiagnosticSeverity.Error);
        if (exigirCatalogoGerado && hasErrors)
            return typeof(RegistroSpedCatalogoGeneratorVigenciaTests).Assembly;

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
