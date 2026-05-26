namespace TecnoFisc.Sped.NFeNFCe.Tests;

/// <summary>
/// Testes unitários do grupo COFINS (slice 14.4): valida o parser para as variantes
/// COFINSAliq, COFINSQtde, COFINSNT, COFINSOutr (ambas as formas do choice interno) e COFINSST,
/// além dos casos de ausência de COFINS e COFINSST.
/// </summary>
public sealed class CofinsTests
{
    // Snippet ICMS00 mínimo reutilizado como "companheiro" no imposto.
    private const string Icms00Minimo = """
        <ICMS>
          <ICMS00>
            <orig>0</orig>
            <CST>00</CST>
            <modBC>3</modBC>
            <vBC>0.00</vBC>
            <pICMS>0.00</pICMS>
            <vICMS>0.00</vICMS>
          </ICMS00>
        </ICMS>
        """;

    // -------------------------------------------------------------------------
    // COFINSAliq — CST 01, ad valorem
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CofinsAliq_ParseiaCorretamente()
    {
        const string cofinsSnippet = """
            <COFINS>
              <COFINSAliq>
                <CST>01</CST>
                <vBC>1000.00</vBC>
                <pCOFINS>3.00</pCOFINS>
                <vCOFINS>30.00</vCOFINS>
              </COFINSAliq>
            </COFINS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + cofinsSnippet,
            TestContext.Current.CancellationToken);

        imposto.Cofins.Should().NotBeNull();
        var aliq = imposto.Cofins.Should().BeOfType<CofinsAliq>().Subject;
        aliq.CST.ToString().Should().Be("01");
        aliq.VBC.Should().Be(1000.00m);
        aliq.PCOFINS.Should().Be(3.00m);
        aliq.VCOFINS.Should().Be(30.00m);
    }

    [Fact]
    public async Task CofinsAliq_Cst02_ParseiaCorretamente()
    {
        const string cofinsSnippet = """
            <COFINS>
              <COFINSAliq>
                <CST>02</CST>
                <vBC>500.00</vBC>
                <pCOFINS>6.00</pCOFINS>
                <vCOFINS>30.00</vCOFINS>
              </COFINSAliq>
            </COFINS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + cofinsSnippet,
            TestContext.Current.CancellationToken);

        var aliq = imposto.Cofins.Should().BeOfType<CofinsAliq>().Subject;
        aliq.CST.ToString().Should().Be("02");
        aliq.VBC.Should().Be(500.00m);
        aliq.PCOFINS.Should().Be(6.00m);
        aliq.VCOFINS.Should().Be(30.00m);
    }

    // -------------------------------------------------------------------------
    // COFINSQtde — CST 03, específico
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CofinsQtde_ParseiaCorretamente()
    {
        const string cofinsSnippet = """
            <COFINS>
              <COFINSQtde>
                <CST>03</CST>
                <qBCProd>200.0000</qBCProd>
                <vAliqProd>0.2300</vAliqProd>
                <vCOFINS>46.00</vCOFINS>
              </COFINSQtde>
            </COFINS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + cofinsSnippet,
            TestContext.Current.CancellationToken);

        imposto.Cofins.Should().NotBeNull();
        var qtde = imposto.Cofins.Should().BeOfType<CofinsQtde>().Subject;
        qtde.CST.ToString().Should().Be("03");
        qtde.QBCProd.Should().Be(200.0000m);
        qtde.VAliqProd.Should().Be(0.2300m);
        qtde.VCOFINS.Should().Be(46.00m);
    }

    // -------------------------------------------------------------------------
    // COFINSNT — não tributado
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CofinsNt_Cst07_ParseiaCorretamente()
    {
        const string cofinsSnippet = """
            <COFINS>
              <COFINSNT>
                <CST>07</CST>
              </COFINSNT>
            </COFINS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + cofinsSnippet,
            TestContext.Current.CancellationToken);

        imposto.Cofins.Should().NotBeNull();
        var nt = imposto.Cofins.Should().BeOfType<CofinsNt>().Subject;
        nt.CST.ToString().Should().Be("07");
    }

    [Fact]
    public async Task CofinsNt_Cst04_ParseiaCorretamente()
    {
        const string cofinsSnippet = """
            <COFINS>
              <COFINSNT>
                <CST>04</CST>
              </COFINSNT>
            </COFINS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + cofinsSnippet,
            TestContext.Current.CancellationToken);

        var nt = imposto.Cofins.Should().BeOfType<CofinsNt>().Subject;
        nt.CST.ToString().Should().Be("04");
    }

    // -------------------------------------------------------------------------
    // COFINSOutr — forma percentual (vBC + pCOFINS)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CofinsOutr_FormaPercentual_ParseiaCorretamente()
    {
        const string cofinsSnippet = """
            <COFINS>
              <COFINSOutr>
                <CST>99</CST>
                <vBC>800.00</vBC>
                <pCOFINS>3.00</pCOFINS>
                <vCOFINS>24.00</vCOFINS>
              </COFINSOutr>
            </COFINS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + cofinsSnippet,
            TestContext.Current.CancellationToken);

        imposto.Cofins.Should().NotBeNull();
        var outr = imposto.Cofins.Should().BeOfType<CofinsOutr>().Subject;
        outr.CST.ToString().Should().Be("99");
        outr.VBC.Should().Be(800.00m);
        outr.PCOFINS.Should().Be(3.00m);
        outr.QBCProd.Should().BeNull();
        outr.VAliqProd.Should().BeNull();
        outr.VCOFINS.Should().Be(24.00m);
    }

    // -------------------------------------------------------------------------
    // COFINSOutr — forma específica (qBCProd + vAliqProd)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CofinsOutr_FormaEspecifica_ParseiaCorretamente()
    {
        const string cofinsSnippet = """
            <COFINS>
              <COFINSOutr>
                <CST>49</CST>
                <qBCProd>100.0000</qBCProd>
                <vAliqProd>0.2300</vAliqProd>
                <vCOFINS>23.00</vCOFINS>
              </COFINSOutr>
            </COFINS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + cofinsSnippet,
            TestContext.Current.CancellationToken);

        imposto.Cofins.Should().NotBeNull();
        var outr = imposto.Cofins.Should().BeOfType<CofinsOutr>().Subject;
        outr.CST.ToString().Should().Be("49");
        outr.VBC.Should().BeNull();
        outr.PCOFINS.Should().BeNull();
        outr.QBCProd.Should().Be(100.0000m);
        outr.VAliqProd.Should().Be(0.2300m);
        outr.VCOFINS.Should().Be(23.00m);
    }

    // -------------------------------------------------------------------------
    // COFINS ausente — Cofins deve ser null
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SemCofins_ImpostoCofinsNulo()
    {
        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo,
            TestContext.Current.CancellationToken);

        imposto.Cofins.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // COFINSST — forma percentual (vBC + pCOFINS) sem indSomaCOFINSST
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CofinsSt_FormaPercentual_SemIndSoma_ParseiaCorretamente()
    {
        const string cofinsSnippet = """
            <COFINS>
              <COFINSAliq>
                <CST>01</CST>
                <vBC>1000.00</vBC>
                <pCOFINS>3.00</pCOFINS>
                <vCOFINS>30.00</vCOFINS>
              </COFINSAliq>
            </COFINS>
            <COFINSST>
              <vBC>500.00</vBC>
              <pCOFINS>3.00</pCOFINS>
              <vCOFINS>15.00</vCOFINS>
            </COFINSST>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + cofinsSnippet,
            TestContext.Current.CancellationToken);

        imposto.CofinsSt.Should().NotBeNull();
        var st = imposto.CofinsSt!;
        st.VBC.Should().Be(500.00m);
        st.PCOFINS.Should().Be(3.00m);
        st.QBCProd.Should().BeNull();
        st.VAliqProd.Should().BeNull();
        st.VCOFINS.Should().Be(15.00m);
        st.IndSomaCOFINSST.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // COFINSST — forma específica (qBCProd + vAliqProd) com indSomaCOFINSST
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CofinsSt_FormaEspecifica_ComIndSoma_ParseiaCorretamente()
    {
        const string cofinsSnippet = """
            <COFINS>
              <COFINSAliq>
                <CST>01</CST>
                <vBC>1000.00</vBC>
                <pCOFINS>3.00</pCOFINS>
                <vCOFINS>30.00</vCOFINS>
              </COFINSAliq>
            </COFINS>
            <COFINSST>
              <qBCProd>50.0000</qBCProd>
              <vAliqProd>0.2300</vAliqProd>
              <vCOFINS>11.50</vCOFINS>
              <indSomaCOFINSST>1</indSomaCOFINSST>
            </COFINSST>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + cofinsSnippet,
            TestContext.Current.CancellationToken);

        imposto.CofinsSt.Should().NotBeNull();
        var st = imposto.CofinsSt!;
        st.VBC.Should().BeNull();
        st.PCOFINS.Should().BeNull();
        st.QBCProd.Should().Be(50.0000m);
        st.VAliqProd.Should().Be(0.2300m);
        st.VCOFINS.Should().Be(11.50m);
        st.IndSomaCOFINSST.Should().Be(1);
    }

    // -------------------------------------------------------------------------
    // COFINSST ausente — CofinsSt deve ser null
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SemCofinsSt_ImpostoCofinsStNulo()
    {
        const string cofinsSnippet = """
            <COFINS>
              <COFINSAliq>
                <CST>01</CST>
                <vBC>1000.00</vBC>
                <pCOFINS>3.00</pCOFINS>
                <vCOFINS>30.00</vCOFINS>
              </COFINSAliq>
            </COFINS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + cofinsSnippet,
            TestContext.Current.CancellationToken);

        imposto.CofinsSt.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // Ordem independente — COFINSST antes de COFINS
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CofinsStAntesDeCofins_OrdemIndependente()
    {
        const string cofinsSnippet = """
            <COFINSST>
              <vBC>200.00</vBC>
              <pCOFINS>3.00</pCOFINS>
              <vCOFINS>6.00</vCOFINS>
            </COFINSST>
            <COFINS>
              <COFINSAliq>
                <CST>01</CST>
                <vBC>200.00</vBC>
                <pCOFINS>3.00</pCOFINS>
                <vCOFINS>6.00</vCOFINS>
              </COFINSAliq>
            </COFINS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + cofinsSnippet,
            TestContext.Current.CancellationToken);

        imposto.Cofins.Should().BeOfType<CofinsAliq>();
        imposto.CofinsSt.Should().NotBeNull();
        imposto.CofinsSt!.VBC.Should().Be(200.00m);
        imposto.CofinsSt.VCOFINS.Should().Be(6.00m);
    }

    // -------------------------------------------------------------------------
    // indSomaCOFINSST = 0
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CofinsSt_IndSomaCOFINSST_Zero_ParseiaCorretamente()
    {
        const string cofinsSnippet = """
            <COFINS>
              <COFINSAliq>
                <CST>01</CST>
                <vBC>100.00</vBC>
                <pCOFINS>3.00</pCOFINS>
                <vCOFINS>3.00</vCOFINS>
              </COFINSAliq>
            </COFINS>
            <COFINSST>
              <vBC>100.00</vBC>
              <pCOFINS>3.00</pCOFINS>
              <vCOFINS>3.00</vCOFINS>
              <indSomaCOFINSST>0</indSomaCOFINSST>
            </COFINSST>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + cofinsSnippet,
            TestContext.Current.CancellationToken);

        imposto.CofinsSt!.IndSomaCOFINSST.Should().Be(0);
    }
}
