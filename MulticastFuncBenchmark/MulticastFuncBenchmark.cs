using BenchmarkDotNet.Attributes;
using MulticastFunc;
using System.Buffers;

namespace MulticastFuncBenchmark;

[MemoryDiagnoser]
public class MulticastFuncBenchmark
{
    Func<int>? funcDelegate;
    MulticastFunc<int>? multicastFunc;
    readonly ArrayBufferWriter<int> bufferWriter = new ArrayBufferWriter<int>();
    static int Method() => 1;

    [Params(5, 25, 125)]
    public int InvocationCount = 5;

    [GlobalSetup]
    public void BenchmarkSetup()
    {
        funcDelegate = BuildFuncDelegate();
        multicastFunc = BuildMulticastFunc();
    }

    public Func<int>? BuildFuncDelegate()
    {
        Func<int>? func = default;
        for (int i = 0; i < InvocationCount; i++)
        {
            func += Method;
        }
        return func;
    }

    public MulticastFunc<int>? BuildMulticastFunc()
    {
        MulticastFunc<int>? func = default;
        for (int i = 0; i < InvocationCount; i++)
        {
            func += Method;
        }
        return func;
    }

    [Benchmark]
    public int[] InvokeFuncLinq()
    {
        var results = funcDelegate!.GetInvocationList().Cast<Func<int>>().Select(x => x.Invoke()).ToArray();
        return results;
    }

    [Benchmark]
    public int InvokeFunc()
    {
        return funcDelegate!.Invoke();
    }

    [Benchmark(Baseline = true)]
    public int[] InvokeMulticastFunc()
    {
        var results = multicastFunc!.Invoke();
        return results;
    }

    [Benchmark]
    public ReadOnlySpan<int> InvokeMulticastFuncWithBuffer()
    {
        var results = multicastFunc!.Invoke(bufferWriter.GetSpan(multicastFunc!.Count));
        return results;
    }
}