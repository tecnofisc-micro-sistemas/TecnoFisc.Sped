namespace TecnoFisc.Sped.NFeNFCe.IO;

/// <summary>Contagem efetiva e cancelamento inclusive quando XmlReader não fornece token.</summary>
internal sealed class LimitedReadStream(Stream inner, long maxBytes, bool leaveOpen, CancellationToken cancellationToken) : Stream
{
    private long _read;
    public long BytesRead => _read;
    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => _read; set => throw new NotSupportedException(); }
    private int Count(int count)
    {
        _read += count;
        if (_read > maxBytes) throw new LimiteLeituraException($"Limite de {maxBytes} bytes excedido.");
        return count;
    }
    private int Request(int size) => (int)Math.Min(size, Math.Min(int.MaxValue, maxBytes - _read + 1));
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    public override int Read(Span<byte> buffer)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Count(inner.Read(buffer[..Request(buffer.Length)]));
    }
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken readToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!readToken.CanBeCanceled || readToken == cancellationToken)
            return Count(await inner.ReadAsync(buffer[..Request(buffer.Length)], cancellationToken).ConfigureAwait(false));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, readToken);
        return Count(await inner.ReadAsync(buffer[..Request(buffer.Length)], linked.Token).ConfigureAwait(false));
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing && !leaveOpen) inner.Dispose();
        base.Dispose(disposing);
    }
    public override void Flush() => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
