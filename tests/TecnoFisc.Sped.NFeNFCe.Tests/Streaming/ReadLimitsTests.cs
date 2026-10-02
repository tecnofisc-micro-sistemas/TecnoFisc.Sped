using System.Text;

namespace TecnoFisc.Sped.NFeNFCe.Tests.Streaming;

public sealed class ReadLimitsTests
{
    internal static string Fixture => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "nfe55-icms60-canonico.xml"));

    [Fact]
    public async Task ExactLimitWorks_OneByteLessFails_LeavesInputOpen()
    {
        byte[] bytes = Encoding.UTF8.GetBytes(Fixture);
        using var stream = new MemoryStream(bytes);
        var parser = new ParserNFe(new() { MaxXmlBytes = bytes.Length });
        var note = await parser.ReadNFeAsync(stream, TestContext.Current.CancellationToken);
        note.Itens.Should().HaveCount(2);
        stream.CanRead.Should().BeTrue();
        stream.Position = 0;
        parser = new ParserNFe(new() { MaxXmlBytes = bytes.Length - 1 });
        await Assert.ThrowsAsync<LimiteLeituraException>(() => parser.ReadNFeAsync(stream, TestContext.Current.CancellationToken));
        stream.CanRead.Should().BeTrue();
    }

    [Fact]
    public async Task DepthAppliesToUnknownSubtrees()
    {
        var xml = Fixture.Replace("</infNFe>", "<novo><a><b><c><d/></c></b></a></novo></infNFe>", StringComparison.Ordinal);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var parser = new ParserNFe(new() { MaxXmlDepth = 7 });
        await Assert.ThrowsAsync<LimiteLeituraException>(() => parser.ReadNFeAsync(stream, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task IgnoredElementCallbackDoesNotCollectPayload()
    {
        var names = new List<string>();
        var xml = Fixture.Replace("</infNFe>", "<novo>payload</novo></infNFe>", StringComparison.Ordinal);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var parser = new ParserNFe(new() { OnIgnoredElement = field => names.Add(field.Nome) });
        await parser.ReadNFeAsync(stream, TestContext.Current.CancellationToken);
        names.Should().Contain("novo");
    }

    [Fact]
    public async Task CancellationReachesBlockedRead()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var stream = new BlockingStream();
        var task = new ParserNFe().ReadNFeAsync(stream, cancellation.Token);
        await stream.Started.Task.WaitAsync(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
    }

    private sealed class BlockingStream : MemoryStream
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return 0;
        }
    }
}
