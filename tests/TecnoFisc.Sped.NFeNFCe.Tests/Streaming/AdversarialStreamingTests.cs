using System.Buffers.Binary;
using System.Text;

namespace TecnoFisc.Sped.NFeNFCe.Tests.Streaming;

public sealed class AdversarialStreamingTests
{
    [Fact]
    public async Task WrongZipChecksumIsReported()
    {
        using var original = ZipStreamingTests.Archive(("a.xml", ReadLimitsTests.Fixture));
        byte[] bytes = original.ToArray();
        for (int i = 0; i < bytes.Length - 46; i++)
            if (BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(i)) == 0x02014b50)
            { bytes[i + 16] ^= 0xff; break; }
        using var input = new MemoryStream(bytes);
        bool failed = false;
        await foreach (var result in new ParserNFe(new() { FailFast = false }).ReadZipAsync(input, "zip", TestContext.Current.CancellationToken))
            failed |= !result.IsSuccess;
        failed.Should().BeTrue();
    }

    [Fact]
    public async Task ForeignNamespaceCannotSupplyFiscalIdentification()
    {
        string xml = ReadLimitsTests.Fixture.Replace("<infNFe ", "<infNFe xmlns=\"urn:foreign\" ", StringComparison.Ordinal);
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        await Assert.ThrowsAsync<FormatException>(() => new ParserNFe().ReadNFeAsync(input, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CallbackFailuresAreNotClassifiedAsBadXml()
    {
        string xml = ReadLimitsTests.Fixture.Replace("</infNFe>", "<unknown/></infNFe>", StringComparison.Ordinal);
        using var zip = ZipStreamingTests.Archive(("a.xml", xml));
        var parser = new ParserNFe(new() { FailFast = false, OnIgnoredElement = _ => throw new FormatException("callback") });
        var error = await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await foreach (var item in parser.ReadZipAsync(zip, "zip", TestContext.Current.CancellationToken)) _ = item;
        });
        (error.Message == "callback" || error.InnerException?.Message == "callback").Should().BeTrue();
    }

    [Fact]
    public async Task ForgedCentralCountIsRejectedBeforeEntriesAreMaterialized()
    {
        using var original = ZipStreamingTests.Archive(("a.xml", ReadLimitsTests.Fixture), ("b.xml", ReadLimitsTests.Fixture));
        byte[] bytes = original.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(bytes.Length - 22 + 8), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(bytes.Length - 22 + 10), 1);
        using var input = new MemoryStream(bytes);
        await Assert.ThrowsAsync<LimiteLeituraException>(async () =>
        {
            await foreach (var r in new ParserNFe(new() { MaxZipEntries = 1 }).ReadZipAsync(input, "zip", TestContext.Current.CancellationToken)) _ = r;
        });
    }

    [Fact]
    public async Task TotalBudgetStopsArchiveButNotNextFile()
    {
        using var input = ZipStreamingTests.Archive(("a.xml", ReadLimitsTests.Fixture), ("b.xml", ReadLimitsTests.Fixture), ("c.xml", ReadLimitsTests.Fixture));
        var results = new List<ResultadoImportacaoNFe>();
        var parser = new ParserNFe(new() { FailFast = false, MaxZipUncompressedBytes = Encoding.UTF8.GetByteCount(ReadLimitsTests.Fixture) + 10 });
        await foreach (var result in parser.ReadZipAsync(input, "zip", TestContext.Current.CancellationToken)) results.Add(result);
        results.Should().HaveCount(2);
        results[0].IsSuccess.Should().BeTrue();
        results[1].Erro.Should().BeOfType<LimiteLeituraException>();
    }

    [Fact]
    public async Task TruncatedXmlNeverProducesIncompleteNote()
    {
        string xml = ReadLimitsTests.Fixture[..ReadLimitsTests.Fixture.IndexOf("</infNFe>", StringComparison.Ordinal)];
        using var input = ZipStreamingTests.Archive(("a.xml", xml));
        await foreach (var result in new ParserNFe(new() { FailFast = false }).ReadZipAsync(input, "zip", TestContext.Current.CancellationToken))
            result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task NonSeekableXmlIsReadDirectly()
    {
        using var input = new ZipStreamingTests.NonSeekableStream(Encoding.UTF8.GetBytes(ReadLimitsTests.Fixture));
        var note = await new ParserNFe().ReadNFeAsync(input, TestContext.Current.CancellationToken);
        note.Itens.Should().HaveCount(2);
    }
}
