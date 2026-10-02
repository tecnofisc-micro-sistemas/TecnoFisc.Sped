namespace TecnoFisc.Sped.NFeNFCe.IO;

/// <summary>Contagem efetiva e cancelamento inclusive quando XmlReader não fornece token.</summary>
internal sealed class LimitedReadStream(Stream inner, long maxBytes, bool leaveOpen, CancellationToken cancellationToken,
    uint? expectedCrc = null, long? expectedLength = null) : Stream
{
    private long _read;
    private uint _crc = uint.MaxValue;
    public long BytesRead => _read;
    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => _read; set => throw new NotSupportedException(); }
    private int Count(ReadOnlySpan<byte> bytes, int count)
    {
        _read += count;
        if (_read > maxBytes)
            throw new LimiteLeituraException($"Limite de {maxBytes} bytes excedido.");
        if (expectedCrc is { } crc)
        {
            foreach (byte value in bytes[..count])
                _crc = CrcTable.Values[(_crc ^ value) & 255] ^ (_crc >> 8);
            if (_read > expectedLength || count == 0 && _read != expectedLength)
                throw new InvalidDataException("Tamanho descompactado ZIP difere do diretório central.");
            if (_read == expectedLength && ~_crc != crc)
                throw new InvalidDataException("CRC32 da entrada ZIP inválido.");
        }
        return count;
    }
    private int Request(int size)
    {
        long remaining = Math.Max(0, maxBytes - _read);
        return (int)Math.Min(size, remaining >= int.MaxValue ? int.MaxValue : remaining + 1);
    }
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    public override int Read(Span<byte> buffer)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (buffer.IsEmpty)
            return 0;
        int read = inner.Read(buffer[..Request(buffer.Length)]);
        return Count(buffer, read);
    }
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken readToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (buffer.IsEmpty)
            return 0;
        if (!readToken.CanBeCanceled || readToken == cancellationToken)
        {
            int read = await inner.ReadAsync(buffer[..Request(buffer.Length)], cancellationToken).ConfigureAwait(false);
            return Count(buffer.Span, read);
        }
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, readToken);
        int count = await inner.ReadAsync(buffer[..Request(buffer.Length)], linked.Token).ConfigureAwait(false);
        return Count(buffer.Span, count);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing && !leaveOpen)
            inner.Dispose();
        base.Dispose(disposing);
    }
    public override void Flush() => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    private static class CrcTable
    {
        internal static readonly uint[] Values = Create();
        private static uint[] Create()
        {
            var values = new uint[256];
            for (uint i = 0; i < values.Length; i++)
            {
                uint crc = i;
                for (int bit = 0; bit < 8; bit++)
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xedb88320U : crc >> 1;
                values[i] = crc;
            }
            return values;
        }
    }
}
