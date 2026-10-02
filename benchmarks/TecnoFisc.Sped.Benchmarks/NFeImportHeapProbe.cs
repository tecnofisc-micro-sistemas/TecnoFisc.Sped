using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using TecnoFisc.Sped.NFeNFCe;

namespace TecnoFisc.Sped.Benchmarks;

internal static class NFeImportHeapProbe
{
    public static async Task RunAsync(string[] args)
    {
        int Option(string name, int fallback)
        {
            int index = Array.IndexOf(args, name);
            return index < 0 ? fallback : int.Parse(args[index + 1], CultureInfo.InvariantCulture);
        }
        int count = Option("--count", 10000), parallelism = Option("--parallelism", 1), capacity = Option("--buffer", 64), delay = Option("--delay-ms", 0);
        int delayEvery = Option("--delay-every", 1);
        if (count <= 0 || delayEvery <= 0 || delay < 0) throw new ArgumentOutOfRangeException(nameof(args));
        string path = Path.Combine(Path.GetTempPath(), "nfe-probe-" + Guid.NewGuid().ToString("N") + ".xml");
        await File.WriteAllTextAsync(path, ParserNFeBenchmark.GerarCanonico(1), Encoding.UTF8);
        try
        {
            var parser = new ParserNFe(new() { Parallelism = parallelism, BufferCapacity = capacity });
            using (var warm = File.OpenRead(path)) await parser.ReadNFeAsync(warm);
            long before = GC.GetTotalMemory(true), heapPeak = before, workingPeak = 0;
            long allocatedBefore = GC.GetTotalAllocatedBytes(true);
            int gen0 = GC.CollectionCount(0), gen1 = GC.CollectionCount(1), gen2 = GC.CollectionCount(2);
            using var process = Process.GetCurrentProcess();
            using var monitorStop = new CancellationTokenSource();
            async Task MonitorAsync()
            {
                try
                {
                    while (true)
                    {
                        heapPeak = Math.Max(heapPeak, GC.GetTotalMemory(false));
                        process.Refresh(); workingPeak = Math.Max(workingPeak, process.WorkingSet64);
                        await Task.Delay(25, monitorStop.Token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) when (monitorStop.IsCancellationRequested) { }
            }
            Task monitor = MonitorAsync();
            var timer = Stopwatch.StartNew();
            int read = 0;
            await foreach (var result in parser.ReadFilesAsync(Enumerable.Repeat(path, count)))
            {
                if (!result.IsSuccess) throw result.Erro!;
                read++;
                if (delay > 0 && read % delayEvery == 0) await Task.Delay(delay);
            }
            timer.Stop();
            await monitorStop.CancelAsync(); await monitor;
            long retained = GC.GetTotalMemory(true);
            Console.WriteLine(JsonSerializer.Serialize(new { count, read, parallelism, capacity, delay, delayEvery,
                seconds = timer.Elapsed.TotalSeconds, notesPerSecond = read / timer.Elapsed.TotalSeconds,
                heapBefore = before, heapAfterCollection = retained, heapPeak, workingPeak,
                allocatedBytes = GC.GetTotalAllocatedBytes(true) - allocatedBefore,
                gen0Collections = GC.CollectionCount(0) - gen0, gen1Collections = GC.CollectionCount(1) - gen1, gen2Collections = GC.CollectionCount(2) - gen2,
                sourceBytes = new FileInfo(path).Length,
                source = "Repeated lazy path to a one-item XML; new FileStream and object graph on every iteration." }));
        }
        finally { File.Delete(path); }
    }
}
