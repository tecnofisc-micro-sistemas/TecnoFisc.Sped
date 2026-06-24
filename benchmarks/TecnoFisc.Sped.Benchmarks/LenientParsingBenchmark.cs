using System.Text;
using BenchmarkDotNet.Attributes;
using TecnoFisc.Sped.EfdContribuicoes.Parser;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Benchmarks;

/// <summary>
/// Guarda de regressão de performance para o modo de parsing leniente
/// (<see cref="ReadingOptions.LenientFieldParsing"/>). Compara dois caminhos sobre o mesmo
/// arquivo sintético limpo (sem erros de formato):
/// <list type="bullet">
///   <item><see cref="Estrito"/> — baseline; lança na primeira falha de campo (comportamento padrão).</item>
///   <item><see cref="LenienteCaminhoFeliz"/> — arquivo limpo, nenhum erro acumulado;
///         a lista <c>ErrosDeFormato</c> permanece <c>null</c> (lazy), portanto a alocação extra
///         esperada é ~zero em relação ao caminho estrito.</item>
/// </list>
/// Um <c>Ratio</c> próximo de 1,00 e <c>Allocated</c> idêntico entre os dois métodos confirma
/// que a flag leniente não impõe overhead no caminho feliz.
/// </summary>
[MemoryDiagnoser]
public class LenientParsingBenchmark
{
    private byte[] _arquivo = [];

    /// <summary>Quantidade de registros C100 no arquivo sintético.</summary>
    [Params(2_000)]
    public int QtdC100 { get; set; }

    /// <summary>
    /// Monta em memória um arquivo EFD Contribuições sintético com <see cref="QtdC100"/> registros
    /// C100 de caminho feliz. Todos os campos presentes convertem sem erro em ambos os modos.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        // Registro 0000: COD_VER=006, indicadores e datas fixas, CNPJ válido (11222333000181).
        // Bloco C: C001 + N×C100 + C990.  Bloco 9: 9999.
        // qtdBlocoC = C001(1) + C100(QtdC100) + C990(1) = QtdC100 + 2
        // totalLinhas = 0000(1) + C001(1) + C100(QtdC100) + C990(1) + 9999(1) = QtdC100 + 4
        int qtdBlocoC = QtdC100 + 2;
        int totalLinhas = QtdC100 + 4;

        var sb = new StringBuilder(capacity: QtdC100 * 50 + 256);
        sb.Append("|0000|006|0|||01012025|31012025|EMPRESA LTDA|11222333000181|SP|3550308||00|2|\r\n");
        sb.Append("|C001|0|\r\n");
        for (int i = 1; i <= QtdC100; i++)
        {
            // C100: IndOper=0, IndEmit=1, CodPart=FORN, CodMod=55, CodSit=00, Ser=001, NumDoc={i}
            // A linha termina logo após NumDoc (campo 8); ChvNfe e campos posteriores ficam em
            // branco (opcional). Nenhum campo presente pode falhar a conversão.
            sb.Append("|C100|0|1|FORN|55|00|001|");
            sb.Append(i);
            sb.Append("|\r\n");
        }
        sb.Append('|').Append("C990").Append('|').Append(qtdBlocoC).Append("|\r\n");
        sb.Append('|').Append("9999").Append('|').Append(totalLinhas).Append("|\r\n");

        _arquivo = EncodingSped.Latin1.GetBytes(sb.ToString());
    }

    /// <summary>Caminho estrito (baseline): qualquer falha de campo lançaria exceção.</summary>
    [Benchmark(Baseline = true)]
    public async Task<int> Estrito() => await Contar(new ReadingOptions());

    /// <summary>
    /// Caminho leniente com arquivo limpo: nenhum erro é acumulado, lista permanece null.
    /// Overhead esperado ~zero em relação a <see cref="Estrito"/>.
    /// </summary>
    [Benchmark]
    public async Task<int> LenienteCaminhoFeliz() => await Contar(new ReadingOptions { LenientFieldParsing = true });

    private async Task<int> Contar(ReadingOptions opcoes)
    {
        var parser = new ParserEfdContribuicoes(opcoes);
        using var stream = new MemoryStream(_arquivo, writable: false);
        int n = 0;
        await foreach (var _ in parser.ReadStreamingAsync(stream))
            n++;
        return n;
    }
}
