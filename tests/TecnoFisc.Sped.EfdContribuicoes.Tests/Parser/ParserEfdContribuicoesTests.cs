using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdContribuicoes.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Parser;

/// <summary>
/// Smoke do <see cref="ParserEfdContribuicoes"/>: fachada do leiaute V006 que monta o
/// catálogo de EFD Contribuições e delega ao <see cref="LeitorSpedTxt"/> do Core.
/// </summary>
public sealed class ParserEfdContribuicoesTests
{
    private static MemoryStream Fluxo(string conteudo)
        => new(EncodingSped.Latin1.GetBytes(conteudo));

    private static async Task<List<RegistroSped>> LerTodosAsync(
        ParserEfdContribuicoes parser, string sped, CancellationToken cancelamento)
    {
        var registros = new List<RegistroSped>();
        await foreach (var registro in parser.LerStreamingAsync(Fluxo(sped), cancelamento))
            registros.Add(registro);
        return registros;
    }

    [Fact]
    public async Task LerStreamingAsync_ConstrutorPadrao_UsaCatalogoDoLeiauteEfdContribuicoes()
    {
        var parser = new ParserEfdContribuicoes();
        const string sped =
            "|0001|0|\r\n" +
            "|0990|2|\r\n" +
            "|9001|0|\r\n" +
            "|9999|4|\r\n";

        var registros = await LerTodosAsync(parser, sped, TestContext.Current.CancellationToken);

        registros.Should().HaveCount(4);
        registros[0].Should().BeOfType<Registro0001>();
        registros[1].Should().BeOfType<Registro0990>();
        registros[2].Should().BeOfType<Registro9001>();
        registros[3].Should().BeOfType<Registro9999>();
    }

    [Fact]
    public async Task LerStreamingAsync_PreencheCamposTipadosDoCatalogo()
    {
        var parser = new ParserEfdContribuicoes();
        const string sped =
            "|0001|0|\r\n" +
            "|9999|2|\r\n";

        var registros = await LerTodosAsync(parser, sped, TestContext.Current.CancellationToken);

        registros.OfType<Registro0001>().Single().IndMov
            .Should().Be(IndicadorMovimentoBloco.ComDados);
        registros.OfType<Registro9999>().Single().QtdLin.Should().Be(2);
    }

    [Fact]
    public async Task LerStreamingAsync_VinculaPaiEFilhosConformeNiveisDoLeiaute()
    {
        var parser = new ParserEfdContribuicoes();
        const string sped =
            "|9001|0|\r\n" +
            "|9900|0000|1|\r\n" +
            "|9900|9999|1|\r\n";

        var registros = await LerTodosAsync(parser, sped, TestContext.Current.CancellationToken);

        var abertura = registros.OfType<Registro9001>().Single();
        var totalizadores = registros.OfType<Registro9900>().ToList();
        totalizadores.Should().HaveCount(2);
        totalizadores.Should().AllSatisfy(t => t.Pai.Should().BeSameAs(abertura));
        abertura.Filhos.Should().Equal(totalizadores);
    }

    [Fact]
    public async Task LerStreamingAsync_QuandoCodigoForaDoLeiaute_LancaErroLayout()
    {
        var parser = new ParserEfdContribuicoes();
        const string sped = "|ZZZZ|1|\r\n";

        var act = async () => await LerTodosAsync(parser, sped, TestContext.Current.CancellationToken);

        var assercao = await act.Should().ThrowAsync<ErroLayoutSpedException>();
        assercao.Which.Erro.CodigoRegistro.Should().Be("ZZZZ");
    }

    [Fact]
    public void Construtor_QuandoCatalogoNulo_LancaArgumentNullException()
    {
        var act = () => new ParserEfdContribuicoes(catalogo: null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task LerStreamingAsync_ComCatalogoCustomizado_DelegaParaCatalogoInjetado()
    {
        // O catálogo do assembly Core de testes não conhece os registros de EFD Contribuições;
        // se o parser usar este catálogo (em vez do padrão), "0001" deve ser desconhecido.
        var catalogoCore = CatalogoBuilder.BuildFromAssembly(typeof(LeitorSpedTxt).Assembly);
        var parser = new ParserEfdContribuicoes(catalogoCore);
        const string sped = "|0001|0|\r\n";

        var act = async () => await LerTodosAsync(parser, sped, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ErroLayoutSpedException>();
    }

    [Fact]
    public async Task LerAsync_BufferedConvenience_RetornaArquivoComBlocosPopulados()
    {
        var parser = new ParserEfdContribuicoes();
        const string sped =
            "|0001|0|\r\n" +
            "|0990|2|\r\n" +
            "|9001|0|\r\n" +
            "|9900|0000|1|\r\n" +
            "|9990|2|\r\n" +
            "|9999|6|\r\n";

        var arquivo = await parser.LerAsync(Fluxo(sped), TestContext.Current.CancellationToken);

        arquivo.Bloco0.EnumerarRegistros().Should().HaveCount(2);
        arquivo.Bloco9.EnumerarRegistros().Should().HaveCount(4);
        arquivo.EnumerarRegistros().Select(r => r.Codigo)
            .Should().Equal("0001", "0990", "9001", "9900", "9990", "9999");
    }

    [Fact]
    public async Task LerAsync_BufferedConvenience_PreservaVinculacaoPaiEFilhos()
    {
        var parser = new ParserEfdContribuicoes();
        const string sped =
            "|9001|0|\r\n" +
            "|9900|0000|1|\r\n" +
            "|9900|9999|1|\r\n";

        var arquivo = await parser.LerAsync(Fluxo(sped), TestContext.Current.CancellationToken);

        var abertura = arquivo.Bloco9.EnumerarRegistros().OfType<Registro9001>().Single();
        var totalizadores = arquivo.Bloco9.EnumerarRegistros().OfType<Registro9900>().ToList();
        totalizadores.Should().HaveCount(2);
        totalizadores.Should().AllSatisfy(t => t.Pai.Should().BeSameAs(abertura));
    }
}
