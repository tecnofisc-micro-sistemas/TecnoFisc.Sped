using System.Text;
using TecnoFisc.Sped.NFeNFCe.Tests.Streaming;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

public sealed class GruposCompletosTests
{
    private static async Task<NFe> Parse(string xml)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return await new ParserNFe().ReadNFeAsync(stream, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DocumentGroups_PreserveListsAmountsAndText()
    {
        string xml = ReadLimitsTests.Fixture.Replace("</infNFe>", """
            <transp><modFrete>0</modFrete><vol><qVol>2</qVol><pesoB>1.250</pesoB><lacres><nLacre>0001</nLacre></lacres><lacres><nLacre>0002</nLacre></lacres></vol></transp>
            <cobr><fat><nFat>00042</nFat><vOrig>20.01</vOrig><vLiq>20.01</vLiq></fat><dup><nDup>001</nDup><dVenc>2026-10-02</dVenc><vDup>10.01</vDup></dup><dup><nDup>002</nDup><vDup>10.00</vDup></dup></cobr>
            <pag><detPag><tPag>03</tPag><vPag>20.01</vPag><card><tpIntegra>2</tpIntegra><cAut>00009</cAut></card></detPag><vTroco>0.00</vTroco></pag>
            <infAdic><infCpl>Texto adicional</infCpl><obsCont xCampo="A"><xTexto>primeiro</xTexto></obsCont><obsCont xCampo="B"><xTexto>segundo</xTexto></obsCont></infAdic>
            </infNFe>
            """, StringComparison.Ordinal);
        var note = await Parse(xml);
        note.Transp!.Vol[0].Lacres.Select(x => x.NLacre).Should().Equal("0001", "0002");
        note.Transp.Vol[0].PesoB.Should().Be(1.250m);
        note.Cobr!.Dup.Should().HaveCount(2);
        note.Cobr.Dup[0].DVenc.Should().Be(new DateOnly(2026, 10, 2));
        note.Cobr.Fat!.NFat.Should().Be("00042");
        note.Pag!.VTroco.Should().Be(0m);
        note.Pag.DetPag[0].Card!.CAut.Should().Be("00009");
        note.InfAdic!.ObsCont[1].XCampo.Should().Be("B");
    }

    [Fact]
    public async Task SpecializedProductAndRtc_PreserveNestedData()
    {
        string xml = ReadLimitsTests.Fixture.Replace("</prod>", """
            <vFrete>0.00</vFrete><rastro><nLote>0007</nLote><qLote>2.125</qLote><dFab>2026-01-01</dFab><dVal>2027-01-01</dVal></rastro>
            <med><cProdANVISA>ISENTO</cProdANVISA><xMotivoIsencao>Teste</xMotivoIsencao><vPMC>42.35</vPMC></med></prod>
            """, StringComparison.Ordinal).Replace("</imposto>", """
            <IBSCBS><CST>000</CST><cClassTrib>000001</cClassTrib><gIBSCBS><vBC>100.01</vBC><gIBSUF><pIBSUF>0.1000</pIBSUF><vIBSUF>0.10</vIBSUF></gIBSUF><gIBSMun><pIBSMun>0</pIBSMun><vIBSMun>0</vIBSMun></gIBSMun><vIBS>0.10</vIBS><gCBS><pCBS>0.9000</pCBS><vCBS>0.90</vCBS></gCBS></gIBSCBS></IBSCBS></imposto>
            """, StringComparison.Ordinal);
        var note = await Parse(xml);
        var product = note.Itens[0].Prod;
        product.Rastro[0].NLote.Should().Be("0007");
        product.Rastro[0].QLote.Should().Be(2.125m);
        product.Med!.VPMC.Should().Be(42.35m);
        product.VFrete.Should().Be(0m);
        product.VSeg.Should().BeNull();
        var rtc = note.Itens[0].Imposto.IBSCBS!;
        rtc.CST.Should().Be("000");
        rtc.CClassTrib.Should().Be("000001");
        rtc.GIBSCBS!.VBC.Should().Be(100.01m);
        rtc.GIBSCBS.GCBS!.VCBS.Should().Be(0.90m);
    }
}
