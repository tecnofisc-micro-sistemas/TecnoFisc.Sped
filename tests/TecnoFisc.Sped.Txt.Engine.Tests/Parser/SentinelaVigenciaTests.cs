using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

/// <summary>
/// Prova que o descarte por vigência (<see cref="ReadingOptions.RespeitarVigenciaDoLeiaute"/>)
/// deixa de ser mudo: o registro barrado (e a subárvore cortada junto) vira
/// <see cref="RegistroNaoReconhecido"/> em vez de simplesmente sumir do stream
/// (achado 2 do PR 531).
/// </summary>
public sealed class SentinelaVigenciaTests
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

    private static async Task<List<RegistroSped>> ReadAsync(string conteudo)
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(typeof(RegistroVigenciaColunaSintetico).Assembly);
        var leitor = new LeitorSpedTxt(catalogo, new ReadingOptions { RespeitarVigenciaDoLeiaute = true });
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(conteudo));

        var lidos = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(stream).ConfigureAwait(false))
            lidos.Add(registro);
        return lidos;
    }

    [Fact]
    public async Task RegistroPosteriorAVersaoDeclarada_ViraSentinelaEmVezDeSumir()
    {
        var lidos = await ReadAsync(ArquivoSinteticoComRegistroFuturo);

        var sentinela = lidos.OfType<RegistroNaoReconhecido>().Should().ContainSingle().Which;
        sentinela.Codigo.Should().Be("A400");
        sentinela.Erro.Mensagem.Should().StartWith("Registro posterior à versão declarada no 0000");
        sentinela.LinhaCrua.Should().Contain("|A400|");
    }

    [Fact]
    public async Task SubarvoreCortada_TambemViraSentinela()
    {
        var lidos = await ReadAsync(ArquivoSinteticoComRegistroFuturoEFilho);

        lidos.OfType<RegistroNaoReconhecido>().Select(r => r.Codigo)
            .Should().BeEquivalentTo(["A400", "A410"]);
    }
}
