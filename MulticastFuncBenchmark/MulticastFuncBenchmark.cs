using BenchmarkDotNet.Attributes;
using MulticastFunc;
using System.Buffers;

namespace MulticastFuncBenchmark;

[MemoryDiagnoser]
[HideColumns("Error", "StdDev", "Median", "RatioSD", "Gen0")]
public class MulticastFuncBenchmark
{
    Func<int>? funcDelegate;
    MulticastFunc<int>? multicastFunc;
    readonly ArrayBufferWriter<int> bufferWriter = new ArrayBufferWriter<int>();
    static int Method() => 1;

    [Params(5, 25, 125)]
    public int DelegateCount = 5;

    [GlobalSetup]
    public void BenchmarkSetup()
    {
        funcDelegate = BuildFunc();
        multicastFunc = BuildMulticastFunc();
    }

    [GlobalCleanup]
    public void BenchmarkCleanup()
    {
        bufferWriter.Clear();
    }

    public Func<int>? BuildFunc()
    {
        Func<int>? func = default;
        for (int i = 0; i < DelegateCount; i++)
        {
            func += Method;
        }
        return func;
    }

    public MulticastFunc<int>? BuildMulticastFunc()
    {
        MulticastFunc<int>? func = default;
        for (int i = 0; i < DelegateCount; i++)
        {
            func += Method;
        }
        return func;
    }

    [Benchmark]
    public int[] Invoke_Func_Linq()
    {
        var results = funcDelegate!.GetInvocationList().Cast<Func<int>>().Select(x => x.Invoke()).ToArray();
        return results;
    }

    [Benchmark]
    public int Invoke_Func()
    {
        return funcDelegate!.Invoke();
    }

    [Benchmark (Baseline = true)]
    public int[] Invoke_MulticastFunc()
    {
        var results = multicastFunc!.Invoke();
        return results;
    }

    [Benchmark]
    public ReadOnlySpan<int> Invoke_MulticastFunc_SpanBuffer()
    {
        var results = multicastFunc!.Invoke(bufferWriter.GetSpan(multicastFunc!.Count));
        return results;
    }
}