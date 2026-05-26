using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

/// <summary>
/// Testes unitários das variantes ICMS do Simples Nacional (CSOSN) — slice 14.4: valida o parser
/// para ICMSSN101, ICMSSN102, ICMSSN201, ICMSSN202, ICMSSN500 e ICMSSN900.
/// </summary>
public sealed class IcmsSimplesNacionalTests
{
    // ---------------------------------------------------------------------------
    // ICMSSN101 — CSOSN 101 (com permissão de crédito)
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task IcmsSN101_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMSSN101>
              <orig>0</orig>
              <CSOSN>101</CSOSN>
              <pCredSN>3.50</pCredSN>
              <vCredICMSSN>35.00</vCredICMSSN>
            </ICMSSN101>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn101 = icms.Should().BeOfType<IcmsSN101>().Subject;
        sn101.Orig.Should().Be(OrigemMercadoria.Nacional);
        sn101.CSOSN.ToString().Should().Be("101");
        sn101.PCredSN.Should().Be(3.50m);
        sn101.VCredICMSSN.Should().Be(35.00m);
    }

    [Fact]
    public async Task IcmsSN101_OrigEstrangeira_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMSSN101>
              <orig>1</orig>
              <CSOSN>101</CSOSN>
              <pCredSN>2.00</pCredSN>
              <vCredICMSSN>20.00</vCredICMSSN>
            </ICMSSN101>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn101 = icms.Should().BeOfType<IcmsSN101>().Subject;
        sn101.Orig.Should().Be(OrigemMercadoria.EstrangeiraImportacaoDireta);
        sn101.CSOSN.ToString().Should().Be("101");
        sn101.PCredSN.Should().Be(2.00m);
        sn101.VCredICMSSN.Should().Be(20.00m);
    }

    // ---------------------------------------------------------------------------
    // ICMSSN102 — CSOSN 102, 103, 300, 400 (sem permissão de crédito)
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task IcmsSN102_Csosn102_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMSSN102>
              <orig>0</orig>
              <CSOSN>102</CSOSN>
            </ICMSSN102>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn102 = icms.Should().BeOfType<IcmsSN102>().Subject;
        sn102.Orig.Should().Be(OrigemMercadoria.Nacional);
        sn102.CSOSN.ToString().Should().Be("102");
    }

    [Fact]
    public async Task IcmsSN102_Csosn400_ParseiaCorretamente()
    {
        // Verifica round-trip para segundo código coberto pelo mesmo XSD complexType
        const string snippet = """
            <ICMSSN102>
              <orig>0</orig>
              <CSOSN>400</CSOSN>
            </ICMSSN102>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn102 = icms.Should().BeOfType<IcmsSN102>().Subject;
        sn102.CSOSN.ToString().Should().Be("400");
    }

    [Fact]
    public async Task IcmsSN102_SemOrig_DefaultNacional()
    {
        // orig é opcional (minOccurs="0") no XSD para ICMSSN102; ausente → default Nacional (0).
        const string snippet = """
            <ICMSSN102><CSOSN>400</CSOSN></ICMSSN102>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn102 = icms.Should().BeOfType<IcmsSN102>().Subject;
        sn102.CSOSN.ToString().Should().Be("400");
        sn102.Orig.Should().Be(OrigemMercadoria.Nacional);
    }

    [Fact]
    public async Task IcmsSN102_Csosn103_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMSSN102>
              <orig>0</orig>
              <CSOSN>103</CSOSN>
            </ICMSSN102>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        icms.Should().BeOfType<IcmsSN102>()
            .Which.CSOSN.ToString().Should().Be("103");
    }

    [Fact]
    public async Task IcmsSN102_Csosn300_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMSSN102>
              <orig>0</orig>
              <CSOSN>300</CSOSN>
            </ICMSSN102>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        icms.Should().BeOfType<IcmsSN102>()
            .Which.CSOSN.ToString().Should().Be("300");
    }

    // ---------------------------------------------------------------------------
    // ICMSSN201 — CSOSN 201 (com crédito + ST)
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task IcmsSN201_ComFcpSt_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMSSN201>
              <orig>0</orig>
              <CSOSN>201</CSOSN>
              <modBCST>4</modBCST>
              <pMVAST>30.00</pMVAST>
              <pRedBCST>10.00</pRedBCST>
              <vBCST>900.00</vBCST>
              <pICMSST>12.00</pICMSST>
              <vICMSST>108.00</vICMSST>
              <vBCFCPST>900.00</vBCFCPST>
              <pFCPST>2.00</pFCPST>
              <vFCPST>18.00</vFCPST>
              <pCredSN>3.50</pCredSN>
              <vCredICMSSN>35.00</vCredICMSSN>
            </ICMSSN201>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn201 = icms.Should().BeOfType<IcmsSN201>().Subject;
        sn201.Orig.Should().Be(OrigemMercadoria.Nacional);
        sn201.CSOSN.ToString().Should().Be("201");
        sn201.ModBCST.Should().Be(4);
        sn201.PMVAST.Should().Be(30.00m);
        sn201.PRedBCST.Should().Be(10.00m);
        sn201.VBCST.Should().Be(900.00m);
        sn201.PICMSST.Should().Be(12.00m);
        sn201.VICMSST.Should().Be(108.00m);
        sn201.VBCFCPST.Should().Be(900.00m);
        sn201.PFCPST.Should().Be(2.00m);
        sn201.VFCPST.Should().Be(18.00m);
        sn201.PCredSN.Should().Be(3.50m);
        sn201.VCredICMSSN.Should().Be(35.00m);
    }

    [Fact]
    public async Task IcmsSN201_SemFcpStSemOpcional_ParseiaCorretamente()
    {
        // Sem pMVAST, pRedBCST, vBCFCPST, pFCPST, vFCPST → devem ser null
        const string snippet = """
            <ICMSSN201>
              <orig>0</orig>
              <CSOSN>201</CSOSN>
              <modBCST>3</modBCST>
              <vBCST>500.00</vBCST>
              <pICMSST>12.00</pICMSST>
              <vICMSST>60.00</vICMSST>
              <pCredSN>2.00</pCredSN>
              <vCredICMSSN>20.00</vCredICMSSN>
            </ICMSSN201>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn201 = icms.Should().BeOfType<IcmsSN201>().Subject;
        sn201.CSOSN.ToString().Should().Be("201");
        sn201.ModBCST.Should().Be(3);
        sn201.PMVAST.Should().BeNull();
        sn201.PRedBCST.Should().BeNull();
        sn201.VBCFCPST.Should().BeNull();
        sn201.PFCPST.Should().BeNull();
        sn201.VFCPST.Should().BeNull();
        sn201.VBCST.Should().Be(500.00m);
        sn201.PCredSN.Should().Be(2.00m);
        sn201.VCredICMSSN.Should().Be(20.00m);
    }

    // ---------------------------------------------------------------------------
    // ICMSSN202 — CSOSN 202 e 203 (sem crédito + ST)
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task IcmsSN202_Csosn202_ComTodosOsCampos_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMSSN202>
              <orig>0</orig>
              <CSOSN>202</CSOSN>
              <modBCST>4</modBCST>
              <pMVAST>25.00</pMVAST>
              <pRedBCST>5.00</pRedBCST>
              <vBCST>800.00</vBCST>
              <pICMSST>12.00</pICMSST>
              <vICMSST>96.00</vICMSST>
              <vBCFCPST>800.00</vBCFCPST>
              <pFCPST>2.00</pFCPST>
              <vFCPST>16.00</vFCPST>
            </ICMSSN202>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn202 = icms.Should().BeOfType<IcmsSN202>().Subject;
        sn202.Orig.Should().Be(OrigemMercadoria.Nacional);
        sn202.CSOSN.ToString().Should().Be("202");
        sn202.ModBCST.Should().Be(4);
        sn202.PMVAST.Should().Be(25.00m);
        sn202.PRedBCST.Should().Be(5.00m);
        sn202.VBCST.Should().Be(800.00m);
        sn202.PICMSST.Should().Be(12.00m);
        sn202.VICMSST.Should().Be(96.00m);
        sn202.VBCFCPST.Should().Be(800.00m);
        sn202.PFCPST.Should().Be(2.00m);
        sn202.VFCPST.Should().Be(16.00m);
    }

    [Fact]
    public async Task IcmsSN202_Csosn203_SemOpcional_ParseiaCorretamente()
    {
        // Verifica round-trip para CSOSN 203 e ausência dos opcionais
        const string snippet = """
            <ICMSSN202>
              <orig>0</orig>
              <CSOSN>203</CSOSN>
              <modBCST>3</modBCST>
              <vBCST>600.00</vBCST>
              <pICMSST>12.00</pICMSST>
              <vICMSST>72.00</vICMSST>
            </ICMSSN202>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn202 = icms.Should().BeOfType<IcmsSN202>().Subject;
        sn202.CSOSN.ToString().Should().Be("203");
        sn202.ModBCST.Should().Be(3);
        sn202.PMVAST.Should().BeNull();
        sn202.PRedBCST.Should().BeNull();
        sn202.VBCST.Should().Be(600.00m);
        sn202.VBCFCPST.Should().BeNull();
        sn202.PFCPST.Should().BeNull();
        sn202.VFCPST.Should().BeNull();
    }

    // ---------------------------------------------------------------------------
    // ICMSSN500 — CSOSN 500 (ST retido anteriormente)
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task IcmsSN500_ComTodosOsCampos_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMSSN500>
              <orig>0</orig>
              <CSOSN>500</CSOSN>
              <vBCSTRet>700.00</vBCSTRet>
              <pST>12.00</pST>
              <vICMSSubstituto>50.00</vICMSSubstituto>
              <vICMSSTRet>84.00</vICMSSTRet>
              <vBCFCPSTRet>700.00</vBCFCPSTRet>
              <pFCPSTRet>2.00</pFCPSTRet>
              <vFCPSTRet>14.00</vFCPSTRet>
              <pRedBCEfet>10.00</pRedBCEfet>
              <vBCEfet>630.00</vBCEfet>
              <pICMSEfet>12.00</pICMSEfet>
              <vICMSEfet>75.60</vICMSEfet>
            </ICMSSN500>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn500 = icms.Should().BeOfType<IcmsSN500>().Subject;
        sn500.Orig.Should().Be(OrigemMercadoria.Nacional);
        sn500.CSOSN.ToString().Should().Be("500");
        sn500.VBCSTRet.Should().Be(700.00m);
        sn500.PST.Should().Be(12.00m);
        sn500.VICMSSubstituto.Should().Be(50.00m);
        sn500.VICMSSTRet.Should().Be(84.00m);
        sn500.VBCFCPSTRet.Should().Be(700.00m);
        sn500.PFCPSTRet.Should().Be(2.00m);
        sn500.VFCPSTRet.Should().Be(14.00m);
        sn500.PRedBCEfet.Should().Be(10.00m);
        sn500.VBCEfet.Should().Be(630.00m);
        sn500.PICMSEfet.Should().Be(12.00m);
        sn500.VICMSEfet.Should().Be(75.60m);
    }

    [Fact]
    public async Task IcmsSN500_SemCamposOpcionais_ParseiaCorretamente()
    {
        // Todos os grupos são opcionais — deve resultar em todos nulls
        const string snippet = """
            <ICMSSN500>
              <orig>0</orig>
              <CSOSN>500</CSOSN>
            </ICMSSN500>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn500 = icms.Should().BeOfType<IcmsSN500>().Subject;
        sn500.CSOSN.ToString().Should().Be("500");
        sn500.VBCSTRet.Should().BeNull();
        sn500.PST.Should().BeNull();
        sn500.VICMSSubstituto.Should().BeNull();
        sn500.VICMSSTRet.Should().BeNull();
        sn500.VBCFCPSTRet.Should().BeNull();
        sn500.PFCPSTRet.Should().BeNull();
        sn500.VFCPSTRet.Should().BeNull();
        sn500.PRedBCEfet.Should().BeNull();
        sn500.VBCEfet.Should().BeNull();
        sn500.PICMSEfet.Should().BeNull();
        sn500.VICMSEfet.Should().BeNull();
    }

    // ---------------------------------------------------------------------------
    // ICMSSN900 — CSOSN 900 (outros)
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task IcmsSN900_ComTodosOsCampos_ParseiaCorretamente()
    {
        const string snippet = """
            <ICMSSN900>
              <orig>0</orig>
              <CSOSN>900</CSOSN>
              <modBC>3</modBC>
              <vBC>1000.00</vBC>
              <pRedBC>5.00</pRedBC>
              <pICMS>12.00</pICMS>
              <vICMS>114.00</vICMS>
              <modBCST>4</modBCST>
              <pMVAST>20.00</pMVAST>
              <pRedBCST>10.00</pRedBCST>
              <vBCST>1080.00</vBCST>
              <pICMSST>12.00</pICMSST>
              <vICMSST>129.60</vICMSST>
              <vBCFCPST>1080.00</vBCFCPST>
              <pFCPST>2.00</pFCPST>
              <vFCPST>21.60</vFCPST>
              <pCredSN>3.50</pCredSN>
              <vCredICMSSN>35.00</vCredICMSSN>
            </ICMSSN900>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn900 = icms.Should().BeOfType<IcmsSN900>().Subject;
        sn900.Orig.Should().Be(OrigemMercadoria.Nacional);
        sn900.CSOSN.ToString().Should().Be("900");
        sn900.ModBC.Should().Be(3);
        sn900.VBC.Should().Be(1000.00m);
        sn900.PRedBC.Should().Be(5.00m);
        sn900.PICMS.Should().Be(12.00m);
        sn900.VICMS.Should().Be(114.00m);
        sn900.ModBCST.Should().Be(4);
        sn900.PMVAST.Should().Be(20.00m);
        sn900.PRedBCST.Should().Be(10.00m);
        sn900.VBCST.Should().Be(1080.00m);
        sn900.PICMSST.Should().Be(12.00m);
        sn900.VICMSST.Should().Be(129.60m);
        sn900.VBCFCPST.Should().Be(1080.00m);
        sn900.PFCPST.Should().Be(2.00m);
        sn900.VFCPST.Should().Be(21.60m);
        sn900.PCredSN.Should().Be(3.50m);
        sn900.VCredICMSSN.Should().Be(35.00m);
    }

    [Fact]
    public async Task IcmsSN900_SemOrig_DefaultNacional()
    {
        // orig é opcional (minOccurs="0") no XSD para ICMSSN900; ausente → default Nacional (0).
        const string snippet = """
            <ICMSSN900>
              <CSOSN>900</CSOSN>
            </ICMSSN900>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn900 = icms.Should().BeOfType<IcmsSN900>().Subject;
        sn900.CSOSN.ToString().Should().Be("900");
        sn900.Orig.Should().Be(OrigemMercadoria.Nacional);
    }

    [Fact]
    public async Task IcmsSN900_SemCamposOpcionais_ParseiaCorretamente()
    {
        // Apenas orig e CSOSN → todos os grupos opcionais devem ser null
        const string snippet = """
            <ICMSSN900>
              <orig>0</orig>
              <CSOSN>900</CSOSN>
            </ICMSSN900>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn900 = icms.Should().BeOfType<IcmsSN900>().Subject;
        sn900.CSOSN.ToString().Should().Be("900");
        sn900.ModBC.Should().BeNull();
        sn900.VBC.Should().BeNull();
        sn900.PRedBC.Should().BeNull();
        sn900.PICMS.Should().BeNull();
        sn900.VICMS.Should().BeNull();
        sn900.ModBCST.Should().BeNull();
        sn900.PMVAST.Should().BeNull();
        sn900.PRedBCST.Should().BeNull();
        sn900.VBCST.Should().BeNull();
        sn900.PICMSST.Should().BeNull();
        sn900.VICMSST.Should().BeNull();
        sn900.VBCFCPST.Should().BeNull();
        sn900.PFCPST.Should().BeNull();
        sn900.VFCPST.Should().BeNull();
        sn900.PCredSN.Should().BeNull();
        sn900.VCredICMSSN.Should().BeNull();
    }

    [Fact]
    public async Task IcmsSN900_ApenasGrupoIcms_ParseiaCorretamente()
    {
        // Apenas grupo ICMS próprio preenchido, ST e crédito ausentes
        const string snippet = """
            <ICMSSN900>
              <orig>0</orig>
              <CSOSN>900</CSOSN>
              <modBC>3</modBC>
              <vBC>500.00</vBC>
              <pICMS>12.00</pICMS>
              <vICMS>60.00</vICMS>
            </ICMSSN900>
            """;

        var icms = await IcmsTestHelper.ParseIcmsAsync(snippet, TestContext.Current.CancellationToken);

        var sn900 = icms.Should().BeOfType<IcmsSN900>().Subject;
        sn900.ModBC.Should().Be(3);
        sn900.VBC.Should().Be(500.00m);
        sn900.PRedBC.Should().BeNull();
        sn900.PICMS.Should().Be(12.00m);
        sn900.VICMS.Should().Be(60.00m);
        sn900.ModBCST.Should().BeNull();
        sn900.PCredSN.Should().BeNull();
        sn900.VCredICMSSN.Should().BeNull();
    }
}
