using BenchmarkDotNet.Attributes;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.EfdContribuicoes.Generated;
using TecnoFisc.Sped.EfdContribuicoes.Parser;

namespace TecnoFisc.Sped.Benchmarks;

/// <summary>
/// Compara o caminho quente do parser entre as duas estratégias de catálogo: reflexivo
/// (<see cref="CatalogoBuilder"/>, com <c>Expression.Compile</c> + boxing por campo) e gerado
/// em compile-time (<see cref="CatalogoSpedGerado"/>, com helpers tipados inline). Mede tempo
/// e alocação de bytes durante o consumo streaming de um fluxo SPED sintético do Bloco 9.
/// </summary>
[MemoryDiagnoser]
public class ParserCatalogoBenchmark
{
    private byte[] _bytesArquivo = [];
    private IRegistroSpedCatalogo _catalogoReflexivo = null!;
    private IRegistroSpedCatalogo _catalogoGerado = null!;

    [Params(10_000, 100_000)]
    public int QtdRegistros { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _bytesArquivo = StreamingVsBufferedBenchmark.GerarArquivoSinteticoBloco9(QtdRegistros);
        _catalogoReflexivo = CatalogoBuilder.BuildFromAssembly(typeof(ParserEfdContribuicoes).Assembly);
        _catalogoGerado = new CatalogoSpedGerado();
    }

    [Benchmark(Baseline = true, Description = "Parser + catálogo reflexivo")]
    public async Task<int> Reflexivo()
    {
        var parser = new ParserEfdContribuicoes(_catalogoReflexivo);
        using var stream = new MemoryStream(_bytesArquivo, writable: false);
        int total = 0;
        await foreach (var _ in parser.ReadStreamingAsync(stream))
            total++;
        return total;
    }

    [Benchmark(Description = "Parser + catálogo gerado (compile-time)")]
    public async Task<int> Gerado()
    {
        var parser = new ParserEfdContribuicoes(_catalogoGerado);
        using var stream = new MemoryStream(_bytesArquivo, writable: false);
        int total = 0;
        await foreach (var _ in parser.ReadStreamingAsync(stream))
            total++;
        return total;
    }
}
