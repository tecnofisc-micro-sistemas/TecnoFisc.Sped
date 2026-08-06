using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoC;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoC;

public sealed class RegistroC990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroC990(), "C990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeDeLinhas()
    {
        var resultado = new ParserEcf().ParseLinha("|C990|12|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroC990>()
            .Which.QtdLin.Should().Be(12);
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        arquivo.BlocoC.Registros.Select(registro => registro.Codigo).Should().Equal(
            "C001", "C040", "C050", "C051", "C053", "C100",
            "C150", "C155", "C157", "C350", "C355", "C990");
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo).Should().Equal(
            "0000", "C001", "C040", "C050", "C051", "C053", "C100",
            "C150", "C155", "C157", "C350", "C355", "C990");

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoC.Registros;
        var c001 = (RegistroC001)registros[0];
        var c040 = (RegistroC040)registros[1];
        var c050 = (RegistroC050)registros[2];
        var c051 = (RegistroC051)registros[3];
        var c053 = (RegistroC053)registros[4];
        var c100 = (RegistroC100)registros[5];
        var c150 = (RegistroC150)registros[6];
        var c155 = (RegistroC155)registros[7];
        var c157 = (RegistroC157)registros[8];
        var c350 = (RegistroC350)registros[9];
        var c355 = (RegistroC355)registros[10];
        var c990 = (RegistroC990)registros[11];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, c001, c990);
        AssertRegistroEcf.ConformsToManifest(c001, "C001", "1:1", r0000, c040);
        AssertRegistroEcf.ConformsToManifest(c040, "C040", "0:12", c001, c050, c100, c150, c350);
        AssertRegistroEcf.ConformsToManifest(c050, "C050", "0:N", c040, c051, c053);
        AssertRegistroEcf.ConformsToManifest(c051, "C051", "0:N", c050);
        AssertRegistroEcf.ConformsToManifest(c053, "C053", "0:N", c050);
        AssertRegistroEcf.ConformsToManifest(c100, "C100", "0:N", c040);
        AssertRegistroEcf.ConformsToManifest(c150, "C150", "0:12", c040, c155);
        AssertRegistroEcf.ConformsToManifest(c155, "C155", "1:N", c150, c157);
        AssertRegistroEcf.ConformsToManifest(c157, "C157", "1:N", c155);
        AssertRegistroEcf.ConformsToManifest(c350, "C350", "0:N", c040, c355);
        AssertRegistroEcf.ConformsToManifest(c355, "C355", "0:N", c350);
        AssertRegistroEcf.ConformsToManifest(c990, "C990", "1:1", r0000);

        c001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        c040.Nire.Should().Be("00123456789");
        c050.CodNat.Should().Be(CodigoNaturezaContaContabil.ContasDeAtivo);
        c053.CodIdt.Should().Be("000123");
        c155.VlSldIni.Should().Be(1000.25m);
        c157.IndVlSldFin.Should().Be(IndicadorDebitoCredito.Credor);
        c355.VlCta.Should().Be(2500m);
        c990.QtdLin.Should().Be(12);
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
        relido.BlocoC.Registros[7].Should().BeOfType<RegistroC155>()
            .Which.VlSldFin.Should().Be(1150.50m);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-c.txt");
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
