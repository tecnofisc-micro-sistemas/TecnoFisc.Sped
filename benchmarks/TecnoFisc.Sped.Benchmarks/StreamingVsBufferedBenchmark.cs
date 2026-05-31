using System.Text;

using BenchmarkDotNet.Attributes;

using TecnoFisc.Sped.EfdContribuicoes;
using TecnoFisc.Sped.EfdContribuicoes.Parser;

namespace TecnoFisc.Sped.Benchmarks;

/// <summary>
/// Compara <see cref="ParserEfdContribuicoes.ReadStreamingAsync"/> (memory-bounded) com
/// <see cref="ParserEfdContribuicoes.ReadAsync"/> (buffered, materializa
/// <see cref="ArquivoEfdContribuicoes"/> inteiro).
/// </summary>
/// <remarks>
/// <para>
/// Atenção à interpretação da coluna <c>Allocated</c>: ela reporta o total de bytes alocados
/// no heap durante a operação, não o pico de memória viva. Como ambos os caminhos criam o
/// mesmo número de <see cref="TecnoFisc.Sped.Txt.Engine.Abstracoes.RegistroSped"/> (um por linha),
/// o total alocado fica praticamente idêntico — a única diferença é o overhead do
/// <see cref="ArquivoEfdContribuicoes"/> (dicionário de blocos + listas de filhos) no caminho
/// buffered.
/// </para>
/// <para>
/// O ganho real do streaming aparece em três métricas que esta tabela do BenchmarkDotNet expõe:
/// <list type="bullet">
///   <item><c>Gen2</c>: streaming mantém ~0 (objetos morrem em Gen0); buffered promove tudo para Gen2 enquanto retém o arquivo.</item>
///   <item><c>Gen1</c>: streaming mantém baixo; buffered cresce com <see cref="QtdRegistros"/>.</item>
///   <item>Peak working set (não medido aqui, ver <c>PeakHeapProbe</c>): streaming O(1), buffered O(N).</item>
/// </list>
/// Para tamanhos pequenos (≤100k registros 9900) a diferença em <c>Allocated</c> é apenas o
/// overhead do arquivo. Para tamanhos onde a economia importa de verdade — gigabytes de fluxo —
/// só o streaming termina sem estourar o heap.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class StreamingVsBufferedBenchmark
{
    private byte[] _bytesArquivo = [];

    /// <summary>Quantidade de registros 9900 no fluxo sintético.</summary>
    [Params(10_000, 100_000, 1_000_000)]
    public int QtdRegistros { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _bytesArquivo = GerarArquivoSinteticoBloco9(QtdRegistros);
    }

    [Benchmark(Description = "Streaming (descarta cada registro)")]
    public async Task<int> Streaming()
    {
        var parser = new ParserEfdContribuicoes();
        using var stream = new MemoryStream(_bytesArquivo, writable: false);
        int total = 0;
        await foreach (var _ in parser.ReadStreamingAsync(stream))
            total++;
        return total;
    }

    [Benchmark(Baseline = true, Description = "Buffered (materializa Arquivo)")]
    public async Task<int> Buffered()
    {
        var parser = new ParserEfdContribuicoes();
        using var stream = new MemoryStream(_bytesArquivo, writable: false);
        var arquivo = await parser.ReadAsync(stream);
        return arquivo.Bloco9.EnumerarRegistros().Count();
    }

    /// <summary>
    /// Gera um fluxo SPED sintético do Bloco 9 com <paramref name="qtdRegistros"/> linhas
    /// <c>9900</c> entre <c>9001</c> e <c>9990</c>/<c>9999</c>. Suficiente para exercitar
    /// PipeReader, encoding Latin1, catálogo e leitor sem depender de um arquivo real.
    /// </summary>
    internal static byte[] GerarArquivoSinteticoBloco9(int qtdRegistros)
    {
        var sb = new StringBuilder();
        sb.Append("|9001|0|\r\n");
        for (int i = 0; i < qtdRegistros; i++)
        {
            sb.Append("|9900|0150|");
            sb.Append(i);
            sb.Append("|\r\n");
        }
        sb.Append("|9990|");
        sb.Append(qtdRegistros + 1);
        sb.Append("|\r\n");
        sb.Append("|9999|");
        sb.Append(qtdRegistros + 3);
        sb.Append("|\r\n");

        return Encoding.Latin1.GetBytes(sb.ToString());
    }
}
