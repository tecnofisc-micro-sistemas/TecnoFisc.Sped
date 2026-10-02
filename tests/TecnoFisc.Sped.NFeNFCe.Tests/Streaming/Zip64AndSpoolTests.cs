using System.Buffers.Binary;

namespace TecnoFisc.Sped.NFeNFCe.Tests.Streaming;

public sealed class Zip64AndSpoolTests
{
    private static byte[] Zip64()
    {
        using var archive = ZipStreamingTests.Archive(("a.xml", ReadLimitsTests.Fixture));
        byte[] original = archive.ToArray();
        int end = original.Length - 22;
        byte[] result = new byte[original.Length + 76];
        original.AsSpan(0, end).CopyTo(result);
        original.AsSpan(end).CopyTo(result.AsSpan(end + 76));
        var header = result.AsSpan(end, 56);
        BinaryPrimitives.WriteUInt32LittleEndian(header, 0x06064b50);
        BinaryPrimitives.WriteUInt64LittleEndian(header[4..], 44);
        BinaryPrimitives.WriteUInt16LittleEndian(header[12..], 45);
        BinaryPrimitives.WriteUInt16LittleEndian(header[14..], 45);
        BinaryPrimitives.WriteUInt64LittleEndian(header[24..], 1);
        BinaryPrimitives.WriteUInt64LittleEndian(header[32..], 1);
        BinaryPrimitives.WriteUInt64LittleEndian(header[40..], BinaryPrimitives.ReadUInt32LittleEndian(original.AsSpan(end + 12)));
        BinaryPrimitives.WriteUInt64LittleEndian(header[48..], BinaryPrimitives.ReadUInt32LittleEndian(original.AsSpan(end + 16)));
        var locator = result.AsSpan(end + 56, 20);
        BinaryPrimitives.WriteUInt32LittleEndian(locator, 0x07064b50);
        BinaryPrimitives.WriteUInt64LittleEndian(locator[8..], (ulong)end);
        BinaryPrimitives.WriteUInt32LittleEndian(locator[16..], 1);
        result.AsSpan(end + 76 + 8, 12).Fill(0xff);
        return result;
    }

    [Fact]
    public async Task Zip64EndRecordsAreSupported()
    {
        using var stream = new MemoryStream(Zip64());
        int read = 0;
        await foreach (var result in new ParserNFe().ReadZipAsync(stream, "zip64", TestContext.Current.CancellationToken))
        { result.IsSuccess.Should().BeTrue(); read++; }
        read.Should().Be(1);
    }

    [Fact]
    public async Task Zip64ExtendedRecordCannotRunPastLocator()
    {
        byte[] bytes = Zip64();
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(bytes.Length - 98 + 4), ulong.MaxValue);
        using var stream = new MemoryStream(bytes);
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await foreach (var result in new ParserNFe().ReadZipAsync(stream, "zip64", TestContext.Current.CancellationToken)) _ = result;
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SpoolExactLimitAndOverflowAlwaysCleanTemporaryFiles(bool overflow)
    {
        string directory = Path.Combine(Path.GetTempPath(), "nfe-spool-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using var archive = ZipStreamingTests.Archive(("a.xml", ReadLimitsTests.Fixture));
            byte[] bytes = archive.ToArray();
            using var stream = new ZipStreamingTests.NonSeekableStream(bytes);
            var parser = new ParserNFe(new() { MaxZipSpoolBytes = bytes.Length - (overflow ? 1 : 0), TemporaryDirectory = directory, FailFast = false });
            var results = new List<ResultadoImportacaoNFe>();
            await foreach (var result in parser.ReadZipAsync(stream, "upload", TestContext.Current.CancellationToken)) results.Add(result);
            results.Should().ContainSingle();
            results[0].IsSuccess.Should().Be(!overflow);
            if (overflow) results[0].Erro.Should().BeOfType<LimiteLeituraException>();
            Directory.EnumerateFiles(directory).Should().BeEmpty();
            stream.CanRead.Should().BeTrue();
        }
        finally { Directory.Delete(directory); }
    }

    [Fact]
    public async Task CancelBlockedSpoolCleansTemporaryFileAndLeavesInputOpen()
    {
        string directory = Path.Combine(Path.GetTempPath(), "nfe-spool-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using var stream = new BlockingUpload();
            using var stop = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            var parser = new ParserNFe(new() { TemporaryDirectory = directory });
            await using var enumerator = parser.ReadZipAsync(stream, "upload", stop.Token).GetAsyncEnumerator(stop.Token);
            Task<bool> pending = enumerator.MoveNextAsync().AsTask();
            await stream.Started.Task.WaitAsync(TestContext.Current.CancellationToken);
            await stop.CancelAsync();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
            Directory.EnumerateFiles(directory).Should().BeEmpty();
            stream.CanRead.Should().BeTrue();
        }
        finally { Directory.Delete(directory); }
    }

    private sealed class BlockingUpload : MemoryStream
    {
        public override bool CanSeek => false;
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return 0;
        }
    }
}
