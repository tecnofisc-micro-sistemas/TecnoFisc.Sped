using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

/// <summary>
/// Testes unitários das variantes ICMS00/10/20/30 (slice 14.4): valida o parser para os CSTs do
/// regime normal tributado e com substituição tributária, incluindo os campos FCP e desoneração.
/// </summary>
public sealed class IcmsNormalTests
{
    // ---------------------------------------------------------------------------
    // ICMS00 — tributação integral
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Icms00_SemFcp_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS00>
              <orig>0</orig>
              <CST>00</CST>
              <modBC>3</modBC>
              <vBC>1000.00</vBC>
              <pICMS>12.00</pICMS>
              <vICMS>120.00</vICMS>
            </ICMS00>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms00 = icms.Should().BeOfType<Icms00>().Subject;
        icms00.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms00.CST.ToString().Should().Be("000");
        icms00.ModBC.Should().Be(3);
        icms00.VBC.Should().Be(1000.00m);
        icms00.PICMS.Should().Be(12.00m);
        icms00.VICMS.Should().Be(120.00m);
        icms00.PFCP.Should().BeNull();
        icms00.VFCP.Should().BeNull();
    }

    [Fact]
    public async Task Icms00_ComFcp_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS00>
              <orig>1</orig>
              <CST>00</CST>
              <modBC>0</modBC>
              <vBC>2000.00</vBC>
              <pICMS>18.00</pICMS>
              <vICMS>360.00</vICMS>
              <pFCP>2.00</pFCP>
              <vFCP>40.00</vFCP>
            </ICMS00>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms00 = icms.Should().BeOfType<Icms00>().Subject;
        icms00.Orig.Should().Be(OrigemMercadoria.EstrangeiraImportacaoDireta);
        icms00.CST.ToString().Should().Be("100");
        icms00.ModBC.Should().Be(0);
        icms00.VBC.Should().Be(2000.00m);
        icms00.PICMS.Should().Be(18.00m);
        icms00.VICMS.Should().Be(360.00m);
        icms00.PFCP.Should().Be(2.00m);
        icms00.VFCP.Should().Be(40.00m);
    }

    // ---------------------------------------------------------------------------
    // ICMS10 — tributada com ST
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Icms10_SemFcpSemDeson_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS10>
              <orig>0</orig>
              <CST>10</CST>
              <modBC>3</modBC>
              <vBC>500.00</vBC>
              <pICMS>12.00</pICMS>
              <vICMS>60.00</vICMS>
              <modBCST>4</modBCST>
              <pMVAST>40.00</pMVAST>
              <vBCST>700.00</vBCST>
              <pICMSST>18.00</pICMSST>
              <vICMSST>66.00</vICMSST>
            </ICMS10>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms10 = icms.Should().BeOfType<Icms10>().Subject;
        icms10.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms10.CST.ToString().Should().Be("010");
        icms10.ModBC.Should().Be(3);
        icms10.VBC.Should().Be(500.00m);
        icms10.PICMS.Should().Be(12.00m);
        icms10.VICMS.Should().Be(60.00m);
        icms10.VBCFCP.Should().BeNull();
        icms10.PFCP.Should().BeNull();
        icms10.VFCP.Should().BeNull();
        icms10.ModBCST.Should().Be(4);
        icms10.PMVAST.Should().Be(40.00m);
        icms10.PRedBCST.Should().BeNull();
        icms10.VBCST.Should().Be(700.00m);
        icms10.PICMSST.Should().Be(18.00m);
        icms10.VICMSST.Should().Be(66.00m);
        icms10.VBCFCPST.Should().BeNull();
        icms10.PFCPST.Should().BeNull();
        icms10.VFCPST.Should().BeNull();
        icms10.VICMSSTDeson.Should().BeNull();
        icms10.MotDesICMSST.Should().BeNull();
    }

    [Fact]
    public async Task Icms10_ComFcpComFcpStComDeson_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS10>
              <orig>2</orig>
              <CST>10</CST>
              <modBC>3</modBC>
              <vBC>800.00</vBC>
              <pICMS>12.00</pICMS>
              <vICMS>96.00</vICMS>
              <vBCFCP>800.00</vBCFCP>
              <pFCP>2.00</pFCP>
              <vFCP>16.00</vFCP>
              <modBCST>4</modBCST>
              <vBCST>1100.00</vBCST>
              <pICMSST>18.00</pICMSST>
              <vICMSST>102.00</vICMSST>
              <vBCFCPST>1100.00</vBCFCPST>
              <pFCPST>2.00</pFCPST>
              <vFCPST>22.00</vFCPST>
              <vICMSSTDeson>50.00</vICMSSTDeson>
              <motDesICMSST>9</motDesICMSST>
            </ICMS10>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms10 = icms.Should().BeOfType<Icms10>().Subject;
        icms10.Orig.Should().Be(OrigemMercadoria.EstrangeiraMercadoInterno);
        icms10.CST.ToString().Should().Be("210");
        icms10.VBCFCP.Should().Be(800.00m);
        icms10.PFCP.Should().Be(2.00m);
        icms10.VFCP.Should().Be(16.00m);
        icms10.ModBCST.Should().Be(4);
        icms10.VBCST.Should().Be(1100.00m);
        icms10.PICMSST.Should().Be(18.00m);
        icms10.VICMSST.Should().Be(102.00m);
        icms10.VBCFCPST.Should().Be(1100.00m);
        icms10.PFCPST.Should().Be(2.00m);
        icms10.VFCPST.Should().Be(22.00m);
        icms10.VICMSSTDeson.Should().Be(50.00m);
        icms10.MotDesICMSST.Should().Be(9);
    }

    // ---------------------------------------------------------------------------
    // ICMS20 — com redução de BC
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Icms20_SemFcpSemDeson_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS20>
              <orig>0</orig>
              <CST>20</CST>
              <modBC>3</modBC>
              <pRedBC>25.00</pRedBC>
              <vBC>750.00</vBC>
              <pICMS>12.00</pICMS>
              <vICMS>90.00</vICMS>
            </ICMS20>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms20 = icms.Should().BeOfType<Icms20>().Subject;
        icms20.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms20.CST.ToString().Should().Be("020");
        icms20.ModBC.Should().Be(3);
        icms20.PRedBC.Should().Be(25.00m);
        icms20.VBC.Should().Be(750.00m);
        icms20.PICMS.Should().Be(12.00m);
        icms20.VICMS.Should().Be(90.00m);
        icms20.VBCFCP.Should().BeNull();
        icms20.PFCP.Should().BeNull();
        icms20.VFCP.Should().BeNull();
        icms20.VICMSDeson.Should().BeNull();
        icms20.MotDesICMS.Should().BeNull();
        icms20.IndDeduzDeson.Should().BeNull();
    }

    [Fact]
    public async Task Icms20_ComFcpComDeson_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS20>
              <orig>0</orig>
              <CST>20</CST>
              <modBC>3</modBC>
              <pRedBC>33.33</pRedBC>
              <vBC>666.67</vBC>
              <pICMS>18.00</pICMS>
              <vICMS>120.00</vICMS>
              <vBCFCP>666.67</vBCFCP>
              <pFCP>2.00</pFCP>
              <vFCP>13.33</vFCP>
              <vICMSDeson>60.00</vICMSDeson>
              <motDesICMS>9</motDesICMS>
              <indDeduzDeson>1</indDeduzDeson>
            </ICMS20>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms20 = icms.Should().BeOfType<Icms20>().Subject;
        icms20.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms20.CST.ToString().Should().Be("020");
        icms20.PRedBC.Should().Be(33.33m);
        icms20.VBC.Should().Be(666.67m);
        icms20.PICMS.Should().Be(18.00m);
        icms20.VICMS.Should().Be(120.00m);
        icms20.VBCFCP.Should().Be(666.67m);
        icms20.PFCP.Should().Be(2.00m);
        icms20.VFCP.Should().Be(13.33m);
        icms20.VICMSDeson.Should().Be(60.00m);
        icms20.MotDesICMS.Should().Be(9);
        icms20.IndDeduzDeson.Should().Be(1);
    }

    // ---------------------------------------------------------------------------
    // ICMS30 — isenta/não-tributada com ST
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Icms30_SemFcpStSemDeson_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS30>
              <orig>0</orig>
              <CST>30</CST>
              <modBCST>4</modBCST>
              <pMVAST>50.00</pMVAST>
              <vBCST>1500.00</vBCST>
              <pICMSST>18.00</pICMSST>
              <vICMSST>150.00</vICMSST>
            </ICMS30>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms30 = icms.Should().BeOfType<Icms30>().Subject;
        icms30.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms30.CST.ToString().Should().Be("030");
        icms30.ModBCST.Should().Be(4);
        icms30.PMVAST.Should().Be(50.00m);
        icms30.PRedBCST.Should().BeNull();
        icms30.VBCST.Should().Be(1500.00m);
        icms30.PICMSST.Should().Be(18.00m);
        icms30.VICMSST.Should().Be(150.00m);
        icms30.VBCFCPST.Should().BeNull();
        icms30.PFCPST.Should().BeNull();
        icms30.VFCPST.Should().BeNull();
        icms30.VICMSDeson.Should().BeNull();
        icms30.MotDesICMS.Should().BeNull();
        icms30.IndDeduzDeson.Should().BeNull();
    }

    [Fact]
    public async Task Icms30_ComFcpStComDeson_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS30>
              <orig>0</orig>
              <CST>30</CST>
              <modBCST>4</modBCST>
              <pRedBCST>10.00</pRedBCST>
              <vBCST>900.00</vBCST>
              <pICMSST>18.00</pICMSST>
              <vICMSST>72.00</vICMSST>
              <vBCFCPST>900.00</vBCFCPST>
              <pFCPST>2.00</pFCPST>
              <vFCPST>18.00</vFCPST>
              <vICMSDeson>30.00</vICMSDeson>
              <motDesICMS>7</motDesICMS>
              <indDeduzDeson>0</indDeduzDeson>
            </ICMS30>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms30 = icms.Should().BeOfType<Icms30>().Subject;
        icms30.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms30.CST.ToString().Should().Be("030");
        icms30.ModBCST.Should().Be(4);
        icms30.PRedBCST.Should().Be(10.00m);
        icms30.VBCST.Should().Be(900.00m);
        icms30.PICMSST.Should().Be(18.00m);
        icms30.VICMSST.Should().Be(72.00m);
        icms30.VBCFCPST.Should().Be(900.00m);
        icms30.PFCPST.Should().Be(2.00m);
        icms30.VFCPST.Should().Be(18.00m);
        icms30.VICMSDeson.Should().Be(30.00m);
        icms30.MotDesICMS.Should().Be(7);
        icms30.IndDeduzDeson.Should().Be(0);
    }
}
