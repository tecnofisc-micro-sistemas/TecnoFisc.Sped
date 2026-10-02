using System.IO.Compression;
using System.Text;
using BenchmarkDotNet.Attributes;
using TecnoFisc.Sped.NFeNFCe;

namespace TecnoFisc.Sped.Benchmarks;

[MemoryDiagnoser]
public class ParserNFeImportBenchmark
{
    private string _directory = null!;
    private byte[] _zip = [];
    private ParserNFe _parser = null!;
    [Params(1, 50, 500)] public int QtdItens { get; set; }
    [Params(1, 4)] public int Parallelism { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _directory = Path.Combine(Path.GetTempPath(), "nfe-benchmark-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        byte[] xml = Encoding.UTF8.GetBytes(ParserNFeBenchmark.GerarCanonico(QtdItens));
        using var memory = new MemoryStream();
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, true))
            for (int i = 0; i < 100; i++)
            {
                string name = $"{i}.xml";
                File.WriteAllBytes(Path.Combine(_directory, name), xml);
                using var entry = zip.CreateEntry(name).Open();
                entry.Write(xml);
            }
        _zip = memory.ToArray();
        _parser = new(new() { Parallelism = Parallelism });
    }
    [Benchmark(OperationsPerInvoke = 100)]
    public async Task<int> DirectoryImport()
    {
        int count = 0;
        await foreach (var r in _parser.ReadDirectoryAsync(_directory)) if (r.IsSuccess) count++;
        return count;
    }
    [Benchmark(OperationsPerInvoke = 100)]
    public async Task<int> ZipImport()
    {
        using var stream = new MemoryStream(_zip, false);
        int count = 0;
        await foreach (var r in _parser.ReadZipAsync(stream, "benchmark.zip")) if (r.IsSuccess) count++;
        return count;
    }
    [GlobalCleanup]
    public void Cleanup() => Directory.Delete(_directory, true);
}
