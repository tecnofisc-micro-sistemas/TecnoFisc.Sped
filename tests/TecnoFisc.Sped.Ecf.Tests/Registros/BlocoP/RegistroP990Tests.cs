using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoP;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoP;

public sealed class RegistroP990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroP990(), "P990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeExataDeLinhasDoBloco()
    {
        var resultado = new ParserEcf().ParseLinha("|P990|16|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroP990>()
            .Which.QtdLin.Should().Be(16);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|P990|INVALIDA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroP990>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "QTD_LIN" && erro.ValorBruto == "INVALIDA");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemRoteamentoHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        string[] ordemBlocoP = [
            "P001", "P030", "P100", "P130", "P150", "P200", "P230", "P300",
            "P400", "P500", "P030", "P200", "P300", "P400", "P500", "P990",
        ];
        arquivo.BlocoP.Registros.Select(registro => registro.Codigo).Should().Equal(ordemBlocoP);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. ordemBlocoP]);

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoP.Registros;
        var p001 = (RegistroP001)registros[0];
        var p030T01 = (RegistroP030)registros[1];
        var p100 = (RegistroP100)registros[2];
        var p130 = (RegistroP130)registros[3];
        var p150 = (RegistroP150)registros[4];
        var p200T01 = (RegistroP200)registros[5];
        var p230 = (RegistroP230)registros[6];
        var p300T01 = (RegistroP300)registros[7];
        var p400T01 = (RegistroP400)registros[8];
        var p500T01 = (RegistroP500)registros[9];
        var p030T02 = (RegistroP030)registros[10];
        var p200T02 = (RegistroP200)registros[11];
        var p300T02 = (RegistroP300)registros[12];
        var p400T02 = (RegistroP400)registros[13];
        var p500T02 = (RegistroP500)registros[14];
        var p990 = (RegistroP990)registros[15];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, p001, p990);
        AssertRegistroEcf.ConformsToManifest(p001, "P001", "1:1", r0000, p030T01, p030T02);
        AssertRegistroEcf.ConformsToManifest(
            p030T01,
            "P030",
            "0:5",
            p001,
            p100,
            p130,
            p150,
            p200T01,
            p230,
            p300T01,
            p400T01,
            p500T01);
        AssertRegistroEcf.ConformsToManifest(p100, "P100", "0:N", p030T01);
        AssertRegistroEcf.ConformsToManifest(p130, "P130", "0:N", p030T01);
        AssertRegistroEcf.ConformsToManifest(p150, "P150", "0:N", p030T01);
        AssertRegistroEcf.ConformsToManifest(p200T01, "P200", "0:N", p030T01);
        AssertRegistroEcf.ConformsToManifest(p230, "P230", "0:N", p030T01);
        AssertRegistroEcf.ConformsToManifest(p300T01, "P300", "0:N", p030T01);
        AssertRegistroEcf.ConformsToManifest(p400T01, "P400", "0:N", p030T01);
        AssertRegistroEcf.ConformsToManifest(p500T01, "P500", "0:N", p030T01);
        AssertRegistroEcf.ConformsToManifest(
            p030T02,
            "P030",
            "0:5",
            p001,
            p200T02,
            p300T02,
            p400T02,
            p500T02);
        AssertRegistroEcf.ConformsToManifest(p200T02, "P200", "0:N", p030T02);
        AssertRegistroEcf.ConformsToManifest(p300T02, "P300", "0:N", p030T02);
        AssertRegistroEcf.ConformsToManifest(p400T02, "P400", "0:N", p030T02);
        AssertRegistroEcf.ConformsToManifest(p500T02, "P500", "0:N", p030T02);
        AssertRegistroEcf.ConformsToManifest(p990, "P990", "1:1", r0000);

        p001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        p030T01.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        p030T01.PerApur.Should().Be("T01");
        p100.Tipo.Should().Be(IndicadorTipoConta.Analitica);
        p100.ValCtaRefIni.Should().Be(10000m);
        p100.ValCtaRefFin.Should().Be("00020000,50");
        p130.Valor.Should().Be("000100000,00");
        p150.Valor.Should().Be(-10000.25m);
        p200T01.CampoCodigo.Should().Be("0025");
        p230.Valor.Should().Be("12,3400%");
        p300T01.Valor.Should().Be("-00010000,00");
        p400T01.Valor.Should().Be("00010000,00");
        p500T01.Valor.Should().Be("-0,00");
        p030T02.PerApur.Should().Be("T02");
        p200T02.Valor.Should().BeNull();
        p300T02.Valor.Should().Be("+100,0000");
        p400T02.Valor.Should().Be("9,0000%");
        p500T02.Valor.Should().BeNull();
        p990.QtdLin.Should().Be(ordemBlocoP.Length);
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
        var registros = relido.BlocoP.Registros;
        registros[2].Should().BeOfType<RegistroP100>().Which.Pai.Should().BeSameAs(registros[1]);
        registros[6].Should().BeOfType<RegistroP230>().Which.Valor.Should().Be("12,3400%");
        registros[11].Should().BeOfType<RegistroP200>().Which.Pai.Should().BeSameAs(registros[10]);
        registros[13].Should().BeOfType<RegistroP400>().Which.Valor.Should().Be("9,0000%");
        registros[15].Should().BeOfType<RegistroP990>().Which.QtdLin.Should().Be(16);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-p.txt");
        byte[] bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);
        return bytes;
    }
}
