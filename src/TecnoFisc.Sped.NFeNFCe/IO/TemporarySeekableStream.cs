namespace TecnoFisc.Sped.NFeNFCe.IO;

internal sealed class TemporarySeekableStream : IAsyncDisposable
{
    public Stream Stream { get; }
    private readonly bool _owns;
    private TemporarySeekableStream(Stream stream, bool owns) { Stream = stream; _owns = owns; }
    public static async Task<TemporarySeekableStream> CreateAsync(Stream input, ParserNFeOptions options, CancellationToken ct)
    {
        if (input.CanSeek) return new(input, false);
        string directory = options.TemporaryDirectory ?? Path.GetTempPath();
        string path = Path.Combine(directory, "nfe-" + Guid.NewGuid().ToString("N") + ".zip");
        var file = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 65536,
            FileOptions.Asynchronous | FileOptions.DeleteOnClose);
        try
        {
            using var limited = new LimitedReadStream(input, options.MaxZipSpoolBytes, true, ct);
            await limited.CopyToAsync(file, 65536, ct).ConfigureAwait(false);
            file.Position = 0;
            return new(file, true);
        }
        catch { await file.DisposeAsync().ConfigureAwait(false); throw; }
    }
    public ValueTask DisposeAsync() => _owns ? Stream.DisposeAsync() : ValueTask.CompletedTask;
}
