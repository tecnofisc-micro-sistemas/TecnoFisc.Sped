using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Xml;
using TecnoFisc.Sped.Xml.Engine;

namespace TecnoFisc.Sped.NFeNFCe;

public sealed partial class ParserNFe
{
    /// <summary>Enumera arquivos sem materializar caminhos; ordem de conclusão não garantida.</summary>
    public IAsyncEnumerable<ResultadoImportacaoNFe> ReadDirectoryAsync(string caminho, bool recursivo = false, CancellationToken cancelamento = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caminho);
        return ReadFilesAsync(Directory.EnumerateFiles(caminho, "*", new EnumerationOptions
        {
            RecurseSubdirectories = recursivo,
            IgnoreInaccessible = false,
            AttributesToSkip = FileAttributes.ReparsePoint,
        }).Where(p => IsExtension(p, ".xml") || IsExtension(p, ".zip")), cancelamento);
    }

    public IAsyncEnumerable<ResultadoImportacaoNFe> ReadFilesAsync(IEnumerable<string> caminhos, CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(caminhos);
        return ImportFilesAsync(caminhos, cancelamento);
    }

    private static bool IsExtension(string path, string extension) => Path.GetExtension(path).Equals(extension, StringComparison.OrdinalIgnoreCase);
    internal static bool IsDocumentError(Exception error) => error is XmlException or FormatException or OverflowException or LimiteLeituraException or InvalidDataException;

    private async IAsyncEnumerable<ResultadoImportacaoNFe> ImportFilesAsync(IEnumerable<string> paths, [EnumeratorCancellation] CancellationToken ct)
    {
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var input = Channel.CreateBounded<string>(new BoundedChannelOptions(Options.BufferCapacity) { SingleWriter = true, FullMode = BoundedChannelFullMode.Wait });
        var output = Channel.CreateBounded<ResultadoImportacaoNFe>(new BoundedChannelOptions(Options.BufferCapacity) { SingleReader = true, FullMode = BoundedChannelFullMode.Wait });
        Exception? failure = null;

        async Task GuardAsync(Func<Task> action)
        {
            try
            { await action().ConfigureAwait(false); }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
            catch (Exception error)
            {
                Interlocked.CompareExchange(ref failure, error, null);
                await stop.CancelAsync().ConfigureAwait(false);
            }
        }
        async Task ProduceAsync()
        {
            try
            {
                foreach (string path in paths)
                {
                    stop.Token.ThrowIfCancellationRequested();
                    ArgumentException.ThrowIfNullOrWhiteSpace(path);
                    await input.Writer.WriteAsync(path, stop.Token).ConfigureAwait(false);
                }
            }
            finally { input.Writer.TryComplete(); }
        }
        async Task ConsumeAsync()
        {
            await foreach (string path in input.Reader.ReadAllAsync(stop.Token).ConfigureAwait(false))
            {
                if (!IsExtension(path, ".xml") && !IsExtension(path, ".zip"))
                {
                    var error = new FormatException($"Extensão não suportada: {Path.GetExtension(path)}.");
                    if (Options.FailFast)
                        throw error;
                    await output.Writer.WriteAsync(ResultadoImportacaoNFe.Failure(new(path), error), stop.Token).ConfigureAwait(false);
                    continue;
                }
                await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.Asynchronous | FileOptions.SequentialScan);
                var results = IsExtension(path, ".zip") ? ReadZipAsync(stream, path, stop.Token) : ReadXmlResultsAsync(stream, new(path), stop.Token);
                await foreach (var result in results.ConfigureAwait(false))
                    await output.Writer.WriteAsync(result, stop.Token).ConfigureAwait(false);
            }
        }
        var tasks = new Task[Options.Parallelism + 1];
        tasks[0] = Task.Run(() => GuardAsync(ProduceAsync), CancellationToken.None);
        for (int i = 1; i < tasks.Length; i++)
            tasks[i] = Task.Run(() => GuardAsync(ConsumeAsync), CancellationToken.None);
        async Task FinishAsync()
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
            output.Writer.TryComplete(failure);
        }
        Task finished = FinishAsync();
        try
        {
            await foreach (var result in output.Reader.ReadAllAsync(ct).ConfigureAwait(false))
                yield return result;
        }
        finally
        {
            await stop.CancelAsync().ConfigureAwait(false);
            await finished.ConfigureAwait(false);
        }
    }

    internal async IAsyncEnumerable<ResultadoImportacaoNFe> ReadXmlResultsAsync(Stream stream, OrigemDocumentoNFe origin, [EnumeratorCancellation] CancellationToken ct)
    {
        await using var iterator = ReadDocumentsAsync(stream, ct).GetAsyncEnumerator(ct);
        bool found = false;
        while (true)
        {
            Exception? error = null;
            IDocumentoFiscalXml? document = null;
            bool moved = false;
            try
            {
                moved = await iterator.MoveNextAsync().ConfigureAwait(false);
                if (moved)
                    document = iterator.Current;
                else if (!found)
                    throw new FormatException("XML não contém documentos fiscais suportados.");
            }
            catch (Exception ex) when (!Options.FailFast && IsDocumentError(ex)) { error = ex; }
            if (error is not null)
            { yield return ResultadoImportacaoNFe.Failure(origin, error); yield break; }
            if (!moved)
                yield break;
            found = true;
            yield return ResultadoImportacaoNFe.Success(origin, document!);
        }
    }
}
