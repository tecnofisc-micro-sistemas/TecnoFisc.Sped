using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

/// <summary>
/// Testes unitários das variantes ICMS40/51/70/90/Part/ST (slice 14.4): valida o parser para os
/// CSTs de isenção, diferimento, redução+ST, outras situações, partilha interestadual e repasse ST.
/// </summary>
public sealed class IcmsNormalParte2Tests
{
    // ---------------------------------------------------------------------------
    // ICMS40 — isenta (CST 40), não tributada (CST 41) e suspensão (CST 50)
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Icms40_Cst40_SemDeseneracao_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS40>
              <orig>0</orig>
              <CST>40</CST>
            </ICMS40>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms40 = icms.Should().BeOfType<Icms40>().Subject;
        icms40.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms40.CST.ToString().Should().Be("040");
        icms40.VICMSDeson.Should().BeNull();
        icms40.MotDesICMS.Should().BeNull();
        icms40.IndDeduzDeson.Should().BeNull();
    }

    [Fact]
    public async Task Icms40_Cst41_ComDesoneracao_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS40>
              <orig>1</orig>
              <CST>41</CST>
              <vICMSDeson>50.00</vICMSDeson>
              <motDesICMS>7</motDesICMS>
              <indDeduzDeson>1</indDeduzDeson>
            </ICMS40>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms40 = icms.Should().BeOfType<Icms40>().Subject;
        icms40.Orig.Should().Be(OrigemMercadoria.EstrangeiraImportacaoDireta);
        icms40.CST.ToString().Should().Be("141");
        icms40.VICMSDeson.Should().Be(50.00m);
        icms40.MotDesICMS.Should().Be(7);
        icms40.IndDeduzDeson.Should().Be(1);
    }

    [Fact]
    public async Task Icms40_Cst50_ComDesoneracao_SemIndDeduz_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS40>
              <orig>2</orig>
              <CST>50</CST>
              <vICMSDeson>120.00</vICMSDeson>
              <motDesICMS>9</motDesICMS>
            </ICMS40>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms40 = icms.Should().BeOfType<Icms40>().Subject;
        icms40.Orig.Should().Be(OrigemMercadoria.EstrangeiraMercadoInterno);
        icms40.CST.ToString().Should().Be("250");
        icms40.VICMSDeson.Should().Be(120.00m);
        icms40.MotDesICMS.Should().Be(9);
        icms40.IndDeduzDeson.Should().BeNull();
    }

    // ---------------------------------------------------------------------------
    // ICMS51 — diferimento
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Icms51_TodosCamposPresentes_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS51>
              <orig>0</orig>
              <CST>51</CST>
              <modBC>3</modBC>
              <pRedBC>10.00</pRedBC>
              <cBenefRBC>MG123456</cBenefRBC>
              <vBC>900.00</vBC>
              <pICMS>12.00</pICMS>
              <vICMSOp>108.00</vICMSOp>
              <pDif>40.00</pDif>
              <vICMSDif>43.20</vICMSDif>
              <vICMS>64.80</vICMS>
              <vBCFCP>900.00</vBCFCP>
              <pFCP>2.00</pFCP>
              <vFCP>18.00</vFCP>
              <pFCPDif>40.00</pFCPDif>
              <vFCPDif>7.20</vFCPDif>
              <vFCPEfet>10.80</vFCPEfet>
            </ICMS51>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms51 = icms.Should().BeOfType<Icms51>().Subject;
        icms51.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms51.CST.ToString().Should().Be("051");
        icms51.ModBC.Should().Be(3);
        icms51.PRedBC.Should().Be(10.00m);
        icms51.CBenefRBC.Should().Be("MG123456");
        icms51.VBC.Should().Be(900.00m);
        icms51.PICMS.Should().Be(12.00m);
        icms51.VICMSOp.Should().Be(108.00m);
        icms51.PDif.Should().Be(40.00m);
        icms51.VICMSDif.Should().Be(43.20m);
        icms51.VICMS.Should().Be(64.80m);
        icms51.VBCFCP.Should().Be(900.00m);
        icms51.PFCP.Should().Be(2.00m);
        icms51.VFCP.Should().Be(18.00m);
        icms51.PFCPDif.Should().Be(40.00m);
        icms51.VFCPDif.Should().Be(7.20m);
        icms51.VFCPEfet.Should().Be(10.80m);
    }

    [Fact]
    public async Task Icms51_SomenteCamposObrigatorios_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS51>
              <orig>0</orig>
              <CST>51</CST>
            </ICMS51>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms51 = icms.Should().BeOfType<Icms51>().Subject;
        icms51.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms51.CST.ToString().Should().Be("051");
        icms51.ModBC.Should().BeNull();
        icms51.PRedBC.Should().BeNull();
        icms51.CBenefRBC.Should().BeNull();
        icms51.VBC.Should().BeNull();
        icms51.PICMS.Should().BeNull();
        icms51.VICMSOp.Should().BeNull();
        icms51.PDif.Should().BeNull();
        icms51.VICMSDif.Should().BeNull();
        icms51.VICMS.Should().BeNull();
        icms51.VBCFCP.Should().BeNull();
        icms51.PFCP.Should().BeNull();
        icms51.VFCP.Should().BeNull();
        icms51.PFCPDif.Should().BeNull();
        icms51.VFCPDif.Should().BeNull();
        icms51.VFCPEfet.Should().BeNull();
    }

    // ---------------------------------------------------------------------------
    // ICMS70 — redução de BC + ST
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Icms70_SemFcpSemDeson_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS70>
              <orig>0</orig>
              <CST>70</CST>
              <modBC>3</modBC>
              <pRedBC>25.00</pRedBC>
              <vBC>750.00</vBC>
              <pICMS>12.00</pICMS>
              <vICMS>90.00</vICMS>
              <modBCST>4</modBCST>
              <pMVAST>40.00</pMVAST>
              <vBCST>1050.00</vBCST>
              <pICMSST>18.00</pICMSST>
              <vICMSST>99.00</vICMSST>
            </ICMS70>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms70 = icms.Should().BeOfType<Icms70>().Subject;
        icms70.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms70.CST.ToString().Should().Be("070");
        icms70.ModBC.Should().Be(3);
        icms70.PRedBC.Should().Be(25.00m);
        icms70.VBC.Should().Be(750.00m);
        icms70.PICMS.Should().Be(12.00m);
        icms70.VICMS.Should().Be(90.00m);
        icms70.VBCFCP.Should().BeNull();
        icms70.PFCP.Should().BeNull();
        icms70.VFCP.Should().BeNull();
        icms70.ModBCST.Should().Be(4);
        icms70.PMVAST.Should().Be(40.00m);
        icms70.PRedBCST.Should().BeNull();
        icms70.VBCST.Should().Be(1050.00m);
        icms70.PICMSST.Should().Be(18.00m);
        icms70.VICMSST.Should().Be(99.00m);
        icms70.VBCFCPST.Should().BeNull();
        icms70.PFCPST.Should().BeNull();
        icms70.VFCPST.Should().BeNull();
        icms70.VICMSDeson.Should().BeNull();
        icms70.MotDesICMS.Should().BeNull();
        icms70.IndDeduzDeson.Should().BeNull();
        icms70.VICMSSTDeson.Should().BeNull();
        icms70.MotDesICMSST.Should().BeNull();
    }

    [Fact]
    public async Task Icms70_ComFcpComDesonComDesonST_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS70>
              <orig>0</orig>
              <CST>70</CST>
              <modBC>3</modBC>
              <pRedBC>20.00</pRedBC>
              <vBC>800.00</vBC>
              <pICMS>12.00</pICMS>
              <vICMS>96.00</vICMS>
              <vBCFCP>800.00</vBCFCP>
              <pFCP>2.00</pFCP>
              <vFCP>16.00</vFCP>
              <modBCST>4</modBCST>
              <pRedBCST>10.00</pRedBCST>
              <vBCST>1000.00</vBCST>
              <pICMSST>18.00</pICMSST>
              <vICMSST>84.00</vICMSST>
              <vBCFCPST>1000.00</vBCFCPST>
              <pFCPST>2.00</pFCPST>
              <vFCPST>20.00</vFCPST>
              <vICMSDeson>30.00</vICMSDeson>
              <motDesICMS>3</motDesICMS>
              <indDeduzDeson>0</indDeduzDeson>
              <vICMSSTDeson>40.00</vICMSSTDeson>
              <motDesICMSST>12</motDesICMSST>
            </ICMS70>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms70 = icms.Should().BeOfType<Icms70>().Subject;
        icms70.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms70.CST.ToString().Should().Be("070");
        icms70.PRedBC.Should().Be(20.00m);
        icms70.VBC.Should().Be(800.00m);
        icms70.PICMS.Should().Be(12.00m);
        icms70.VICMS.Should().Be(96.00m);
        icms70.VBCFCP.Should().Be(800.00m);
        icms70.PFCP.Should().Be(2.00m);
        icms70.VFCP.Should().Be(16.00m);
        icms70.ModBCST.Should().Be(4);
        icms70.PRedBCST.Should().Be(10.00m);
        icms70.VBCST.Should().Be(1000.00m);
        icms70.PICMSST.Should().Be(18.00m);
        icms70.VICMSST.Should().Be(84.00m);
        icms70.VBCFCPST.Should().Be(1000.00m);
        icms70.PFCPST.Should().Be(2.00m);
        icms70.VFCPST.Should().Be(20.00m);
        icms70.VICMSDeson.Should().Be(30.00m);
        icms70.MotDesICMS.Should().Be(3);
        icms70.IndDeduzDeson.Should().Be(0);
        icms70.VICMSSTDeson.Should().Be(40.00m);
        icms70.MotDesICMSST.Should().Be(12);
    }

    // ---------------------------------------------------------------------------
    // ICMS90 — outras situações
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Icms90_SomenteCst_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS90>
              <orig>0</orig>
              <CST>90</CST>
            </ICMS90>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms90 = icms.Should().BeOfType<Icms90>().Subject;
        icms90.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms90.CST.ToString().Should().Be("090");
        icms90.ModBC.Should().BeNull();
        icms90.VBC.Should().BeNull();
        icms90.PICMS.Should().BeNull();
        icms90.VICMS.Should().BeNull();
        icms90.ModBCST.Should().BeNull();
        icms90.VBCST.Should().BeNull();
    }

    [Fact]
    public async Task Icms90_ComGrupoProprioComGrupoST_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMS90>
              <orig>0</orig>
              <CST>90</CST>
              <modBC>3</modBC>
              <vBC>1000.00</vBC>
              <pRedBC>5.00</pRedBC>
              <cBenefRBC>SP12345678</cBenefRBC>
              <pICMS>12.00</pICMS>
              <vICMSOp>114.00</vICMSOp>
              <pDif>20.00</pDif>
              <vICMSDif>22.80</vICMSDif>
              <vICMS>91.20</vICMS>
              <vBCFCP>1000.00</vBCFCP>
              <pFCP>2.00</pFCP>
              <vFCP>20.00</vFCP>
              <pFCPDif>20.00</pFCPDif>
              <vFCPDif>4.00</vFCPDif>
              <vFCPEfet>16.00</vFCPEfet>
              <modBCST>4</modBCST>
              <pMVAST>30.00</pMVAST>
              <pRedBCST>5.00</pRedBCST>
              <vBCST>1300.00</vBCST>
              <pICMSST>18.00</pICMSST>
              <vICMSST>144.00</vICMSST>
              <vBCFCPST>1300.00</vBCFCPST>
              <pFCPST>2.00</pFCPST>
              <vFCPST>26.00</vFCPST>
              <vICMSDeson>20.00</vICMSDeson>
              <motDesICMS>9</motDesICMS>
              <indDeduzDeson>1</indDeduzDeson>
              <vICMSSTDeson>15.00</vICMSSTDeson>
              <motDesICMSST>3</motDesICMSST>
            </ICMS90>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icms90 = icms.Should().BeOfType<Icms90>().Subject;
        icms90.Orig.Should().Be(OrigemMercadoria.Nacional);
        icms90.CST.ToString().Should().Be("090");
        icms90.ModBC.Should().Be(3);
        icms90.VBC.Should().Be(1000.00m);
        icms90.PRedBC.Should().Be(5.00m);
        icms90.CBenefRBC.Should().Be("SP12345678");
        icms90.PICMS.Should().Be(12.00m);
        icms90.VICMSOp.Should().Be(114.00m);
        icms90.PDif.Should().Be(20.00m);
        icms90.VICMSDif.Should().Be(22.80m);
        icms90.VICMS.Should().Be(91.20m);
        icms90.VBCFCP.Should().Be(1000.00m);
        icms90.PFCP.Should().Be(2.00m);
        icms90.VFCP.Should().Be(20.00m);
        icms90.PFCPDif.Should().Be(20.00m);
        icms90.VFCPDif.Should().Be(4.00m);
        icms90.VFCPEfet.Should().Be(16.00m);
        icms90.ModBCST.Should().Be(4);
        icms90.PMVAST.Should().Be(30.00m);
        icms90.PRedBCST.Should().Be(5.00m);
        icms90.VBCST.Should().Be(1300.00m);
        icms90.PICMSST.Should().Be(18.00m);
        icms90.VICMSST.Should().Be(144.00m);
        icms90.VBCFCPST.Should().Be(1300.00m);
        icms90.PFCPST.Should().Be(2.00m);
        icms90.VFCPST.Should().Be(26.00m);
        icms90.VICMSDeson.Should().Be(20.00m);
        icms90.MotDesICMS.Should().Be(9);
        icms90.IndDeduzDeson.Should().Be(1);
        icms90.VICMSSTDeson.Should().Be(15.00m);
        icms90.MotDesICMSST.Should().Be(3);
    }

    // ---------------------------------------------------------------------------
    // ICMSPart — partilha interestadual
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task IcmsPart_SemFcpStSemDeson_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMSPart>
              <orig>0</orig>
              <CST>10</CST>
              <modBC>3</modBC>
              <vBC>1000.00</vBC>
              <pICMS>12.00</pICMS>
              <vICMS>120.00</vICMS>
              <modBCST>4</modBCST>
              <pMVAST>35.00</pMVAST>
              <vBCST>1350.00</vBCST>
              <pICMSST>18.00</pICMSST>
              <vICMSST>123.00</vICMSST>
              <pBCOp>60.00</pBCOp>
              <UFST>SP</UFST>
            </ICMSPart>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var part = icms.Should().BeOfType<IcmsPart>().Subject;
        part.Orig.Should().Be(OrigemMercadoria.Nacional);
        part.CST.ToString().Should().Be("010");
        part.ModBC.Should().Be(3);
        part.VBC.Should().Be(1000.00m);
        part.PRedBC.Should().BeNull();
        part.PICMS.Should().Be(12.00m);
        part.VICMS.Should().Be(120.00m);
        part.ModBCST.Should().Be(4);
        part.PMVAST.Should().Be(35.00m);
        part.PRedBCST.Should().BeNull();
        part.VBCST.Should().Be(1350.00m);
        part.PICMSST.Should().Be(18.00m);
        part.VICMSST.Should().Be(123.00m);
        part.VBCFCPST.Should().BeNull();
        part.PFCPST.Should().BeNull();
        part.VFCPST.Should().BeNull();
        part.PBCOp.Should().Be(60.00m);
        part.UFST.Should().Be("SP");
        part.VICMSDeson.Should().BeNull();
        part.MotDesICMS.Should().BeNull();
        part.IndDeduzDeson.Should().BeNull();
    }

    [Fact]
    public async Task IcmsPart_ComFcpStComDesonComRedBC_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMSPart>
              <orig>0</orig>
              <CST>20</CST>
              <modBC>3</modBC>
              <vBC>800.00</vBC>
              <pRedBC>20.00</pRedBC>
              <pICMS>12.00</pICMS>
              <vICMS>96.00</vICMS>
              <modBCST>4</modBCST>
              <pRedBCST>10.00</pRedBCST>
              <vBCST>1080.00</vBCST>
              <pICMSST>18.00</pICMSST>
              <vICMSST>97.20</vICMSST>
              <vBCFCPST>1080.00</vBCFCPST>
              <pFCPST>2.00</pFCPST>
              <vFCPST>21.60</vFCPST>
              <pBCOp>60.00</pBCOp>
              <UFST>MG</UFST>
              <vICMSDeson>24.00</vICMSDeson>
              <motDesICMS>9</motDesICMS>
              <indDeduzDeson>0</indDeduzDeson>
            </ICMSPart>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var part = icms.Should().BeOfType<IcmsPart>().Subject;
        part.Orig.Should().Be(OrigemMercadoria.Nacional);
        part.CST.ToString().Should().Be("020");
        part.VBC.Should().Be(800.00m);
        part.PRedBC.Should().Be(20.00m);
        part.PICMS.Should().Be(12.00m);
        part.VICMS.Should().Be(96.00m);
        part.ModBCST.Should().Be(4);
        part.PRedBCST.Should().Be(10.00m);
        part.VBCST.Should().Be(1080.00m);
        part.PICMSST.Should().Be(18.00m);
        part.VICMSST.Should().Be(97.20m);
        part.VBCFCPST.Should().Be(1080.00m);
        part.PFCPST.Should().Be(2.00m);
        part.VFCPST.Should().Be(21.60m);
        part.PBCOp.Should().Be(60.00m);
        part.UFST.Should().Be("MG");
        part.VICMSDeson.Should().Be(24.00m);
        part.MotDesICMS.Should().Be(9);
        part.IndDeduzDeson.Should().Be(0);
    }

    // ---------------------------------------------------------------------------
    // ICMSST — repasse de ICMS ST retido (substituto → UF de destino)
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task IcmsST_SemFcpSemEfet_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMSST>
              <orig>0</orig>
              <CST>41</CST>
              <vBCSTRet>1200.00</vBCSTRet>
              <pST>18.00</pST>
              <vICMSSTRet>96.00</vICMSSTRet>
              <vBCSTDest>1250.00</vBCSTDest>
              <vICMSSTDest>100.00</vICMSSTDest>
            </ICMSST>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icmsST = icms.Should().BeOfType<IcmsST>().Subject;
        icmsST.Orig.Should().Be(OrigemMercadoria.Nacional);
        icmsST.CST.ToString().Should().Be("041");
        icmsST.VBCSTRet.Should().Be(1200.00m);
        icmsST.PST.Should().Be(18.00m);
        icmsST.VICMSSubstituto.Should().BeNull();
        icmsST.VICMSSTRet.Should().Be(96.00m);
        icmsST.VBCFCPSTRet.Should().BeNull();
        icmsST.PFCPSTRet.Should().BeNull();
        icmsST.VFCPSTRet.Should().BeNull();
        icmsST.VBCSTDest.Should().Be(1250.00m);
        icmsST.VICMSSTDest.Should().Be(100.00m);
        icmsST.PRedBCEfet.Should().BeNull();
        icmsST.VBCEfet.Should().BeNull();
        icmsST.PICMSEfet.Should().BeNull();
        icmsST.VICMSEfet.Should().BeNull();
    }

    [Fact]
    public async Task IcmsST_ComFcpComSubstitutoComEfet_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMSST>
              <orig>1</orig>
              <CST>60</CST>
              <vBCSTRet>2000.00</vBCSTRet>
              <pST>18.00</pST>
              <vICMSSubstituto>180.00</vICMSSubstituto>
              <vICMSSTRet>160.00</vICMSSTRet>
              <vBCFCPSTRet>2000.00</vBCFCPSTRet>
              <pFCPSTRet>2.00</pFCPSTRet>
              <vFCPSTRet>40.00</vFCPSTRet>
              <vBCSTDest>2100.00</vBCSTDest>
              <vICMSSTDest>168.00</vICMSSTDest>
              <pRedBCEfet>5.00</pRedBCEfet>
              <vBCEfet>1995.00</vBCEfet>
              <pICMSEfet>12.00</pICMSEfet>
              <vICMSEfet>239.40</vICMSEfet>
            </ICMSST>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var icmsST = icms.Should().BeOfType<IcmsST>().Subject;
        icmsST.Orig.Should().Be(OrigemMercadoria.EstrangeiraImportacaoDireta);
        icmsST.CST.ToString().Should().Be("160");
        icmsST.VBCSTRet.Should().Be(2000.00m);
        icmsST.PST.Should().Be(18.00m);
        icmsST.VICMSSubstituto.Should().Be(180.00m);
        icmsST.VICMSSTRet.Should().Be(160.00m);
        icmsST.VBCFCPSTRet.Should().Be(2000.00m);
        icmsST.PFCPSTRet.Should().Be(2.00m);
        icmsST.VFCPSTRet.Should().Be(40.00m);
        icmsST.VBCSTDest.Should().Be(2100.00m);
        icmsST.VICMSSTDest.Should().Be(168.00m);
        icmsST.PRedBCEfet.Should().Be(5.00m);
        icmsST.VBCEfet.Should().Be(1995.00m);
        icmsST.PICMSEfet.Should().Be(12.00m);
        icmsST.VICMSEfet.Should().Be(239.40m);
    }
}
