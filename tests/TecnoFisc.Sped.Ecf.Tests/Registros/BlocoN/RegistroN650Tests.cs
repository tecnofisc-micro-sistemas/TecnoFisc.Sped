using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoN;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoN.Lote1;

public sealed class RegistroN650Tests
{
    private static readonly string TextoLote = string.Join('\n',
        "|0000|LECF|0012|11111111000191|EMPRESA SINTETICA|0|0|||01012025|31122025|N||0||",
        "|N001|0|",
        "|N030|01012025|31032025|T01|",
        "|N500|0001|BASE DO IRPJ|-00123,4500|",
        "|N600|0066|PARCELA DAS DEMAIS ATIVIDADES|10000,00|",
        "|N605|000111||-10000,25|D|",
        "|N610|0077|REDUCAO POR REINVESTIMENTO|10000,00|",
        "|N615|2000,00|3,0000|-999,99|1,2500|7,50|-12,34|",
        "|N620|0007|OPERACOES DE CARATER CULTURAL|10000,00|",
        "|N630|0021|IRPJ MENSAL|-10000,00|",
        "|N650|0001|BASE DA CSLL|-500,25|",
        string.Empty);

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroN650(), "N650", "1:13");
    }

    [Fact]
    public void Parser_LeCodigoDescricaoEValorDecimalOpcionalComSinal()
    {
        var resultado = new ParserEcf().ParseLinha("|N650|0001|BASE DA CSLL|-500,25|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN650>().Which;
        registro.CampoCodigo.Should().Be("0001");
        registro.Descricao.Should().Be("BASE DA CSLL");
        registro.Valor.Should().Be(-500.25m);
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|N650|0001|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN650>().Which;
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }

    [Fact]
    public async Task ReadAsync_LoteMaterializaOrdemEHierarquiaComPaisEFilhosExplicitos()
    {
        await using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(TextoLote));

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        arquivo.BlocoN.Registros.Select(registro => registro.Codigo).Should().Equal(
            "N001", "N030", "N500", "N600", "N605", "N610", "N615", "N620", "N630", "N650");

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

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, n001);
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
            n650);
        AssertRegistroEcf.ConformsToManifest(n500, "N500", "1:13", n030);
        AssertRegistroEcf.ConformsToManifest(n600, "N600", "0:N", n030);
        AssertRegistroEcf.ConformsToManifest(n605, "N605", "0:N", n030);
        AssertRegistroEcf.ConformsToManifest(n610, "N610", "1:N", n030);
        AssertRegistroEcf.ConformsToManifest(n615, "N615", "1:1", n030);
        AssertRegistroEcf.ConformsToManifest(n620, "N620", "0:N", n030);
        AssertRegistroEcf.ConformsToManifest(n630, "N630", "0:N", n030);
        AssertRegistroEcf.ConformsToManifest(n650, "N650", "1:13", n030);

        n001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        n030.PerApur.Should().Be("T01");
        n500.Valor.Should().Be("-00123,4500");
        n605.Valor.Should().Be(-10000.25m);
        n615.VlLiqIncenFinor.Should().Be(-999.99m);
        n650.Valor.Should().Be(-500.25m);
    }

    [Fact]
    public async Task Writer_LotePreservaRepresentacoesEReconstruiHierarquiaNoNovoParse()
    {
        await using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(TextoLote));
        var parser = new ParserEcf();
        var arquivo = await parser.ReadAsync(entrada, TestContext.Current.CancellationToken);

        await using var saida = new MemoryStream();
        await new EscritorSpedTxt(new CatalogoSpedGerado()).WriteAsync(
            saida,
            arquivo.EnumerarRegistros(),
            TestContext.Current.CancellationToken);

        string serializado = EncodingSped.Latin1.GetString(saida.ToArray());
        serializado.Should().Be(TextoLote.Replace("\n", "\r\n"));

        saida.Position = 0;
        var relido = await parser.ReadAsync(saida, TestContext.Current.CancellationToken);
        var registros = relido.BlocoN.Registros;
        registros[2].Should().BeOfType<RegistroN500>()
            .Which.Valor.Should().Be("-00123,4500");
        registros[4].Should().BeOfType<RegistroN605>()
            .Which.Pai.Should().BeSameAs(registros[1]);
        registros[6].Should().BeOfType<RegistroN615>()
            .Which.VlLiqIncenFinor.Should().Be(-999.99m);
        registros[9].Should().BeOfType<RegistroN650>()
            .Which.Pai.Should().BeSameAs(registros[1]);
    }
}
