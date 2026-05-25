using BenchmarkDotNet.Running;
using MulticastFunc;
using System;

namespace MulticastFuncBenchmark
{
    public class Program
    {
        static void Main()
        {
            //BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAll();
            BenchmarkRunner.Run<InvokeBenchmark>();
        }
    }
}