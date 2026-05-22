using BenchmarkDotNet.Attributes;
using MulticastFunc;
using System;

namespace MulticastFuncBenchmark
{
    /// <summary>
    /// Measures the cost of adding one delegate to a list of delegates.
    /// </summary>
    [MemoryDiagnoser]
    [HideColumns("Error", "StdDev", "Median", "RatioSD", "Gen0")]
    public class SubscribeBenchmark
    {
        Func<int>? funcBase;
        MulticastFunc<int>? multicastBase;
        static int Method() => 1;

        [Params(5, 25, 125)]
        public int DelegateCount = 5;

        [GlobalSetup]
        public void Setup()
        {
            funcBase = null;
            multicastBase = null;
            for (int i = 0; i < DelegateCount; i++)
            {
                funcBase += Method;
                multicastBase += Method;
            }
        }

        [Benchmark(Baseline = true)]
        public Func<int>? Subscribe_Func()
        {
            return funcBase + Method;
        }

        [Benchmark]
        public MulticastFunc<int>? Subscribe_MulticastFunc()
        {
            return multicastBase + Method;
        }
    }

    /// <summary>
    /// Measures the cost of removing a delegate from a list of delegates.
    /// </summary>
    [MemoryDiagnoser]
    [HideColumns("Error", "StdDev", "Median", "RatioSD", "Gen0")]
    public class UnsubscribeBenchmark
    {
        Func<int>? funcBase;
        MulticastFunc<int>? multicastBase;
        static int Method() => 1;

        [Params(5, 25, 125)]
        public int DelegateCount = 5;

        [GlobalSetup]
        public void Setup()
        {
            funcBase = null;
            multicastBase = null;
            for (int i = 0; i < DelegateCount; i++)
            {
                funcBase += Method;
                multicastBase += Method;
            }
        }

        [Benchmark(Baseline = true)]
        public Func<int>? Unsubscribe_Func()
        {
            return funcBase - Method;
        }

        [Benchmark]
        public MulticastFunc<int>? Unsubscribe_MulticastFunc()
        {
            return multicastBase - Method;
        }
    }
}
