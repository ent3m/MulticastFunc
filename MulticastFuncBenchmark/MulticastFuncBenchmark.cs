using BenchmarkDotNet.Attributes;
using MulticastFunc;
using System;
using System.Linq;

namespace MulticastFuncBenchmark
{
    [MemoryDiagnoser]
    [HideColumns("Error", "StdDev", "Median", "RatioSD", "Gen0")]
    public class MulticastFuncBenchmark
    {
        Func<int>? funcDelegate;
        MulticastFunc<int>? multicastFunc;
        int[]? spanBuffer;
        static int Method() => 1;

        [Params(5, 25, 125)]
        public int DelegateCount = 5;

        [GlobalSetup]
        public void BenchmarkSetup()
        {
            funcDelegate = BuildFunc();
            multicastFunc = BuildMulticastFunc();
            spanBuffer = new int[DelegateCount];
        }

        private Func<int>? BuildFunc()
        {
            Func<int>? func = default;
            for (int i = 0; i < DelegateCount; i++)
            {
                func += Method;
            }
            return func;
        }

        private MulticastFunc<int>? BuildMulticastFunc()
        {
            MulticastFunc<int>? func = default;
            for (int i = 0; i < DelegateCount; i++)
            {
                func += Method;
            }
            return func;
        }

        [Benchmark]
        public int[] Invoke_MulticastFunc()
        {
            return multicastFunc!.Invoke();
        }

        [Benchmark]
        public ReadOnlySpan<int> Invoke_MulticastFunc_SpanBuffer()
        {
            return multicastFunc!.Invoke(spanBuffer);
        }

        /// <summary>
        /// Baseline: invokes all delegates but discards all results except the last.
        /// Included to show the cost of the raw dispatch loop with no result collection.
        /// </summary>
        [Benchmark(Baseline = true)]
        public int Invoke_Func_LastResultOnly()
        {
            return funcDelegate!.Invoke();
        }

        [Benchmark]
        public int[] Invoke_Func_Linq()
        {
            return funcDelegate!.GetInvocationList().Cast<Func<int>>().Select(x => x.Invoke()).ToArray();
        }
    }
}
