using System.Text;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

public sealed class EventBoundaryTests
{
    private const string Key = "31210711222333000181550050007156531436322400";
    private const string OtherKey = "35261012ABC34501DE35550010000000011000000011";
    private static string Request => $"<evento><versao>1.00</versao><infEvento><chNFe>{Key}</chNFe><tpEvento>110111</tpEvento><nSeqEvento>1</nSeqEvento><detEvento versao=\"1.00\"><descEvento>Cancelamento</descEvento><nProt>1</nProt><xJust>Justificativa de teste</xJust></detEvento></infEvento></evento>";

    [Fact]
    public async Task EventVersionAsChildAndDetailsVersionArePreserved()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Request));
        var result = (EventoCancelamento)await new ParserNFe().ReadEventoAsync(stream, TestContext.Current.CancellationToken);
        result.Versao.Should().Be("1.00");
        result.VersaoDetalhes.Should().Be("1.00");
        result.IsEffective.Should().BeFalse();
    }

    [Fact]
    public async Task EmptyEventDoesNotConsumeItsSibling()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<NFeLog><evento/>" + Request + "</NFeLog>"));
        await Assert.ThrowsAsync<FormatException>(async () =>
        {
            await foreach (var item in new ParserNFe().ReadDocumentsAsync(stream, TestContext.Current.CancellationToken)) _ = item;
        });
    }

    [Theory]
    [InlineData(OtherKey, 110111, 1)]
    [InlineData(Key, 110110, 1)]
    [InlineData(Key, 110111, 2)]
    public async Task DivergentReturnIdentityIsRejected(string key, int type, int sequence)
    {
        string xml = $"<procEventoNFe><retEvento><infEvento><chNFe>{key}</chNFe><tpEvento>{type}</tpEvento><nSeqEvento>{sequence}</nSeqEvento><cStat>135</cStat></infEvento></retEvento>{Request}</procEventoNFe>";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        await Assert.ThrowsAsync<FormatException>(() => new ParserNFe().ReadEventoAsync(stream, TestContext.Current.CancellationToken));
    }
}
