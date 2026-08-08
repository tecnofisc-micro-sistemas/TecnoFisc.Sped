using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

/// <summary>
/// Prova que o filtro de <see cref="ReadingOptions.RegistrosIgnorados"/> e
/// <see cref="ReadingOptions.BlocosIgnorados"/> tem precedência sobre o gate de vigência
/// (<see cref="ReadingOptions.RespeitarVigenciaDoLeiaute"/>): um registro que o chamador pediu
/// para descartar não deve ser decodificado nem devolvido no stream como sentinela de vigência,
/// mesmo quando também seria descartado por estar fora da versão declarada no <c>0000</c>
/// (achado A do review final do PR 531).
/// </summary>
public sealed class VigenciaComFiltroTests
{
    private const string ArquivoSinteticoComRegistroFuturo =
        "|0000|010|01012025|31012025|EMPRESA|11222333000181|\r\n" +
        "|A400|desc|\r\n" +
        "|9999|2|\r\n";

    private const string ArquivoSinteticoComRegistroFuturoEFilho =
        "|0000|010|01012025|31012025|EMPRESA|11222333000181|\r\n" +
        "|A400|desc|\r\n" +
        "|A410|desc-filho|\r\n" +
        "|9999|3|\r\n";

    private static async Task<List<RegistroSped>> ReadAsync(string conteudo, ReadingOptions opcoes)
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(typeof(RegistroVigenciaColunaSintetico).Assembly);
        var leitor = new LeitorSpedTxt(catalogo, opcoes);
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(conteudo));

        var lidos = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(stream).ConfigureAwait(false))
            lidos.Add(registro);
        return lidos;
    }

    [Fact]
    public async Task RegistroIgnoradoPorCodigo_NaoViraSentinelaDeVigencia()
    {
        var opcoes = new ReadingOptions
        {
            RespeitarVigenciaDoLeiaute = true,
            RegistrosIgnorados = new HashSet<string>(StringComparer.Ordinal) { "A400" },
        };

        var lidos = await ReadAsync(ArquivoSinteticoComRegistroFuturo, opcoes);

        lidos.OfType<RegistroNaoReconhecido>().Should().BeEmpty();
        lidos.Select(r => r.Codigo).Should().NotContain("A400");
    }

    [Fact]
    public async Task BlocoIgnorado_NaoViraSentinelaDeVigenciaParaOsFilhos()
    {
        var opcoes = new ReadingOptions
        {
            RespeitarVigenciaDoLeiaute = true,
            BlocosIgnorados = new HashSet<string>(StringComparer.Ordinal) { "A" },
        };

        var lidos = await ReadAsync(ArquivoSinteticoComRegistroFuturoEFilho, opcoes);

        lidos.OfType<RegistroNaoReconhecido>().Should().BeEmpty();
        lidos.Select(r => r.Codigo).Should().NotContain("A400");
        lidos.Select(r => r.Codigo).Should().NotContain("A410");
    }
}
