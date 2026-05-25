using BenchmarkDotNet.Attributes;
using MulticastFunc;
using System;
using System.Linq;

/*
.NET 9 benchmark results
| Method                          | DelegateCount | Mean       | Ratio | Allocated | Alloc Ratio |
|-------------------------------- |-------------- |-----------:|------:|----------:|------------:|
| Invoke_MulticastFunc            | 5             |  12.230 ns |  1.48 |      48 B |          NA |
| Invoke_MulticastFunc_SpanBuffer | 5             |   8.645 ns |  1.05 |         - |          NA |
| Invoke_Func_LastResultOnly      | 5             |   8.247 ns |  1.00 |         - |          NA |
| Invoke_Func_                    | 5             |  25.434 ns |  3.08 |      48 B |          NA |
| Invoke_Func_Linq                | 5             |  67.662 ns |  8.20 |     248 B |          NA |
|                                 |               |            |       |           |             |
| Invoke_MulticastFunc            | 25            |  45.972 ns |  1.17 |     128 B |          NA |
| Invoke_MulticastFunc_SpanBuffer | 25            |  39.152 ns |  0.99 |         - |          NA |
| Invoke_Func_LastResultOnly      | 25            |  39.368 ns |  1.00 |         - |          NA |
| Invoke_Func_                    | 25            | 101.417 ns |  2.58 |     128 B |          NA |
| Invoke_Func_Linq                | 25            | 213.482 ns |  5.42 |     488 B |          NA |
|                                 |               |            |       |           |             |
| Invoke_MulticastFunc            | 125           | 207.032 ns |  1.22 |     528 B |          NA |
| Invoke_MulticastFunc_SpanBuffer | 125           | 191.725 ns |  1.13 |         - |          NA |
| Invoke_Func_LastResultOnly      | 125           | 170.237 ns |  1.00 |         - |          NA |
| Invoke_Func_                    | 125           | 463.054 ns |  2.72 |     528 B |          NA |
| Invoke_Func_Linq                | 125           | 845.783 ns |  4.97 |    1688 B |          NA |
*/

namespace MulticastFuncBenchmark
{
    [MemoryDiagnoser]
    [HideColumns("Error", "StdDev", "Median", "RatioSD", "Gen0")]
    public class InvokeBenchmark
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
        public int[] Invoke_Func_Enumerate()
        {
            int i = 0;
            foreach (var _ in Delegate.EnumerateInvocationList(funcDelegate))
            {
                i++;
            }
            var results = new int[i];
            i = 0;
            foreach (var d in Delegate.EnumerateInvocationList(funcDelegate))
            {
                results[i++] = d();
            }
            return results;
        }

        [Benchmark]
        public int[] Invoke_Func_Linq()
        {
            return funcDelegate!.GetInvocationList().Cast<Func<int>>().Select(x => x.Invoke()).ToArray();
        }
    }
}
