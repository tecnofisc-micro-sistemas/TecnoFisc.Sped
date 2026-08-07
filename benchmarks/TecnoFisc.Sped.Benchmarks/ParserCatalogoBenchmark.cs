using BenchmarkDotNet.Attributes;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;
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

/// <summary>
/// Extensão do <see cref="ParserCatalogoBenchmark"/> (achado 9, PR #531): a assinatura do
/// <c>Definidor</c> gerado ganhou um parâmetro <c>bool validarDominioDeEnum</c>, propagado de
/// <see cref="LeitorSpedTxt"/> a <b>cada</b> campo — não só nos de enum. Classe separada porque
/// BenchmarkDotNet permite só um <c>Baseline</c> por classe (mesma restrição que levou
/// <see cref="ParserCatalogoBenchmark"/> a não misturar Reflexivo/Gerado com este par); mede
/// sobre o mesmo arquivo sintético do Bloco 9 (nenhum campo ali é enum fechado sem
/// <c>[SpedValor]</c>), isolando o custo de threading do parâmetro do custo do check em si.
/// O caminho desligado é o baseline: é o que os três pacotes já publicados no nuget.org exercitam
/// hoje (só o ECF liga por padrão — ver <c>ParserEcf.ResolverOpcoes</c>).
/// </summary>
[MemoryDiagnoser]
public class ValidacaoDominioEnumCatalogoBenchmark
{
    private byte[] _bytesArquivo = [];
    private IRegistroSpedCatalogo _catalogoGerado = null!;

    [Params(10_000, 100_000)]
    public int QtdRegistros { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _bytesArquivo = StreamingVsBufferedBenchmark.GerarArquivoSinteticoBloco9(QtdRegistros);
        _catalogoGerado = new CatalogoSpedGerado();
    }

    [Benchmark(Baseline = true, Description = "Sem validação de domínio de enum")]
    public async Task<int> SemValidacaoDeDominio()
        => await ContarAsync(new ReadingOptions { ValidarDominioDeEnum = false });

    [Benchmark(Description = "Com validação de domínio de enum")]
    public async Task<int> ComValidacaoDeDominio()
        => await ContarAsync(new ReadingOptions { ValidarDominioDeEnum = true });

    private async Task<int> ContarAsync(ReadingOptions opcoes)
    {
        var parser = new ParserEfdContribuicoes(_catalogoGerado, opcoes);
        using var stream = new MemoryStream(_bytesArquivo, writable: false);
        int total = 0;
        await foreach (var _ in parser.ReadStreamingAsync(stream))
            total++;
        return total;
    }
}
