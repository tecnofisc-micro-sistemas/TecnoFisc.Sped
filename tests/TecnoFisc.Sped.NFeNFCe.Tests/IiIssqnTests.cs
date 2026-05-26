namespace TecnoFisc.Sped.NFeNFCe.Tests;

/// <summary>
/// Testes unitários dos grupos <c>II</c> (Imposto de Importação) e <c>ISSQN</c> (slice 14.4).
/// </summary>
public sealed class IiIssqnTests
{
    // =========================================================================
    // II — Imposto de Importação
    // =========================================================================

    [Fact]
    public async Task Ii_TodosCampos_ParseiaCorretamente()
    {
        const string snippet = """
            <II>
              <vBC>5000.00</vBC>
              <vDespAdu>120.50</vDespAdu>
              <vII>250.00</vII>
              <vIOF>15.75</vIOF>
            </II>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            snippet,
            TestContext.Current.CancellationToken);

        imposto.Ii.Should().NotBeNull();
        var ii = imposto.Ii!;
        ii.VBC.Should().Be(5000.00m);
        ii.VDespAdu.Should().Be(120.50m);
        ii.VII.Should().Be(250.00m);
        ii.VIOF.Should().Be(15.75m);
    }

    [Fact]
    public async Task Ii_Ausente_ImpostoIiNulo()
    {
        // Nenhum grupo <II> → Imposto.Ii deve ser null.
        const string snippet = """
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

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            snippet,
            TestContext.Current.CancellationToken);

        imposto.Ii.Should().BeNull();
    }

    // =========================================================================
    // ISSQN
    // =========================================================================

    [Fact]
    public async Task Issqn_TodosCamposObrigatoriosMaisOpcionais_ParseiaCorretamente()
    {
        const string snippet = """
            <ISSQN>
              <vBC>2000.00</vBC>
              <vAliq>5.00</vAliq>
              <vISSQN>100.00</vISSQN>
              <cMunFG>3550308</cMunFG>
              <cListServ>14.05</cListServ>
              <vDeducao>50.00</vDeducao>
              <vOutro>10.00</vOutro>
              <vDescIncond>20.00</vDescIncond>
              <vDescCond>5.00</vDescCond>
              <vISSRet>30.00</vISSRet>
              <indISS>1</indISS>
              <cServico>0101</cServico>
              <cMun>3550308</cMun>
              <cPais>1058</cPais>
              <nProcesso>PROC-2024-001</nProcesso>
              <indIncentivo>2</indIncentivo>
            </ISSQN>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            snippet,
            TestContext.Current.CancellationToken);

        imposto.Issqn.Should().NotBeNull();
        var issqn = imposto.Issqn!;

        // Campos obrigatórios
        issqn.VBC.Should().Be(2000.00m);
        issqn.VAliq.Should().Be(5.00m);
        issqn.VISSQN.Should().Be(100.00m);
        issqn.CMunFG.ToString().Should().Be("3550308");
        issqn.CListServ.Should().Be("14.05");
        issqn.IndISS.Should().Be(1);
        issqn.IndIncentivo.Should().Be(2);

        // Campos opcionais presentes
        issqn.VDeducao.Should().Be(50.00m);
        issqn.VOutro.Should().Be(10.00m);
        issqn.VDescIncond.Should().Be(20.00m);
        issqn.VDescCond.Should().Be(5.00m);
        issqn.VISSRet.Should().Be(30.00m);
        issqn.CServico.Should().Be("0101");
        issqn.CMun.Should().NotBeNull();
        issqn.CMun!.Value.ToString().Should().Be("3550308");
        issqn.CPais.Should().Be(1058);
        issqn.NProcesso.Should().Be("PROC-2024-001");
    }

    [Fact]
    public async Task Issqn_SomenteObrigatorios_OpcionaisNulos()
    {
        const string snippet = """
            <ISSQN>
              <vBC>1500.00</vBC>
              <vAliq>3.00</vAliq>
              <vISSQN>45.00</vISSQN>
              <cMunFG>3304557</cMunFG>
              <cListServ>01.01</cListServ>
              <indISS>2</indISS>
              <indIncentivo>1</indIncentivo>
            </ISSQN>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            snippet,
            TestContext.Current.CancellationToken);

        imposto.Issqn.Should().NotBeNull();
        var issqn = imposto.Issqn!;

        issqn.VBC.Should().Be(1500.00m);
        issqn.VAliq.Should().Be(3.00m);
        issqn.VISSQN.Should().Be(45.00m);
        issqn.CMunFG.ToString().Should().Be("3304557");
        issqn.CListServ.Should().Be("01.01");
        issqn.IndISS.Should().Be(2);
        issqn.IndIncentivo.Should().Be(1);

        // Todos os opcionais devem ser null
        issqn.VDeducao.Should().BeNull();
        issqn.VOutro.Should().BeNull();
        issqn.VDescIncond.Should().BeNull();
        issqn.VDescCond.Should().BeNull();
        issqn.VISSRet.Should().BeNull();
        issqn.CServico.Should().BeNull();
        issqn.CMun.Should().BeNull();
        issqn.CPais.Should().BeNull();
        issqn.NProcesso.Should().BeNull();
    }

    [Fact]
    public async Task Issqn_Ausente_ImpostoIssqnNulo()
    {
        const string snippet = """
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

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            snippet,
            TestContext.Current.CancellationToken);

        imposto.Issqn.Should().BeNull();
    }

    [Fact]
    public async Task Issqn_CMunFG_CMun_RoundTripCodigoMunicipioIbge()
    {
        // Verifica que CMunFG e CMun são desserializados via CodigoMunicipioIbge e preservam o valor.
        const string snippet = """
            <ISSQN>
              <vBC>800.00</vBC>
              <vAliq>2.00</vAliq>
              <vISSQN>16.00</vISSQN>
              <cMunFG>5208707</cMunFG>
              <cListServ>07.02</cListServ>
              <indISS>3</indISS>
              <cMun>5208707</cMun>
              <indIncentivo>2</indIncentivo>
            </ISSQN>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            snippet,
            TestContext.Current.CancellationToken);

        var issqn = imposto.Issqn!;
        issqn.CMunFG.ToString().Should().Be("5208707");
        issqn.CMun.Should().NotBeNull();
        issqn.CMun!.Value.ToString().Should().Be("5208707");
    }

    // =========================================================================
    // Ordem independente — II/ISSQN antes de ICMS
    // =========================================================================

    [Fact]
    public async Task IiAntesDe_Icms_OrdemIndependente()
    {
        const string snippet = """
            <II>
              <vBC>3000.00</vBC>
              <vDespAdu>80.00</vDespAdu>
              <vII>150.00</vII>
              <vIOF>9.00</vIOF>
            </II>
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

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            snippet,
            TestContext.Current.CancellationToken);

        imposto.Icms.Should().NotBeNull();
        imposto.Ii.Should().NotBeNull();
        imposto.Ii!.VII.Should().Be(150.00m);
        imposto.Ii.VIOF.Should().Be(9.00m);
    }

    [Fact]
    public async Task IssqnAntesDe_Ipi_OrdemIndependente()
    {
        const string snippet = """
            <ISSQN>
              <vBC>1200.00</vBC>
              <vAliq>4.00</vAliq>
              <vISSQN>48.00</vISSQN>
              <cMunFG>2927408</cMunFG>
              <cListServ>10.09</cListServ>
              <indISS>1</indISS>
              <indIncentivo>2</indIncentivo>
            </ISSQN>
            <IPI>
              <cEnq>999</cEnq>
              <IPITrib>
                <CST>50</CST>
                <vBC>0.00</vBC>
                <pIPI>0.00</pIPI>
                <vIPI>0.00</vIPI>
              </IPITrib>
            </IPI>
            """;

        var imposto = await IcmsTestHelper.ParseImpostoAsync(
            snippet,
            TestContext.Current.CancellationToken);

        imposto.Issqn.Should().NotBeNull();
        imposto.Ipi.Should().NotBeNull();
        imposto.Issqn!.VISSQN.Should().Be(48.00m);
        imposto.Issqn.CMunFG.ToString().Should().Be("2927408");
    }
}
