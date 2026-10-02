using System.Buffers.Binary;

namespace TecnoFisc.Sped.NFeNFCe.IO;

/// <summary>Inspeciona o diretório central antes de ZipArchive materializar suas entradas.</summary>
internal static class ZipMetadataGuard
{
    public static async Task ValidateAsync(Stream stream, ParserNFeOptions options, CancellationToken ct)
    {
        if (stream.Length < 22) throw new InvalidDataException("ZIP sem diretório central.");
        byte[] tail = new byte[(int)Math.Min(stream.Length, 65557)];
        stream.Position = stream.Length - tail.Length;
        await stream.ReadExactlyAsync(tail, ct).ConfigureAwait(false);
        int eocd = -1;
        for (int i = tail.Length - 22; i >= 0; i--)
            if (U32(tail, i) == 0x06054b50 && i + 22 + U16(tail, i + 20) == tail.Length) { eocd = i; break; }
        if (eocd < 0) throw new InvalidDataException("Fim do diretório central ZIP inválido.");
        if (U16(tail, eocd + 4) != 0 || U16(tail, eocd + 6) != 0 || U16(tail, eocd + 8) != U16(tail, eocd + 10))
            throw new InvalidDataException("ZIP multipart não suportado.");
        long count = U16(tail, eocd + 10), size = U32(tail, eocd + 12), start = U32(tail, eocd + 16);
        long absoluteEnd = stream.Length - tail.Length + eocd;
        if (count == ushort.MaxValue || size == uint.MaxValue || start == uint.MaxValue)
        {
            if (absoluteEnd < 20) throw new InvalidDataException("ZIP64 sem locator.");
            byte[] locator = new byte[20];
            stream.Position = absoluteEnd - 20;
            await stream.ReadExactlyAsync(locator, ct).ConfigureAwait(false);
            if (U32(locator, 0) != 0x07064b50 || U32(locator, 4) != 0 || U32(locator, 16) != 1)
                throw new InvalidDataException("ZIP64 multipart/locator inválido.");
            long offset = checked((long)U64(locator, 8));
            if (offset < 0 || offset > absoluteEnd - 76) throw new InvalidDataException("Offset ZIP64 inválido.");
            byte[] header = new byte[56];
            stream.Position = offset;
            await stream.ReadExactlyAsync(header, ct).ConfigureAwait(false);
            if (U32(header, 0) != 0x06064b50 || U64(header, 4) < 44 || U32(header, 16) != 0 || U32(header, 20) != 0
                || U64(header, 24) != U64(header, 32)) throw new InvalidDataException("Cabeçalho ZIP64 inválido.");
            count = checked((long)U64(header, 32)); size = checked((long)U64(header, 40)); start = checked((long)U64(header, 48));
            absoluteEnd = offset;
        }
        if (count > options.MaxZipEntries) throw new LimiteLeituraException("Quantidade de entradas ZIP excedida.");
        if (size > options.MaxZipCentralDirectoryBytes) throw new LimiteLeituraException("Diretório central ZIP excede o limite.");
        if (start < 0 || size < 0 || start > absoluteEnd || size > absoluteEnd - start)
            throw new InvalidDataException("Intervalo do diretório central ZIP inválido.");
        long end = start + size, actual = 0;
        byte[] entry = new byte[46];
        stream.Position = start;
        while (stream.Position < end)
        {
            ct.ThrowIfCancellationRequested();
            if (++actual > options.MaxZipEntries) throw new LimiteLeituraException("Quantidade real de entradas ZIP excedida.");
            if (end - stream.Position < 46) throw new InvalidDataException("Entrada do diretório central truncada.");
            await stream.ReadExactlyAsync(entry, ct).ConfigureAwait(false);
            if (U32(entry, 0) != 0x02014b50) throw new InvalidDataException("Assinatura do diretório central inválida.");
            if ((U16(entry, 8) & 1) != 0 || U16(entry, 34) != 0)
                throw new InvalidDataException("ZIP encriptado/multipart não suportado.");
            if (U16(entry, 10) is not (0 or 8)) throw new InvalidDataException("Método de compressão ZIP não suportado.");
            int skip = U16(entry, 28) + U16(entry, 30) + U16(entry, 32);
            if (skip > end - stream.Position) throw new InvalidDataException("Metadados da entrada ZIP truncados.");
            stream.Position += skip;
        }
        if (actual != count) throw new InvalidDataException("Contagem de entradas ZIP inconsistente.");
        stream.Position = 0;
    }
    private static ushort U16(byte[] buffer, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(offset));
    private static uint U32(byte[] buffer, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(offset));
    private static ulong U64(byte[] buffer, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(buffer.AsSpan(offset));
}
