namespace TecnoFisc.Sped.NFeNFCe.Tests;

/// <summary>
/// Testes unitários do grupo IPI (slice 14.4): valida o parser para as variantes
/// IPITrib (ad valorem e específico) e IPINT, além de campos comuns e vTotTrib.
/// </summary>
public sealed class IpiTests
{
    // Snippet ICMS00 mínimo reutilizado como "companheiro" no imposto quando necessário.
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
    // Sem IPI — imposto.Ipi deve ser null
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SemIpi_ImpostoIpiNulo()
    {
        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo,
            TestContext.Current.CancellationToken);

        imposto.Ipi.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // IPITrib — ad valorem (vBC + pIPI)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task IpiTrib_AdValorem_ParseiaCorretamente()
    {
        const string ipiSnippet = """
            <IPI>
              <cEnq>999</cEnq>
              <IPITrib>
                <CST>50</CST>
                <vBC>1000.00</vBC>
                <pIPI>5.00</pIPI>
                <vIPI>50.00</vIPI>
              </IPITrib>
            </IPI>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + ipiSnippet,
            TestContext.Current.CancellationToken);

        imposto.Ipi.Should().NotBeNull();
        var ipi = imposto.Ipi!;
        ipi.CEnq.Should().Be("999");
        ipi.CNPJProd.Should().BeNull();
        ipi.CSelo.Should().BeNull();
        ipi.QSelo.Should().BeNull();

        var trib = ipi.Tributacao.Should().BeOfType<IpiTrib>().Subject;
        trib.CST.ToString().Should().Be("50");
        trib.VBC.Should().Be(1000.00m);
        trib.PIPI.Should().Be(5.00m);
        trib.QUnid.Should().BeNull();
        trib.VUnid.Should().BeNull();
        trib.VIPI.Should().Be(50.00m);
    }

    // -------------------------------------------------------------------------
    // IPITrib — específico (qUnid + vUnid)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task IpiTrib_Especifico_ParseiaCorretamente()
    {
        const string ipiSnippet = """
            <IPI>
              <cEnq>999</cEnq>
              <IPITrib>
                <CST>99</CST>
                <qUnid>100.0000</qUnid>
                <vUnid>0.5000</vUnid>
                <vIPI>50.00</vIPI>
              </IPITrib>
            </IPI>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + ipiSnippet,
            TestContext.Current.CancellationToken);

        imposto.Ipi.Should().NotBeNull();
        var ipi = imposto.Ipi!;
        ipi.CEnq.Should().Be("999");

        var trib = ipi.Tributacao.Should().BeOfType<IpiTrib>().Subject;
        trib.CST.ToString().Should().Be("99");
        trib.VBC.Should().BeNull();
        trib.PIPI.Should().BeNull();
        trib.QUnid.Should().Be(100.0000m);
        trib.VUnid.Should().Be(0.5000m);
        trib.VIPI.Should().Be(50.00m);
    }

    // -------------------------------------------------------------------------
    // IPITrib — campos comuns (CNPJProd, cSelo, qSelo)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task IpiTrib_CamposComuns_ParseiaCorretamente()
    {
        const string ipiSnippet = """
            <IPI>
              <CNPJProd>11222333000181</CNPJProd>
              <cSelo>ABC-123</cSelo>
              <qSelo>10</qSelo>
              <cEnq>001</cEnq>
              <IPITrib>
                <CST>00</CST>
                <vBC>500.00</vBC>
                <pIPI>10.00</pIPI>
                <vIPI>50.00</vIPI>
              </IPITrib>
            </IPI>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + ipiSnippet,
            TestContext.Current.CancellationToken);

        var ipi = imposto.Ipi!;
        ipi.CNPJProd.Should().NotBeNull();
        ipi.CNPJProd!.Value.ToString().Should().Be("11222333000181");
        ipi.CSelo.Should().Be("ABC-123");
        ipi.QSelo.Should().Be(10);
        ipi.CEnq.Should().Be("001");

        var trib = ipi.Tributacao.Should().BeOfType<IpiTrib>().Subject;
        trib.CST.ToString().Should().Be("00");
        trib.VIPI.Should().Be(50.00m);
    }

    // -------------------------------------------------------------------------
    // qSelo com 12 dígitos — regressão: int transbordaria (max 10 dígitos)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task QSelo_12Digitos_ParseiaComoLong()
    {
        // 123456789012 > int.MaxValue (2147483647); o tipo correto é long.
        const string ipiSnippet = """
            <IPI>
              <qSelo>123456789012</qSelo>
              <cEnq>001</cEnq>
              <IPITrib>
                <CST>50</CST>
                <vBC>0.00</vBC>
                <pIPI>0.00</pIPI>
                <vIPI>0.00</vIPI>
              </IPITrib>
            </IPI>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + ipiSnippet,
            TestContext.Current.CancellationToken);

        imposto.Ipi!.QSelo.Should().Be(123456789012L);
    }

    // -------------------------------------------------------------------------
    // IPINT — não tributado
    // -------------------------------------------------------------------------

    [Fact]
    public async Task IpiNt_ParseiaCorretamente()
    {
        const string ipiSnippet = """
            <IPI>
              <cEnq>999</cEnq>
              <IPINT>
                <CST>53</CST>
              </IPINT>
            </IPI>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + ipiSnippet,
            TestContext.Current.CancellationToken);

        imposto.Ipi.Should().NotBeNull();
        var ipi = imposto.Ipi!;
        ipi.CEnq.Should().Be("999");

        var nt = ipi.Tributacao.Should().BeOfType<IpiNt>().Subject;
        nt.CST.ToString().Should().Be("53");
    }

    // -------------------------------------------------------------------------
    // vTotTrib
    // -------------------------------------------------------------------------

    [Fact]
    public async Task VTotTrib_Presente_ParseiaCorretamente()
    {
        const string ipiSnippet = """
            <IPI>
              <cEnq>999</cEnq>
              <IPITrib>
                <CST>50</CST>
                <vBC>1000.00</vBC>
                <pIPI>5.00</pIPI>
                <vIPI>50.00</vIPI>
              </IPITrib>
            </IPI>
            <vTotTrib>123.45</vTotTrib>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo + ipiSnippet,
            TestContext.Current.CancellationToken);

        imposto.VTotTrib.Should().Be(123.45m);
    }

    [Fact]
    public async Task VTotTrib_Ausente_Nulo()
    {
        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            Icms00Minimo,
            TestContext.Current.CancellationToken);

        imposto.VTotTrib.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // Ordem independente — IPI antes de ICMS
    // -------------------------------------------------------------------------

    [Fact]
    public async Task IpiAntesDe_Icms_OrdemIndependente()
    {
        // O parser é order-independent: IPI antes de ICMS deve funcionar igualmente.
        const string ipiSnippet = """
            <IPI>
              <cEnq>999</cEnq>
              <IPITrib>
                <CST>49</CST>
                <vBC>200.00</vBC>
                <pIPI>3.00</pIPI>
                <vIPI>6.00</vIPI>
              </IPITrib>
            </IPI>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            ipiSnippet + Icms00Minimo,
            TestContext.Current.CancellationToken);

        imposto.Icms.Should().NotBeNull();
        imposto.Ipi.Should().NotBeNull();
        var trib = imposto.Ipi!.Tributacao.Should().BeOfType<IpiTrib>().Subject;
        trib.CST.ToString().Should().Be("49");
        trib.VIPI.Should().Be(6.00m);
    }
}
