using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote1;

public sealed class RegistroM355Tests
{
    private static readonly string TextoLote = string.Join('\n',
        "|0000|LECF|0012|11111111000191|EMPRESA SINTETICA|0|0|||01012025|31122025|N||0||",
        "|M001|0|",
        "|M010|000123|CONTA PARTE B|01012018|001001|31122026|I|1000,00|D|11222333000181|",
        "|M030|01012025|31032025|T01|",
        "|M300|0138|OUTRAS EXCLUSOES|E|3|-1000,25|HISTORICO LALUR|",
        "|M305|000123|600,10|D|",
        "|M310|1.01.01.01||400,15|C|",
        "|M312|0000012345|",
        "|M315|1|0001234-56.2025.4.01|",
        "|M350|0138|OUTRAS EXCLUSOES|E|1|-2000,50|HISTORICO LACS|",
        "|M355|000123|2000,50|C|",
        string.Empty);

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM355(), "M355", "0:N");
    }

    [Fact]
    public void Parser_LeContaParteBValorDecimalEIndicador()
    {
        var resultado = new ParserEcf().ParseLinha("|M355|000123|2000,50|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM355>().Which;
        registro.CodCtaB.Should().Be("000123");
        registro.VlCta.Should().Be(2000.50m);
        registro.IndVlCta.Should().Be(IndicadorDebitoCredito.Credor);
    }

    [Fact]
    public async Task ReadAsync_LoteMaterializaCadeiasParteAParteBProcessosEPaisExplicitos()
    {
        await using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(TextoLote));

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        arquivo.BlocoM.Registros.Select(registro => registro.Codigo).Should().Equal(
            "M001", "M010", "M030", "M300", "M305", "M310", "M312", "M315", "M350", "M355");

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

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, m001);
        AssertRegistroEcf.ConformsToManifest(m001, "M001", "1:1", r0000, m010, m030);
        AssertRegistroEcf.ConformsToManifest(m010, "M010", "0:N", m001);
        AssertRegistroEcf.ConformsToManifest(m030, "M030", "0:13", m001, m300, m350);
        AssertRegistroEcf.ConformsToManifest(m300, "M300", "1:N", m030, m305, m310, m315);
        AssertRegistroEcf.ConformsToManifest(m305, "M305", "0:N", m300);
        AssertRegistroEcf.ConformsToManifest(m310, "M310", "0:N", m300, m312);
        AssertRegistroEcf.ConformsToManifest(m312, "M312", "0:N", m310);
        AssertRegistroEcf.ConformsToManifest(m315, "M315", "0:N", m300);
        AssertRegistroEcf.ConformsToManifest(m350, "M350", "1:N", m030, m355);
        AssertRegistroEcf.ConformsToManifest(m355, "M355", "0:N", m350);

        m001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        m010.CodTributo.Should().Be(IndicadorTributoContaParteB.Irpj);
        m300.Valor.Should().Be(-1000.25m);
        m312.NumLcto.Should().Be("0000012345");
        m315.NumProc.Should().Be("0001234-56.2025.4.01");
        m350.Valor.Should().Be(-2000.50m);
    }

    [Fact]
    public async Task Writer_LotePreservaCodigosIdentificadoresDecimaisEHierarquiaNoNovoParse()
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
        var registros = relido.BlocoM.Registros;
        registros[3].Should().BeOfType<RegistroM300>().Which.CampoCodigo.Should().Be("0138");
        registros[6].Should().BeOfType<RegistroM312>().Which.Pai.Should().BeSameAs(registros[5]);
        registros[7].Should().BeOfType<RegistroM315>().Which.NumProc.Should().Be("0001234-56.2025.4.01");
        registros[9].Should().BeOfType<RegistroM355>().Which.Pai.Should().BeSameAs(registros[8]);
    }
}
