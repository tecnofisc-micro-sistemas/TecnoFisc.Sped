using System.IO.Compression;
using System.Text;

namespace TecnoFisc.Sped.NFeNFCe.Tests.Streaming;

public sealed class ZipStreamingTests
{
    internal static MemoryStream Archive(params (string Name, string Xml)[] entries)
    {
        var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            foreach (var (name, xml) in entries)
            {
                using var entry = zip.CreateEntry(name).Open();
                entry.Write(Encoding.UTF8.GetBytes(xml));
            }
        }
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task MixedEntriesAndInvalidXml_ContinueWithoutExtraction()
    {
        using var stream = Archive(("../note.XML", ReadLimitsTests.Fixture), ("note.xml", "<bad>"), ("note.xml", ReadLimitsTests.Fixture), ("readme.txt", "ignored"), ("nested.zip", "unsupported"));
        var parser = new ParserNFe(new() { FailFast = false });
        var results = new List<ResultadoImportacaoNFe>();
        await foreach (var result in parser.ReadZipAsync(stream, "batch.zip", TestContext.Current.CancellationToken)) results.Add(result);
        results.Count(x => x.IsSuccess).Should().Be(2);
        results.Count(x => !x.IsSuccess).Should().Be(2);
        results[0].Origem.Should().Be(new OrigemDocumentoNFe("batch.zip", "../note.XML"));
        stream.CanRead.Should().BeTrue();
    }

    [Fact]
    public async Task NonSeekableInputSpoolsAndCleansUpOnEarlyDispose()
    {
        string temp = Path.Combine(Path.GetTempPath(), "nfe-spool-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            using var zip = Archive(("a.xml", ReadLimitsTests.Fixture), ("b.xml", ReadLimitsTests.Fixture));
            using var input = new NonSeekableStream(zip.ToArray());
            var parser = new ParserNFe(new() { TemporaryDirectory = temp });
            await foreach (var result in parser.ReadZipAsync(input, "upload", TestContext.Current.CancellationToken))
            { result.IsSuccess.Should().BeTrue(); break; }
            Directory.EnumerateFiles(temp).Should().BeEmpty();
            input.CanRead.Should().BeTrue();
        }
        finally { Directory.Delete(temp, true); }
    }

    [Fact]
    public async Task EntryAndArchiveBudgetsAreEnforced()
    {
        using var zip = Archive(("a.xml", ReadLimitsTests.Fixture), ("b.xml", ReadLimitsTests.Fixture));
        await Assert.ThrowsAsync<LimiteLeituraException>(async () =>
        {
            await foreach (var r in new ParserNFe(new() { MaxZipEntries = 1 }).ReadZipAsync(zip, "zip", TestContext.Current.CancellationToken)) _ = r;
        });
        zip.Position = 0;
        var results = new List<ResultadoImportacaoNFe>();
        await foreach (var r in new ParserNFe(new() { MaxZipEntryBytes = 50, FailFast = false }).ReadZipAsync(zip, "zip", TestContext.Current.CancellationToken)) results.Add(r);
        results.Should().HaveCount(2).And.OnlyContain(x => x.Erro is LimiteLeituraException);
    }

    internal sealed class NonSeekableStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin loc) => throw new NotSupportedException();
    }
}
