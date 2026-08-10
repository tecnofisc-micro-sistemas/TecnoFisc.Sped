using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoE;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoE;

public sealed class RegistroE990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroE990(), "E990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeDeLinhas()
    {
        var resultado = new ParserEcf().ParseLinha("|E990|8|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroE990>()
            .Which.QtdLin.Should().Be(8);
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        arquivo.BlocoE.Registros.Select(registro => registro.Codigo).Should().Equal(
            "E001", "E010", "E015", "E020", "E030", "E155", "E355", "E990");
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo).Should().Equal(
            "0000", "E001", "E010", "E015", "E020", "E030", "E155", "E355", "E990");

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoE.Registros;
        var e001 = (RegistroE001)registros[0];
        var e010 = (RegistroE010)registros[1];
        var e015 = (RegistroE015)registros[2];
        var e020 = (RegistroE020)registros[3];
        var e030 = (RegistroE030)registros[4];
        var e155 = (RegistroE155)registros[5];
        var e355 = (RegistroE355)registros[6];
        var e990 = (RegistroE990)registros[7];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, e001, e990);
        AssertRegistroEcf.ConformsToManifest(e001, "E001", "1:1", r0000, e010, e020, e030);
        AssertRegistroEcf.ConformsToManifest(e010, "E010", "0:N", e001, e015);
        AssertRegistroEcf.ConformsToManifest(e015, "E015", "0:N", e010);
        AssertRegistroEcf.ConformsToManifest(e020, "E020", "0:N", e001);
        AssertRegistroEcf.ConformsToManifest(e030, "E030", "0:13", e001, e155, e355);
        AssertRegistroEcf.ConformsToManifest(e155, "E155", "1:N", e030);
        AssertRegistroEcf.ConformsToManifest(e355, "E355", "0:N", e030);
        AssertRegistroEcf.ConformsToManifest(e990, "E990", "1:1", r0000);

        e001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        e010.CodNat.Should().Be("0101");
        e015.ValCta.Should().Be(750.25m);
        e020.DtApLal.Should().Be(new DateOnly(2024, 12, 31));
        e020.DtLimLal.Should().Be(new DateOnly(2029, 12, 31));
        e020.Tributo.Should().Be(IndicadorTributoParteB.Ambos);
        e020.CodPbRfb.Should().Be("000045");
        e030.PerApur.Should().Be("A01");
        e155.VlSldFin.Should().Be(250.50m);
        e355.CodCcus.Should().BeNull();
        e990.QtdLin.Should().Be(8);
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
        relido.BlocoE.Registros[3].Should().BeOfType<RegistroE020>()
            .Which.DtLimLal.Should().Be(new DateOnly(2029, 12, 31));
        relido.BlocoE.Registros[5].Should().BeOfType<RegistroE155>()
            .Which.VlSldFin.Should().Be(250.50m);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-e.txt");
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
