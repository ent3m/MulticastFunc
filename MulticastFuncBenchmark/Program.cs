using BenchmarkDotNet.Running;

namespace MulticastFuncBenchmark
{
    public class Program
    {
        static void Main()
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAll();
        }
    }
}