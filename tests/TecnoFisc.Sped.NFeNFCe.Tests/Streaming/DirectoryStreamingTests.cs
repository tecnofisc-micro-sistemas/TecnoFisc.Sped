using System.Text;

namespace TecnoFisc.Sped.NFeNFCe.Tests.Streaming;

public sealed class DirectoryStreamingTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "nfe-tests-" + Guid.NewGuid().ToString("N"));
    public DirectoryStreamingTests() => Directory.CreateDirectory(_directory);
    public void Dispose() => Directory.Delete(_directory, true);

    [Fact]
    public async Task ContinuesAfterBadXmlAndReportsOrigin()
    {
        await File.WriteAllTextAsync(Path.Combine(_directory, "a.XML"), ReadLimitsTests.Fixture, TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(_directory, "bad.xml"), "<bad>", TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(_directory, "z.xml"), ReadLimitsTests.Fixture, TestContext.Current.CancellationToken);
        var parser = new ParserNFe(new() { FailFast = false, BufferCapacity = 1, Parallelism = 2 });
        var results = new List<ResultadoImportacaoNFe>();
        await foreach (var result in parser.ReadDirectoryAsync(_directory, cancelamento: TestContext.Current.CancellationToken)) results.Add(result);
        results.Count(x => x.IsSuccess).Should().Be(2);
        var failed = results.Single(x => !x.IsSuccess);
        failed.Origem.Arquivo.Should().EndWith("bad.xml");
        failed.Erro.Should().BeOfType<System.Xml.XmlException>();
    }

    [Fact]
    public async Task EarlyDisposeStopsLazyProducerAndReleasesFiles()
    {
        string path = Path.Combine(_directory, "note.xml");
        await File.WriteAllTextAsync(path, ReadLimitsTests.Fixture, Encoding.UTF8, TestContext.Current.CancellationToken);
        int discovered = 0;
        IEnumerable<string> Paths()
        {
            for (int i = 0; i < 10000; i++) { Interlocked.Increment(ref discovered); yield return path; }
        }
        var parser = new ParserNFe(new() { BufferCapacity = 1, Parallelism = 2 });
        var iterator = parser.ReadFilesAsync(Paths(), TestContext.Current.CancellationToken).GetAsyncEnumerator(TestContext.Current.CancellationToken);
        (await iterator.MoveNextAsync()).Should().BeTrue();
        await iterator.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        discovered.Should().BeLessThan(20);
        using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        exclusive.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task FailFastPreservesParseException()
    {
        string path = Path.Combine(_directory, "bad.xml");
        await File.WriteAllTextAsync(path, "<bad>", TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<System.Xml.XmlException>(async () =>
        {
            await foreach (var result in new ParserNFe().ReadFilesAsync([path], TestContext.Current.CancellationToken)) _ = result;
        });
    }
}

