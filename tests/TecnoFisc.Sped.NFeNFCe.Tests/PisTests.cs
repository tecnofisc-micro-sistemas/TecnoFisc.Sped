namespace TecnoFisc.Sped.NFeNFCe.Tests;

/// <summary>
/// Testes unitários do grupo PIS (slice 14.4): valida o parser para as variantes
/// PISAliq, PISQtde, PISNT, PISOutr (ambas as formas do choice interno) e PISST,
/// além dos casos de ausência de PIS e PISST.
/// </summary>
public sealed class PisTests
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
    // PISAliq — CST 01, ad valorem
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PisAliq_ParseiaCorretamente()
    {
        const string pisSnippet = """
            <PIS>
              <PISAliq>
                <CST>01</CST>
                <vBC>1000.00</vBC>
                <pPIS>0.65</pPIS>
                <vPIS>6.50</vPIS>
              </PISAliq>
            </PIS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + pisSnippet,
            TestContext.Current.CancellationToken);

        imposto.Pis.Should().NotBeNull();
        var aliq = imposto.Pis.Should().BeOfType<PisAliq>().Subject;
        aliq.CST.ToString().Should().Be("01");
        aliq.VBC.Should().Be(1000.00m);
        aliq.PPIS.Should().Be(0.65m);
        aliq.VPIS.Should().Be(6.50m);
    }

    [Fact]
    public async Task PisAliq_Cst02_ParseiaCorretamente()
    {
        const string pisSnippet = """
            <PIS>
              <PISAliq>
                <CST>02</CST>
                <vBC>500.00</vBC>
                <pPIS>1.30</pPIS>
                <vPIS>6.50</vPIS>
              </PISAliq>
            </PIS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + pisSnippet,
            TestContext.Current.CancellationToken);

        var aliq = imposto.Pis.Should().BeOfType<PisAliq>().Subject;
        aliq.CST.ToString().Should().Be("02");
        aliq.VBC.Should().Be(500.00m);
        aliq.PPIS.Should().Be(1.30m);
        aliq.VPIS.Should().Be(6.50m);
    }

    // -------------------------------------------------------------------------
    // PISQtde — CST 03, específico
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PisQtde_ParseiaCorretamente()
    {
        const string pisSnippet = """
            <PIS>
              <PISQtde>
                <CST>03</CST>
                <qBCProd>200.0000</qBCProd>
                <vAliqProd>0.0500</vAliqProd>
                <vPIS>10.00</vPIS>
              </PISQtde>
            </PIS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + pisSnippet,
            TestContext.Current.CancellationToken);

        imposto.Pis.Should().NotBeNull();
        var qtde = imposto.Pis.Should().BeOfType<PisQtde>().Subject;
        qtde.CST.ToString().Should().Be("03");
        qtde.QBCProd.Should().Be(200.0000m);
        qtde.VAliqProd.Should().Be(0.0500m);
        qtde.VPIS.Should().Be(10.00m);
    }

    // -------------------------------------------------------------------------
    // PISNT — não tributado
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PisNt_Cst07_ParseiaCorretamente()
    {
        const string pisSnippet = """
            <PIS>
              <PISNT>
                <CST>07</CST>
              </PISNT>
            </PIS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + pisSnippet,
            TestContext.Current.CancellationToken);

        imposto.Pis.Should().NotBeNull();
        var nt = imposto.Pis.Should().BeOfType<PisNt>().Subject;
        nt.CST.ToString().Should().Be("07");
    }

    [Fact]
    public async Task PisNt_Cst04_ParseiaCorretamente()
    {
        const string pisSnippet = """
            <PIS>
              <PISNT>
                <CST>04</CST>
              </PISNT>
            </PIS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + pisSnippet,
            TestContext.Current.CancellationToken);

        var nt = imposto.Pis.Should().BeOfType<PisNt>().Subject;
        nt.CST.ToString().Should().Be("04");
    }

    // -------------------------------------------------------------------------
    // PISOutr — forma percentual (vBC + pPIS)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PisOutr_FormaPercentual_ParseiaCorretamente()
    {
        const string pisSnippet = """
            <PIS>
              <PISOutr>
                <CST>99</CST>
                <vBC>800.00</vBC>
                <pPIS>0.65</pPIS>
                <vPIS>5.20</vPIS>
              </PISOutr>
            </PIS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + pisSnippet,
            TestContext.Current.CancellationToken);

        imposto.Pis.Should().NotBeNull();
        var outr = imposto.Pis.Should().BeOfType<PisOutr>().Subject;
        outr.CST.ToString().Should().Be("99");
        outr.VBC.Should().Be(800.00m);
        outr.PPIS.Should().Be(0.65m);
        outr.QBCProd.Should().BeNull();
        outr.VAliqProd.Should().BeNull();
        outr.VPIS.Should().Be(5.20m);
    }

    // -------------------------------------------------------------------------
    // PISOutr — forma específica (qBCProd + vAliqProd)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PisOutr_FormaEspecifica_ParseiaCorretamente()
    {
        const string pisSnippet = """
            <PIS>
              <PISOutr>
                <CST>49</CST>
                <qBCProd>100.0000</qBCProd>
                <vAliqProd>0.0500</vAliqProd>
                <vPIS>5.00</vPIS>
              </PISOutr>
            </PIS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + pisSnippet,
            TestContext.Current.CancellationToken);

        imposto.Pis.Should().NotBeNull();
        var outr = imposto.Pis.Should().BeOfType<PisOutr>().Subject;
        outr.CST.ToString().Should().Be("49");
        outr.VBC.Should().BeNull();
        outr.PPIS.Should().BeNull();
        outr.QBCProd.Should().Be(100.0000m);
        outr.VAliqProd.Should().Be(0.0500m);
        outr.VPIS.Should().Be(5.00m);
    }

    // -------------------------------------------------------------------------
    // PIS ausente — Pis deve ser null
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SemPis_ImpostoPisNulo()
    {
        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo,
            TestContext.Current.CancellationToken);

        imposto.Pis.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // PISST — forma percentual (vBC + pPIS) sem indSomaPISST
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PisSt_FormaPercentual_SemIndSoma_ParseiaCorretamente()
    {
        const string pisSnippet = """
            <PIS>
              <PISAliq>
                <CST>01</CST>
                <vBC>1000.00</vBC>
                <pPIS>0.65</pPIS>
                <vPIS>6.50</vPIS>
              </PISAliq>
            </PIS>
            <PISST>
              <vBC>500.00</vBC>
              <pPIS>0.65</pPIS>
              <vPIS>3.25</vPIS>
            </PISST>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + pisSnippet,
            TestContext.Current.CancellationToken);

        imposto.PisSt.Should().NotBeNull();
        var st = imposto.PisSt!;
        st.VBC.Should().Be(500.00m);
        st.PPIS.Should().Be(0.65m);
        st.QBCProd.Should().BeNull();
        st.VAliqProd.Should().BeNull();
        st.VPIS.Should().Be(3.25m);
        st.IndSomaPISST.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // PISST — forma específica (qBCProd + vAliqProd) com indSomaPISST
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PisSt_FormaEspecifica_ComIndSoma_ParseiaCorretamente()
    {
        const string pisSnippet = """
            <PIS>
              <PISAliq>
                <CST>01</CST>
                <vBC>1000.00</vBC>
                <pPIS>0.65</pPIS>
                <vPIS>6.50</vPIS>
              </PISAliq>
            </PIS>
            <PISST>
              <qBCProd>50.0000</qBCProd>
              <vAliqProd>0.0800</vAliqProd>
              <vPIS>4.00</vPIS>
              <indSomaPISST>1</indSomaPISST>
            </PISST>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + pisSnippet,
            TestContext.Current.CancellationToken);

        imposto.PisSt.Should().NotBeNull();
        var st = imposto.PisSt!;
        st.VBC.Should().BeNull();
        st.PPIS.Should().BeNull();
        st.QBCProd.Should().Be(50.0000m);
        st.VAliqProd.Should().Be(0.0800m);
        st.VPIS.Should().Be(4.00m);
        st.IndSomaPISST.Should().Be(1);
    }

    // -------------------------------------------------------------------------
    // PISST ausente — PisSt deve ser null
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SemPisSt_ImpostoPisStNulo()
    {
        const string pisSnippet = """
            <PIS>
              <PISAliq>
                <CST>01</CST>
                <vBC>1000.00</vBC>
                <pPIS>0.65</pPIS>
                <vPIS>6.50</vPIS>
              </PISAliq>
            </PIS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + pisSnippet,
            TestContext.Current.CancellationToken);

        imposto.PisSt.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // Ordem independente — PISST antes de PIS
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PisStAntesDePis_OrdemIndependente()
    {
        const string pisSnippet = """
            <PISST>
              <vBC>200.00</vBC>
              <pPIS>0.65</pPIS>
              <vPIS>1.30</vPIS>
            </PISST>
            <PIS>
              <PISAliq>
                <CST>01</CST>
                <vBC>200.00</vBC>
                <pPIS>0.65</pPIS>
                <vPIS>1.30</vPIS>
              </PISAliq>
            </PIS>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + pisSnippet,
            TestContext.Current.CancellationToken);

        imposto.Pis.Should().BeOfType<PisAliq>();
        imposto.PisSt.Should().NotBeNull();
        imposto.PisSt!.VBC.Should().Be(200.00m);
        imposto.PisSt.VPIS.Should().Be(1.30m);
    }

    // -------------------------------------------------------------------------
    // indSomaPISST = 0
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PisSt_IndSomaPISST_Zero_ParseiaCorretamente()
    {
        const string pisSnippet = """
            <PIS>
              <PISAliq>
                <CST>01</CST>
                <vBC>100.00</vBC>
                <pPIS>0.65</pPIS>
                <vPIS>0.65</vPIS>
              </PISAliq>
            </PIS>
            <PISST>
              <vBC>100.00</vBC>
              <pPIS>0.65</pPIS>
              <vPIS>0.65</vPIS>
              <indSomaPISST>0</indSomaPISST>
            </PISST>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + pisSnippet,
            TestContext.Current.CancellationToken);

        imposto.PisSt!.IndSomaPISST.Should().Be(0);
    }
}
