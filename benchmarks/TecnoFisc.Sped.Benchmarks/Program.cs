using BenchmarkDotNet.Running;

using TecnoFisc.Sped.Benchmarks;

if (args.Length >= 2 && args[0] == "--probe" && args[1] == "peak")
{
    await PeakHeapProbe.ExecutarAsync();
    return;
}

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
