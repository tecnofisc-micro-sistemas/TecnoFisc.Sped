using System.Text;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

public sealed class EventoTests
{
    [Fact]
    public void ReturnWithoutIdentityDoesNotProveCancellation()
    {
        var evt = new EventoCancelamento { ChaveAcesso = Core.ValueObjects.ChaveAcesso.Create(Key), TpEvento = 110111,
            NSeqEvento = 1, Retorno = new RetornoEvento { CStat = 135 } };
        evt.IsEffective.Should().BeFalse();
    }
    private const string Key = "31210711222333000181550050007156531436322400";
    [Theory]
    [InlineData(135, true)]
    [InlineData(155, true)]
    [InlineData(136, false)]
    [InlineData(573, false)]
    public async Task CancellationRequiresLinkedAcceptedReturn(int status, bool effective)
    {
        string xml = $"""
            <procEventoNFe versao="1.00" xmlns="http://www.portalfiscal.inf.br/nfe">
            <evento versao="1.00"><infEvento Id="ID110111{Key}01"><cOrgao>31</cOrgao><tpAmb>1</tpAmb><CNPJ>11222333000181</CNPJ><chNFe>{Key}</chNFe><dhEvento>2026-10-02T12:00:00-03:00</dhEvento><tpEvento>110111</tpEvento><nSeqEvento>1</nSeqEvento><verEvento>1.00</verEvento><detEvento versao="1.00"><descEvento>Cancelamento</descEvento><nProt>131214250732778</nProt><xJust>Justificativa de teste</xJust></detEvento></infEvento></evento>
            <retEvento><infEvento><chNFe>{Key}</chNFe><tpEvento>110111</tpEvento><nSeqEvento>1</nSeqEvento><cStat>{status}</cStat></infEvento></retEvento>
            </procEventoNFe>
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var result = await new ParserNFe().ReadEventoAsync(stream, TestContext.Current.CancellationToken);
        var evt = result.Should().BeOfType<EventoCancelamento>().Subject;
        evt.IsEffective.Should().Be(effective);
        evt.XJust.Should().Be("Justificativa de teste");
        evt.ChaveAcesso.ToString().Should().Be(Key);
    }

    [Fact]
    public async Task SerproStreamsNoteFullEventAndSummary()
    {
        await using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "nfe55-icms60-serpro.xml"));
        var docs = new List<Xml.Engine.IDocumentoFiscalXml>();
        await foreach (var doc in new ParserNFe().ReadDocumentsAsync(stream, TestContext.Current.CancellationToken)) docs.Add(doc);
        docs.Should().HaveCount(3);
        docs[0].Should().BeOfType<NFe>();
        docs.OfType<Evento>().Select(x => x.TpEvento).Should().Equal(210210, 610600);
        docs[2].ChaveAcesso.ToString().Should().Be(Key);
        ((Evento)docs[2]).NSeqEvento.Should().BeNull();
    }
}
