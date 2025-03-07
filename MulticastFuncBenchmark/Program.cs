using BenchmarkDotNet.Running;

namespace MulticastFuncBenchmark;

public class Program
{
    static void Main()
    {
        _ = BenchmarkRunner.Run<MulticastFuncBenchmark>();
    }
}