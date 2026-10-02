using System.Text;
using TecnoFisc.Sped.NFeNFCe.Tests.Streaming;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

public sealed class DocumentoRoutingTests
{
    [Fact]
    public async Task MultipleNotes_StreamAll_ButSingularRejects()
    {
        var unit = ReadLimitsTests.Fixture;
        unit = unit[(unit.IndexOf("<nfeProc", StringComparison.Ordinal))..];
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<NFeLog>" + unit + unit + "</NFeLog>"));
        var parser = new ParserNFe();
        int count = 0;
        await foreach (var doc in parser.ReadDocumentsAsync(stream, TestContext.Current.CancellationToken))
        {
            doc.Should().BeOfType<NFe>();
            count++;
        }
        count.Should().Be(2);
        stream.Position = 0;
        await Assert.ThrowsAsync<FormatException>(() => parser.ReadNFeAsync(stream, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WrongProtocolKeyIsRejected()
    {
        string xml = ReadLimitsTests.Fixture.Replace("<chNFe>31210711222333000181550050007156531436322400</chNFe>",
            "<chNFe>31210711222333000181550050007156531436322419</chNFe>", StringComparison.Ordinal);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        await Assert.ThrowsAsync<FormatException>(() => new ParserNFe().ReadNFeAsync(stream, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task NFCe_IsDistinct_AndKeepsSupplement()
    {
        string oldKey = "31210711222333000181550050007156531436322400";
        string baseKey = oldKey[..20] + "65" + oldKey[22..43];
        int sum = 0, weight = 2;
        for (int i = 42; i >= 0; i--) { sum += (baseKey[i] - '0') * weight; weight = weight == 9 ? 2 : weight + 1; }
        int digit = 11 - sum % 11;
        string key = baseKey + (digit >= 10 ? 0 : digit).ToString(System.Globalization.CultureInfo.InvariantCulture);
        string xml = ReadLimitsTests.Fixture.Replace(oldKey, key, StringComparison.Ordinal)
            .Replace("<mod>55</mod>", "<mod>65</mod>", StringComparison.Ordinal)
            .Replace("</NFe>", "<infNFeSupl><qrCode><![CDATA[https://example.test/?p=123&v=2]]></qrCode><urlChave>https://example.test/</urlChave></infNFeSupl></NFe>", StringComparison.Ordinal);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var result = await new ParserNFe().ReadAsync(stream, TestContext.Current.CancellationToken);
        var note = result.Should().BeOfType<NFCe>().Subject;
        note.InfNFeSupl!.QrCode.Should().Be("https://example.test/?p=123&v=2");
        note.Itens.Should().HaveCount(2);
        note.IsAutorizacaoEfetiva.Should().BeTrue();
        stream.Position = 0;
        await Assert.ThrowsAsync<FormatException>(() => new ParserNFe().ReadNFeAsync(stream, TestContext.Current.CancellationToken));
    }
}
