using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoJ;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoJ;

public sealed class RegistroJ990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroJ990(), "J990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeDeLinhas()
    {
        var resultado = new ParserEcf().ParseLinha("|J990|10|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroJ990>()
            .Which.QtdLin.Should().Be(10);
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_RepresentaCadeiaContabilIncrementalValida()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);
        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        var contas = arquivo.BlocoJ.Registros.OfType<RegistroJ050>().ToArray();
        contas.Select(conta => conta.Nível).Should().Equal(1, 2, 3, 4, 4);
        contas.Select(conta => conta.IndCta).Should().Equal(
            IndicadorTipoConta.Sintetica,
            IndicadorTipoConta.Sintetica,
            IndicadorTipoConta.Sintetica,
            IndicadorTipoConta.Analitica,
            IndicadorTipoConta.Analitica);
        contas.Select(conta => conta.CodCta).Should().Equal(
            "0001",
            "0001.01",
            "0001.01.01",
            "0001.01.01.001",
            "0001.01.01.002");
        contas.Select(conta => conta.CodCtaSup).Should().Equal(
            null,
            "0001",
            "0001.01",
            "0001.01.01",
            "0001.01.01");

        var contasPatrimoniaisAnaliticas = contas.Where(
            conta => conta.IndCta == IndicadorTipoConta.Analitica &&
                     conta.CodNat is CodigoNaturezaContaContabil.ContasDeAtivo or
                         CodigoNaturezaContaContabil.ContasDePassivo or
                         CodigoNaturezaContaContabil.PatrimonioLiquido);
        contasPatrimoniaisAnaliticas.Should().NotBeEmpty();
        contasPatrimoniaisAnaliticas.Should().OnlyContain(conta => conta.Nível >= 4);
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        arquivo.BlocoJ.Registros.Select(registro => registro.Codigo).Should().Equal(
            "J001", "J050", "J050", "J050", "J050", "J051", "J053", "J050", "J100", "J990");
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo).Should().Equal(
            "0000", "J001", "J050", "J050", "J050", "J050", "J051", "J053", "J050", "J100", "J990");

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoJ.Registros;
        var j001 = (RegistroJ001)registros[0];
        var j050Nivel1 = (RegistroJ050)registros[1];
        var j050Nivel2 = (RegistroJ050)registros[2];
        var j050Nivel3 = (RegistroJ050)registros[3];
        var j050Analitica = (RegistroJ050)registros[4];
        var j051 = (RegistroJ051)registros[5];
        var j053 = (RegistroJ053)registros[6];
        var j050Subconta = (RegistroJ050)registros[7];
        var j100 = (RegistroJ100)registros[8];
        var j990 = (RegistroJ990)registros[9];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, j001, j990);
        AssertRegistroEcf.ConformsToManifest(
            j001,
            "J001",
            "1:1",
            r0000,
            j050Nivel1,
            j050Nivel2,
            j050Nivel3,
            j050Analitica,
            j050Subconta,
            j100);
        AssertRegistroEcf.ConformsToManifest(j050Nivel1, "J050", "0:N", j001);
        AssertRegistroEcf.ConformsToManifest(j050Nivel2, "J050", "0:N", j001);
        AssertRegistroEcf.ConformsToManifest(j050Nivel3, "J050", "0:N", j001);
        AssertRegistroEcf.ConformsToManifest(j050Analitica, "J050", "0:N", j001, j051, j053);
        AssertRegistroEcf.ConformsToManifest(j051, "J051", "0:N", j050Analitica);
        AssertRegistroEcf.ConformsToManifest(j053, "J053", "0:N", j050Analitica);
        AssertRegistroEcf.ConformsToManifest(j050Subconta, "J050", "0:N", j001);
        AssertRegistroEcf.ConformsToManifest(j100, "J100", "0:N", j001);
        AssertRegistroEcf.ConformsToManifest(j990, "J990", "1:1", r0000);

        j001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        j050Nivel1.CodNat.Should().Be(CodigoNaturezaContaContabil.ContasDeAtivo);
        j050Nivel3.CodCtaSup.Should().Be("0001.01");
        j050Analitica.IndCta.Should().Be(IndicadorTipoConta.Analitica);
        j051.CodCcus.Should().Be("000001");
        j051.CodCtaRef.Should().Be("1.01.01.01.01");
        j053.CodIdt.Should().Be("000123");
        j053.CodCntCorr.Should().Be("0001.01.01.002");
        j053.NatSubCnt.Should().Be("02");
        j050Subconta.CodCta.Should().Be("0001.01.01.002");
        j100.DtAlt.Should().Be(new DateOnly(2025, 1, 1));
        j100.CodCcus.Should().Be("000001");
        j990.QtdLin.Should().Be(10);
    }

    [Fact]
    public async Task Writer_FixtureCompleta_PreservaTextoCanonicoEPermiteNovoParse()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);
        var parser = new ParserEcf();
        var arquivo = await parser.ReadAsync(entrada, TestContext.Current.CancellationToken);

        await using var saida = new MemoryStream();
        await new EscritorSpedTxt(new CatalogoSpedGerado()).WriteAsync(
            saida,
            arquivo.EnumerarRegistros(),
            TestContext.Current.CancellationToken);

        string esperado = EncodingSped.Latin1.GetString(bytes).Replace("\n", "\r\n");
        string serializado = EncodingSped.Latin1.GetString(saida.ToArray());
        serializado.Should().Be(esperado);

        saida.Position = 0;
        var relido = await parser.ReadAsync(saida, TestContext.Current.CancellationToken);
        relido.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(arquivo.EnumerarRegistros().Select(registro => registro.Codigo));
        relido.BlocoJ.Registros[4].Should().BeOfType<RegistroJ050>()
            .Which.CodCta.Should().Be("0001.01.01.001");
        relido.BlocoJ.Registros[6].Should().BeOfType<RegistroJ053>()
            .Which.CodIdt.Should().Be("000123");
        relido.BlocoJ.Registros[8].Should().BeOfType<RegistroJ100>()
            .Which.DtAlt.Should().Be(new DateOnly(2025, 1, 1));
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-j.txt");
        byte[] bytes = await File.ReadAllBytesAsync(
            caminho,
            TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);
        return bytes;
    }
}
