using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote2;

public sealed class RegistroM990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM990(), "M990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeExataDeLinhasDoBloco()
    {
        var resultado = new ParserEcf().ParseLinha("|M990|18|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM990>()
            .Which.QtdLin.Should().Be(18);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|M990|INVALIDA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM990>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroM990.QtdLin) && erro.ValorBruto == "INVALIDA");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemRoteamentoHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        string[] ordemBlocoM = [
            "M001", "M010", "M030", "M300", "M305", "M310", "M312", "M315",
            "M350", "M355", "M360", "M362", "M365", "M410", "M415", "M500",
            "M510", "M990",
        ];
        arquivo.BlocoM.Registros.Select(registro => registro.Codigo).Should().Equal(ordemBlocoM);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. ordemBlocoM]);

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoM.Registros;
        var m001 = (RegistroM001)registros[0];
        var m010 = (RegistroM010)registros[1];
        var m030 = (RegistroM030)registros[2];
        var m300 = (RegistroM300)registros[3];
        var m305 = (RegistroM305)registros[4];
        var m310 = (RegistroM310)registros[5];
        var m312 = (RegistroM312)registros[6];
        var m315 = (RegistroM315)registros[7];
        var m350 = (RegistroM350)registros[8];
        var m355 = (RegistroM355)registros[9];
        var m360 = (RegistroM360)registros[10];
        var m362 = (RegistroM362)registros[11];
        var m365 = (RegistroM365)registros[12];
        var m410 = (RegistroM410)registros[13];
        var m415 = (RegistroM415)registros[14];
        var m500 = (RegistroM500)registros[15];
        var m510 = (RegistroM510)registros[16];
        var m990 = (RegistroM990)registros[17];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, m001, m990);
        AssertRegistroEcf.ConformsToManifest(m001, "M001", "1:1", r0000, m010, m030);
        AssertRegistroEcf.ConformsToManifest(m010, "M010", "0:N", m001);
        AssertRegistroEcf.ConformsToManifest(m030, "M030", "0:13", m001, m300, m350, m410, m500, m510);
        AssertRegistroEcf.ConformsToManifest(m300, "M300", "1:N", m030, m305, m310, m315);
        AssertRegistroEcf.ConformsToManifest(m305, "M305", "0:N", m300);
        AssertRegistroEcf.ConformsToManifest(m310, "M310", "0:N", m300, m312);
        AssertRegistroEcf.ConformsToManifest(m312, "M312", "0:N", m310);
        AssertRegistroEcf.ConformsToManifest(m315, "M315", "0:N", m300);
        AssertRegistroEcf.ConformsToManifest(m350, "M350", "1:N", m030, m355, m360, m365);
        AssertRegistroEcf.ConformsToManifest(m355, "M355", "0:N", m350);
        AssertRegistroEcf.ConformsToManifest(m360, "M360", "0:N", m350, m362);
        AssertRegistroEcf.ConformsToManifest(m362, "M362", "0:N", m360);
        AssertRegistroEcf.ConformsToManifest(m365, "M365", "0:N", m350);
        AssertRegistroEcf.ConformsToManifest(m410, "M410", "0:N", m030, m415);
        AssertRegistroEcf.ConformsToManifest(m415, "M415", "0:N", m410);
        AssertRegistroEcf.ConformsToManifest(m500, "M500", "0:N", m030);
        AssertRegistroEcf.ConformsToManifest(m510, "M510", "0:N", m030);
        AssertRegistroEcf.ConformsToManifest(m990, "M990", "1:1", r0000);

        m001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        m010.DtApLal.Should().Be(new DateOnly(2018, 1, 1));
        m030.PerApur.Should().Be("T01");
        m300.CampoCodigo.Should().Be("0138");
        m300.Valor.Should().Be(-1000.25m);
        m312.NumLcto.Should().Be("0000012345");
        m315.NumProc.Should().Be("0001234-56.2025.4.01");
        m350.CampoCodigo.Should().Be("0138");
        m360.VlCta.Should().Be(-250.75m);
        m362.NumLcto.Should().Be("0000005678");
        m365.IndProc.Should().Be(TipoProcessoEcf.Administrativo);
        m410.IndValLanLalbPb.Should().Be(IndicadorLancamentoParteB.Credito);
        m410.IndLanAnt.Should().Be(IndicadorSimNao.Sim);
        m415.NumProc.Should().Be("0001111-22.2025.5.01");
        m500.IndVlLctoParteB.Should().Be(IndicadorDebitoCredito.Credor);
        m510.CodPbRfb.Should().Be("001005");
        m510.VlLctoParteB.Should().Be(-100m);
        m990.QtdLin.Should().Be(ordemBlocoM.Length);
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
        var registros = relido.BlocoM.Registros;
        registros[6].Should().BeOfType<RegistroM312>().Which.Pai.Should().BeSameAs(registros[5]);
        registros[11].Should().BeOfType<RegistroM362>().Which.Pai.Should().BeSameAs(registros[10]);
        registros[12].Should().BeOfType<RegistroM365>().Which.Pai.Should().BeSameAs(registros[8]);
        registros[14].Should().BeOfType<RegistroM415>().Which.Pai.Should().BeSameAs(registros[13]);
        registros[15].Should().BeOfType<RegistroM500>().Which.CodCtaB.Should().Be("000123");
        registros[16].Should().BeOfType<RegistroM510>().Which.CodPbRfb.Should().Be("001005");
        registros[17].Should().BeOfType<RegistroM990>().Which.QtdLin.Should().Be(18);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-m.txt");
        byte[] bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);
        return bytes;
    }
}
