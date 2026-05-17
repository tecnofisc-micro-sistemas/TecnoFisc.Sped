using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Gerador;
using TecnoFisc.Sped.EfdIcmsIpi.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco9;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Gerador;

/// <summary>
/// Smoke do <see cref="GeradorEfdIcmsIpi"/>: fachada do leiaute V015 que escreve
/// registros declarados no assembly de EFD ICMS-IPI.
/// </summary>
public sealed class GeradorEfdIcmsIpiTests
{
    private static async Task<string> EscreverAsync(
        GeradorEfdIcmsIpi gerador, IEnumerable<RegistroSped> registros, CancellationToken cancelamento)
    {
        using var saida = new MemoryStream();
        await gerador.EscreverAsync(saida, registros, cancelamento);
        return EncodingSped.Latin1.GetString(saida.ToArray());
    }

    [Fact]
    public async Task EscreverAsync_RegistrosDoLeiaute_GravaLinhasPipeDelimitadas()
    {
        var gerador = new GeradorEfdIcmsIpi();
        var registros = new RegistroSped[]
        {
            new Registro0001 { IndMov = IndicadorMovimentoBloco.ComDados },
            new Registro0990 { QtdLin0 = 2 },
            new Registro9001 { IndMov = IndicadorMovimentoBloco.ComDados },
            new Registro9999 { QtdLin = 4 },
        };

        var texto = await EscreverAsync(gerador, registros, TestContext.Current.CancellationToken);

        texto.Should().Be(
            "|0001|0|\r\n" +
            "|0990|2|\r\n" +
            "|9001|0|\r\n" +
            "|9999|4|\r\n");
    }

    [Fact]
    public async Task RoundTrip_ParserGerador_PreservaArquivoOriginal()
    {
        const string sped =
            "|0001|0|\r\n" +
            "|9001|0|\r\n" +
            "|9900|0000|1|\r\n" +
            "|9900|9999|1|\r\n" +
            "|9990|3|\r\n" +
            "|9999|5|\r\n";

        var parser = new ParserEfdIcmsIpi();
        var gerador = new GeradorEfdIcmsIpi();

        var registros = new List<RegistroSped>();
        using (var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped)))
            await foreach (var r in parser.LerStreamingAsync(entrada, TestContext.Current.CancellationToken))
                registros.Add(r);

        var saida = await EscreverAsync(gerador, registros, TestContext.Current.CancellationToken);

        saida.Should().Be(sped);
    }

    [Fact]
    public async Task EscreverAsync_RegistroForaDoLeiaute_LancaInvalidOperationException()
    {
        var gerador = new GeradorEfdIcmsIpi();
        var registros = new RegistroSped[] { new RegistroDesconhecido() };

        var act = async () => await EscreverAsync(gerador, registros, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void Construtor_QuandoCatalogoNulo_LancaArgumentNullException()
    {
        var act = () => new GeradorEfdIcmsIpi(catalogo: null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task EscreverAsync_ComCatalogoCustomizado_DelegaParaCatalogoInjetado()
    {
        var catalogoCore = CatalogoBuilder.BuildFromAssembly(typeof(LeitorSpedTxt).Assembly);
        var gerador = new GeradorEfdIcmsIpi(catalogoCore);
        var registros = new RegistroSped[] { new Registro0001 { IndMov = IndicadorMovimentoBloco.ComDados } };

        var act = async () => await EscreverAsync(gerador, registros, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private sealed class RegistroDesconhecido : RegistroSped
    {
        public override string Codigo => "ZZZZ";
    }
}
