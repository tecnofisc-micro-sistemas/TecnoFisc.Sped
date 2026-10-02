using System.Reflection;
using System.Text;
using System.Xml;
using BenchmarkDotNet.Attributes;
using TecnoFisc.Sped.NFeNFCe;

namespace TecnoFisc.Sped.Benchmarks;

/// <summary>Diagnóstico do custo da proteção do leitor, usando o mesmo modelo completo em ambos os casos.</summary>
[MemoryDiagnoser]
public class NFeReaderOverheadBenchmark
{
    private byte[] _xml = [];
    private static readonly Type ReaderType = typeof(ParserNFe).Assembly.GetType("TecnoFisc.Sped.NFeNFCe.Parser.NFeXmlReader")!;
    private static readonly Type GuardType = typeof(ParserNFe).Assembly.GetType("TecnoFisc.Sped.NFeNFCe.IO.GuardedXmlReader")!;
    private static readonly MethodInfo ReadMethod = ReaderType.GetMethod("ReadAsync")!;
    private static readonly ParserNFeOptions Options = new();
    [GlobalSetup] public void Setup() => _xml = Encoding.UTF8.GetBytes(ParserNFeBenchmark.GerarCanonico(500));
    [Benchmark(Baseline = true)] public Task<NFe> Plain() => Parse(false);
    [Benchmark] public Task<NFe> Guarded() => Parse(true);
    private async Task<NFe> Parse(bool guarded)
    {
        using var stream = new MemoryStream(_xml, false);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true, IgnoreWhitespace = true, IgnoreComments = true, DtdProcessing = DtdProcessing.Prohibit });
        using var wrapper = guarded ? (XmlReader)Activator.CreateInstance(GuardType, reader, Options, CancellationToken.None)! : null;
        object instance = Activator.CreateInstance(ReaderType, wrapper ?? reader, Options)!;
        return await (Task<NFe>)ReadMethod.Invoke(instance, [CancellationToken.None, false])!;
    }
}
