using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

public sealed class LeitorSpedTxtLayoutLenienteTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000Sintetico).Assembly);

    private static MemoryStream FluxoSped(string conteudo)
        => new(EncodingSped.Latin1.GetBytes(conteudo));

    private static async Task<List<RegistroSped>> LerAsync(string conteudo, ReadingOptions opcoes)
    {
        var leitor = new LeitorSpedTxt(_catalogo, opcoes);
        var resultado = new List<RegistroSped>();
        await foreach (var r in leitor.ReadStreamingAsync(FluxoSped(conteudo)))
            resultado.Add(r);
        return resultado;
    }

    private static ReadingOptions LenienteLayout => new() { LenientLayout = true };

    [Fact]
    public async Task Estrito_CodigoDesconhecido_ContinuaLancandoErroLayout()
    {
        var act = async () => await LerAsync("|XXXX|1|\r\n", ReadingOptions.Default);

        var assercao = await act.Should().ThrowAsync<ErroLayoutSpedException>();
        assercao.Which.Erro.CodigoRegistro.Should().Be("XXXX");
    }

    [Fact]
    public async Task Leniente_CodigoDesconhecido_EmiteSentinelaECarregaLinhaCrua()
    {
        var registros = await LerAsync(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|XXXX|foo|bar|\r\n" +
            "|9999|3|\r\n", LenienteLayout);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "XXXX", "9999"]);

        var sentinela = registros.OfType<RegistroNaoReconhecido>().Single();
        sentinela.Codigo.Should().Be("XXXX");
        sentinela.LinhaCrua.Should().Be("|XXXX|foo|bar|");
        sentinela.Erro.CodigoRegistro.Should().Be("XXXX");
    }

    [Fact]
    public async Task Leniente_SentinelaEhFolha_NaoViraPaiDosSeguintes()
    {
        // O registro conhecido seguinte (C001) deve ancorar no pai real (0000), nao no sentinela.
        var registros = await LerAsync(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|XXXX|foo|\r\n" +
            "|C001|0|\r\n" +
            "|9999|4|\r\n", LenienteLayout);

        var r0000 = registros.OfType<Registro0000Sintetico>().Single();
        var sentinela = registros.OfType<RegistroNaoReconhecido>().Single();
        var c001 = registros.OfType<RegistroC001Sintetico>().Single();

        sentinela.Pai.Should().BeSameAs(r0000);     // pendurado como folha no topo vigente
        sentinela.Filhos.Should().BeEmpty();        // nunca recebe filhos
        c001.Pai.Should().BeSameAs(r0000);          // sentinela nao perturbou a hierarquia
    }
}
