using System.Text;
using BenchmarkDotNet.Attributes;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Benchmarks;

/// <summary>
/// Guarda de regressão de performance para a captura de <c>ColunasNaoModeladas</c> (achados 2 e 8,
/// PR #531). O ponto é a baseline: <see cref="SemColunaExcedente"/> é o caminho de produção dos
/// quatro leiautes — um arquivo do leiaute modelado, sem coluna excedente — e não pode pagar nada
/// pela captura, porque a condição <c>if</c> já era avaliada antes; só o ramo vazio mudou.
/// <see cref="ComColunaExcedente"/> mede o custo quando há algo a preservar: uma lista por
/// registro mais um <c>string</c> por coluna, proporcional ao dado que antes era descartado.
/// </summary>
[MemoryDiagnoser]
public class ColunasNaoModeladasBenchmark
{
    private byte[] _semExcedente = null!;
    private byte[] _comExcedente = null!;

    [GlobalSetup]
    public void Setup()
    {
        _semExcedente = MontarArquivoEcf(registros: 10_000, colunasExcedentes: 0);
        _comExcedente = MontarArquivoEcf(registros: 10_000, colunasExcedentes: 5);
    }

    [Benchmark(Baseline = true)]
    public async Task<int> SemColunaExcedente() => await ContarAsync(_semExcedente);

    [Benchmark]
    public async Task<int> ComColunaExcedente() => await ContarAsync(_comExcedente);

    private static async Task<int> ContarAsync(byte[] arquivo)
    {
        var parser = new ParserEcf(new ReadingOptions { RespeitarVigenciaDoLeiaute = true });
        using var stream = new MemoryStream(arquivo, writable: false);
        int n = 0;
        await foreach (var _ in parser.ReadStreamingAsync(stream))
            n++;
        return n;
    }

    /// <summary>
    /// Arquivo ECF sintético do leiaute 12 com <paramref name="registros"/> linhas X450. O X450
    /// modela um único campo (PAIS), então cada coluna acrescentada além dele é exatamente uma
    /// <c>ColunaNaoModelada</c> com motivo <c>AlemDoModelo</c>.
    /// </summary>
    private static byte[] MontarArquivoEcf(int registros, int colunasExcedentes)
    {
        // Bloco X: X001(1) + X450(N) + X990(1) = N + 2
        // Arquivo: 0000(1) + X001(1) + X450(N) + X990(1) + 9999(1) = N + 4
        int qtdBlocoX = registros + 2;
        int totalLinhas = registros + 4;

        var excedente = new StringBuilder();
        for (int c = 0; c < colunasExcedentes; c++)
            excedente.Append("COLUNA EXCEDENTE|");

        var sb = new StringBuilder(capacity: registros * 60 + 256);
        sb.Append("|0000|LECF|0012|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\r\n");
        sb.Append("|X001|0|\r\n");
        for (int i = 0; i < registros; i++)
            sb.Append("|X450|249|").Append(excedente).Append("\r\n");
        sb.Append("|X990|").Append(qtdBlocoX).Append("|\r\n");
        sb.Append("|9999|").Append(totalLinhas).Append("|\r\n");

        return EncodingSped.Latin1.GetBytes(sb.ToString());
    }
}
