using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoN;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoN.Lote2;

public sealed class RegistroN990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroN990(), "N990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeExataDeLinhasDoBloco()
    {
        var resultado = new ParserEcf().ParseLinha("|N990|13|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroN990>()
            .Which.QtdLin.Should().Be(13);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|N990|INVALIDA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroN990>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroN990.QtdLin) && erro.ValorBruto == "INVALIDA");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemRoteamentoHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        string[] ordemBlocoN = [
            "N001", "N030", "N500", "N600", "N605", "N610", "N615",
            "N620", "N630", "N650", "N660", "N670", "N990",
        ];
        arquivo.BlocoN.Registros.Select(registro => registro.Codigo).Should().Equal(ordemBlocoN);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. ordemBlocoN]);

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoN.Registros;
        var n001 = (RegistroN001)registros[0];
        var n030 = (RegistroN030)registros[1];
        var n500 = (RegistroN500)registros[2];
        var n600 = (RegistroN600)registros[3];
        var n605 = (RegistroN605)registros[4];
        var n610 = (RegistroN610)registros[5];
        var n615 = (RegistroN615)registros[6];
        var n620 = (RegistroN620)registros[7];
        var n630 = (RegistroN630)registros[8];
        var n650 = (RegistroN650)registros[9];
        var n660 = (RegistroN660)registros[10];
        var n670 = (RegistroN670)registros[11];
        var n990 = (RegistroN990)registros[12];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, n001, n990);
        AssertRegistroEcf.ConformsToManifest(n001, "N001", "1:1", r0000, n030);
        AssertRegistroEcf.ConformsToManifest(
            n030,
            "N030",
            "0:13",
            n001,
            n500,
            n600,
            n605,
            n610,
            n615,
            n620,
            n630,
            n650,
            n660,
            n670);
        AssertRegistroEcf.ConformsToManifest(n500, "N500", "1:13", n030);
        AssertRegistroEcf.ConformsToManifest(n600, "N600", "0:N", n030);
        AssertRegistroEcf.ConformsToManifest(n605, "N605", "0:N", n030);
        AssertRegistroEcf.ConformsToManifest(n610, "N610", "1:N", n030);
        AssertRegistroEcf.ConformsToManifest(n615, "N615", "1:1", n030);
        AssertRegistroEcf.ConformsToManifest(n620, "N620", "0:N", n030);
        AssertRegistroEcf.ConformsToManifest(n630, "N630", "0:N", n030);
        AssertRegistroEcf.ConformsToManifest(n650, "N650", "1:13", n030);
        AssertRegistroEcf.ConformsToManifest(n660, "N660", "1:N", n030);
        AssertRegistroEcf.ConformsToManifest(n670, "N670", "0:N", n030);
        AssertRegistroEcf.ConformsToManifest(n990, "N990", "1:1", r0000);

        n001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        n030.PerApur.Should().Be("T01");
        n500.CampoCodigo.Should().Be("0001");
        n605.Valor.Should().Be(-10000.25m);
        n615.PerIncenFinor.Should().Be(3m);
        n650.Valor.Should().Be(-500.25m);
        n660.CampoCodigo.Should().Be("0019");
        n660.Valor.Should().Be(10000m);
        n670.CampoCodigo.Should().Be("0023");
        n670.Valor.Should().Be(-10000m);
        n990.QtdLin.Should().Be(ordemBlocoN.Length);
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
        var registros = relido.BlocoN.Registros;
        registros[4].Should().BeOfType<RegistroN605>().Which.Pai.Should().BeSameAs(registros[1]);
        registros[9].Should().BeOfType<RegistroN650>().Which.Valor.Should().Be(-500.25m);
        registros[10].Should().BeOfType<RegistroN660>().Which.Pai.Should().BeSameAs(registros[1]);
        registros[11].Should().BeOfType<RegistroN670>().Which.Valor.Should().Be(-10000m);
        registros[12].Should().BeOfType<RegistroN990>().Which.QtdLin.Should().Be(13);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-n.txt");
        byte[] bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);
        return bytes;
    }
}
