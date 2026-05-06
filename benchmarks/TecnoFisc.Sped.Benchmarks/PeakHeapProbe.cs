using TecnoFisc.Sped.EfdContribuicoes.Parser;

namespace TecnoFisc.Sped.Benchmarks;

/// <summary>
/// Sonda standalone (fora do BenchmarkDotNet) que mede o pico de memória viva no heap
/// gerenciado durante as duas estratégias de leitura. Complementa o
/// <see cref="StreamingVsBufferedBenchmark"/>: a coluna <c>Allocated</c> do BDN reporta total
/// alocado (que é praticamente igual nas duas), enquanto esta sonda captura o que realmente
/// difere — o working set retido durante a operação.
/// </summary>
/// <remarks>
/// O pico é amostrado periodicamente em uma thread em segundo plano via
/// <see cref="GC.GetTotalMemory(bool)"/>. A medição não é exata como um profiler, mas é
/// suficiente para evidenciar que o streaming permanece O(1) enquanto o buffered cresce O(N).
/// Acionada via <c>dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks -- --probe peak</c>.
/// </remarks>
internal static class PeakHeapProbe
{
    public static async Task ExecutarAsync()
    {
        int[] tamanhos = [10_000, 100_000, 1_000_000];

        Console.WriteLine();
        Console.WriteLine("Pico de memória viva (heap gerenciado) — streaming vs buffered");
        Console.WriteLine("Total alocado (BDN Allocated) é parecido nas duas estratégias;");
        Console.WriteLine("o pico abaixo é o que realmente importa para arquivos grandes.");
        Console.WriteLine();
        Console.WriteLine($"{"QtdRegistros",14} | {"Streaming pico",18} | {"Buffered pico",18} | {"Razão Buffered/Streaming",26}");
        Console.WriteLine(new string('-', 86));

        foreach (var n in tamanhos)
        {
            var bytes = StreamingVsBufferedBenchmark.GerarArquivoSinteticoBloco9(n);

            // Aquece JIT e estabiliza heap antes da primeira medição.
            await ExecutarStreamingAsync(bytes);
            await ExecutarBufferedAsync(bytes);

            long picoStreaming = await MedirPicoAsync(() => ExecutarStreamingAsync(bytes));
            long picoBuffered = await MedirPicoAsync(() => ExecutarBufferedAsync(bytes));
            double razao = picoStreaming == 0 ? 0 : (double)picoBuffered / picoStreaming;

            Console.WriteLine(
                $"{n,14:N0} | {FormatarBytes(picoStreaming),18} | {FormatarBytes(picoBuffered),18} | {razao,26:F2}x");
        }

        Console.WriteLine();
        Console.WriteLine("Esperado: razão Buffered/Streaming cresce quase linearmente com QtdRegistros.");
    }

    private static async Task<long> MedirPicoAsync(Func<Task> operacao)
    {
        // Linha de base limpa antes de medir.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long baseline = GC.GetTotalMemory(forceFullCollection: true);

        long pico = baseline;
        using var fimDaOperacao = new CancellationTokenSource();
        var amostragem = Task.Run(async () =>
        {
            while (!fimDaOperacao.IsCancellationRequested)
            {
                long atual = GC.GetTotalMemory(forceFullCollection: false);
                if (atual > pico) pico = atual;
                try { await Task.Delay(2, fimDaOperacao.Token).ConfigureAwait(false); }
                catch (OperationCanceledException) { return; }
            }
        });

        await operacao().ConfigureAwait(false);
        fimDaOperacao.Cancel();
        try { await amostragem.ConfigureAwait(false); }
        catch (OperationCanceledException) { /* esperado */ }

        return pico - baseline;
    }

    private static async Task ExecutarStreamingAsync(byte[] bytes)
    {
        var parser = new ParserEfdContribuicoes();
        using var stream = new MemoryStream(bytes, writable: false);
        int total = 0;
        await foreach (var _ in parser.LerStreamingAsync(stream))
            total++;
        GC.KeepAlive(total);
    }

    private static async Task ExecutarBufferedAsync(byte[] bytes)
    {
        var parser = new ParserEfdContribuicoes();
        using var stream = new MemoryStream(bytes, writable: false);
        var arquivo = await parser.LerAsync(stream);
        GC.KeepAlive(arquivo);
    }

    private static string FormatarBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024L * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}
