using System.Text;

using BenchmarkDotNet.Attributes;

using TecnoFisc.Sped.EfdContribuicoes;
using TecnoFisc.Sped.EfdContribuicoes.Parser;

namespace TecnoFisc.Sped.Benchmarks;

/// <summary>
/// Compara o consumo de memória entre <see cref="ParserEfdContribuicoes.LerStreamingAsync"/>
/// (memory-bounded) e <see cref="ParserEfdContribuicoes.LerAsync"/> (buffered, materializa
/// <see cref="ArquivoEfdContribuicoes"/> inteiro). O streaming deve manter alocações totais
/// proporcionais a <see cref="QtdRegistros"/> mas com working set/Gen2 estáveis; o buffered
/// retém cada registro vivo até o final, exibindo Gen pressure crescente com o tamanho.
/// </summary>
/// <remarks>
/// O fluxo sintético usa apenas registros do Bloco 9 (9001/9900/9990/9999) — suficientes para
/// exercitar pipeline, encoding Latin1, catálogo e leitor compartilhado do Core sem depender
/// de um arquivo real. Stage 5 §12: prova de memória constante para tamanho arbitrário.
/// </remarks>
[MemoryDiagnoser]
public class StreamingVsBufferedBenchmark
{
    private byte[] _bytesArquivo = [];

    /// <summary>Quantidade de registros 9900 no fluxo sintético.</summary>
    [Params(1_000, 10_000, 100_000)]
    public int QtdRegistros { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var sb = new StringBuilder();
        sb.Append("|9001|0|\r\n");
        for (int i = 0; i < QtdRegistros; i++)
        {
            sb.Append("|9900|0150|");
            sb.Append(i);
            sb.Append("|\r\n");
        }
        sb.Append("|9990|");
        sb.Append(QtdRegistros + 1);
        sb.Append("|\r\n");
        sb.Append("|9999|");
        sb.Append(QtdRegistros + 3);
        sb.Append("|\r\n");

        // Latin1 — encoding canônico dos arquivos SPED .txt.
        _bytesArquivo = Encoding.Latin1.GetBytes(sb.ToString());
    }

    [Benchmark(Description = "Streaming (descarta cada registro)")]
    public async Task<int> Streaming()
    {
        var parser = new ParserEfdContribuicoes();
        using var stream = new MemoryStream(_bytesArquivo, writable: false);
        int total = 0;
        await foreach (var _ in parser.LerStreamingAsync(stream))
            total++;
        return total;
    }

    [Benchmark(Baseline = true, Description = "Buffered (materializa Arquivo)")]
    public async Task<int> Buffered()
    {
        var parser = new ParserEfdContribuicoes();
        using var stream = new MemoryStream(_bytesArquivo, writable: false);
        var arquivo = await parser.LerAsync(stream);
        return arquivo.Bloco9.EnumerarRegistros().Count();
    }
}
