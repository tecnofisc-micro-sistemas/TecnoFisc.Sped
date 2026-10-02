using System.IO.Compression;
using System.Runtime.CompilerServices;
using TecnoFisc.Sped.NFeNFCe.IO;

namespace TecnoFisc.Sped.NFeNFCe;

public sealed partial class ParserNFe
{
    /// <summary>Lê entradas XML sequencialmente. Streams não posicionáveis usam spool limitado em disco.</summary>
    public IAsyncEnumerable<ResultadoImportacaoNFe> ReadZipAsync(Stream entrada, string origem, CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        ArgumentException.ThrowIfNullOrWhiteSpace(origem);
        return ImportZipAsync(entrada, origem, cancelamento);
    }

    private async IAsyncEnumerable<ResultadoImportacaoNFe> ImportZipAsync(Stream input, string origin, [EnumeratorCancellation] CancellationToken ct)
    {
        TemporarySeekableStream? storage = null;
        ZipArchive? zip = null;
        try
        {
            Exception? archiveError = null;
            try
            {
                storage = await TemporarySeekableStream.CreateAsync(input, Options, ct).ConfigureAwait(false);
                await ZipMetadataGuard.ValidateAsync(storage.Stream, Options, ct).ConfigureAwait(false);
                zip = new ZipArchive(storage.Stream, ZipArchiveMode.Read, true);
                _ = zip.Entries.Count; // Safe only after the bounded metadata scan.
            }
            catch (Exception ex) when (!Options.FailFast && IsZipError(ex)) { archiveError = ex; }
            if (archiveError is not null)
            { yield return ResultadoImportacaoNFe.Failure(new(origin), archiveError); yield break; }
            long total = 0;
            foreach (var entry in zip!.Entries)
            {
                ct.ThrowIfCancellationRequested();
                var source = new OrigemDocumentoNFe(origin, entry.FullName);
                if (IsExtension(entry.FullName, ".zip"))
                {
                    var error = new InvalidDataException("ZIP aninhado não suportado.");
                    if (Options.FailFast)
                        throw error;
                    yield return ResultadoImportacaoNFe.Failure(source, error);
                    continue;
                }
                if (!IsExtension(entry.FullName, ".xml"))
                    continue;
                Stream? data = null;
                Exception? openError = null;
                try
                { data = entry.Open(); }
                catch (Exception ex) when (!Options.FailFast && IsZipError(ex)) { openError = ex; }
                if (openError is not null)
                { yield return ResultadoImportacaoNFe.Failure(source, openError); continue; }
                await using (data!.ConfigureAwait(false))
                {
                    long remaining = Options.MaxZipUncompressedBytes - total;
                    using var bounded = new LimitedReadStream(data!, Math.Min(Options.MaxZipEntryBytes, remaining), true, ct, entry.Crc32, entry.Length);
                    try
                    {
                        await foreach (var result in ReadXmlResultsAsync(bounded, source, ct).ConfigureAwait(false))
                            yield return result;
                    }
                    finally { total += bounded.BytesRead; }
                    if (total > Options.MaxZipUncompressedBytes)
                        yield break;
                }
            }
        }
        finally
        {
            zip?.Dispose();
            if (storage is not null)
                await storage.DisposeAsync().ConfigureAwait(false);
        }
    }
    private static bool IsZipError(Exception error) => IsDocumentError(error) || error is EndOfStreamException;
}
