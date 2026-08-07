using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.SourceGenerators;

namespace TecnoFisc.Sped.Txt.Engine.Tests.SourceGenerators;

/// <summary>
/// Harness compartilhado dos testes do <see cref="RegistroSpedCatalogoGenerator"/> que só
/// precisam inspecionar o texto C# emitido (sem compilar/carregar o assembly resultante).
/// Compare com <c>RegistroSpedCatalogoGeneratorNomeCampoTests.CompilarComGerador</c>, que emite
/// e carrega o assembly — usado quando o teste precisa executar o catálogo gerado.
/// </summary>
internal static class GeradorHarness
{
    /// <summary>
    /// Roda o gerador sobre <paramref name="fonte"/> e devolve o texto C# de todas as árvores
    /// geradas concatenado. Falha o teste (via FluentAssertions) se a compilação de entrada ou
    /// o próprio gerador reportarem diagnóstico de erro.
    /// </summary>
    internal static string ExecutarGerador(string fonte)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var syntaxTree = CSharpSyntaxTree.ParseText(fonte, parseOptions);
        var references = Referencias().ToArray();
        var compilation = CSharpCompilation.Create(
            "GeradorHarness" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        compilation.GetDiagnostics().Should().NotContain(diagnostico => diagnostico.Severity == DiagnosticSeverity.Error);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new RegistroSpedCatalogoGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out ImmutableArray<Diagnostic> diagnosticos);
        diagnosticos.Should().NotContain(diagnostico => diagnostico.Severity == DiagnosticSeverity.Error);

        GeneratorDriverRunResult resultado = driver.GetRunResult();
        return string.Join(Environment.NewLine, resultado.GeneratedTrees.Select(arvore => arvore.ToString()));
    }

    /// <summary>
    /// Variante de <see cref="ExecutarGerador"/> que devolve os diagnósticos do gerador junto com
    /// o texto gerado, sem falhar o teste quando há diagnóstico de erro — usada para provar que o
    /// gerador ainda emite <c>CatalogoSpedGerado.g.cs</c>/<c>RegistroSpedVisitor.g.cs</c> mesmo
    /// quando reporta um <c>TFSPED00x</c>.
    /// </summary>
    internal static (string Gerado, ImmutableArray<Diagnostic> Diagnosticos) ExecutarGeradorComDiagnosticos(string fonte)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var syntaxTree = CSharpSyntaxTree.ParseText(fonte, parseOptions);
        var references = Referencias().ToArray();
        var compilation = CSharpCompilation.Create(
            "GeradorHarness" + Guid.NewGuid().ToString("N"),
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        compilation.GetDiagnostics().Should().NotContain(diagnostico => diagnostico.Severity == DiagnosticSeverity.Error);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new RegistroSpedCatalogoGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out ImmutableArray<Diagnostic> diagnosticos);

        GeneratorDriverRunResult resultado = driver.GetRunResult();
        string gerado = string.Join(Environment.NewLine, resultado.GeneratedTrees.Select(arvore => arvore.ToString()));
        return (gerado, diagnosticos);
    }

    private static IEnumerable<MetadataReference> Referencias()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        foreach (string caminho in trusted.Split(Path.PathSeparator))
            yield return MetadataReference.CreateFromFile(caminho);

        yield return MetadataReference.CreateFromFile(typeof(RegistroSped).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(Cnpj).Assembly.Location);
    }
}
