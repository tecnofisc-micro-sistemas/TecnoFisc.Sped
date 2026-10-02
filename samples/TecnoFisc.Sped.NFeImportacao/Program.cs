using TecnoFisc.Sped.NFeNFCe;

if (args.Length != 1)
{
    Console.Error.WriteLine("Uso: dotnet run --project samples/TecnoFisc.Sped.NFeImportacao -- <diretorio>");
    return 1;
}

using var stop = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.Cancel(); };
var parser = new ParserNFe(new() { FailFast = false, Parallelism = 4, BufferCapacity = 16 });
var batch = new List<ResultadoImportacaoNFe>(500);
int failures = 0;
try
{
    await foreach (var result in parser.ReadDirectoryAsync(args[0], recursivo: true, cancelamento: stop.Token))
    {
        if (!result.IsSuccess)
        {
            failures++;
            Console.Error.WriteLine($"{result.Origem.Arquivo}::{result.Origem.EntradaZip}: {result.Erro!.Message}");
            continue;
        }
        batch.Add(result);
        if (batch.Count == 500)
        {
            await DeliverBatchAsync(batch, stop.Token);
            batch.Clear();
        }
    }
    if (batch.Count != 0) await DeliverBatchAsync(batch, stop.Token);
}
catch (OperationCanceledException) when (stop.IsCancellationRequested)
{
    Console.Error.WriteLine("Importação interrompida.");
    return 130;
}
return failures == 0 ? 0 : 1;

static Task DeliverBatchAsync(IReadOnlyList<ResultadoImportacaoNFe> batch, CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested();
    int nfe = 0, nfce = 0, events = 0;
    foreach (var result in batch)
    {
        switch (result.Documento)
        {
            case NFe: nfe++; break;
            case NFCe: nfce++; break;
            case Evento: events++; break;
        }
    }
    // Substituir este destino demonstrativo por um await do repositório FiscTax.
    // O consumidor controla transação, deduplicação por chave e persistência de eventos.
    // Não reter batch após retornar: a lista é reutilizada na próxima iteração.
    Console.WriteLine($"Lote disponível: {nfe} NF-e, {nfce} NFC-e, {events} eventos. Nenhuma gravação em banco neste exemplo.");
    return Task.CompletedTask;
}
