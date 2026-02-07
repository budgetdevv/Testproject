using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using System.Diagnostics;

var config = DefaultConfig.Instance;

if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
{
    config = config.AddDiagnoser(new DisassemblyDiagnoser(new()));
}

BenchmarkRunner.Run<StopwatchBenchmark>(config);

public class StopwatchBenchmark
{
    public static readonly long FrequencyPerMillisecond = Stopwatch.Frequency / TimeSpan.MillisecondsPerSecond;

    [Benchmark]
    [Arguments(1_000_000L)]
    [Arguments(10_000_000L)]
    public int Calc(long delay) => (int)(delay * TimeSpan.MillisecondsPerSecond / Stopwatch.Frequency);

    [Benchmark]
    [Arguments(1_000_000L)]
    [Arguments(10_000_000L)]
    public int Calc2(long delay) => (int)(delay / FrequencyPerMillisecond);
}