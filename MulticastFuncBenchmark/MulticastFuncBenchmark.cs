using BenchmarkDotNet.Attributes;
using MulticastFunc;
using System.Buffers;

namespace MulticastFuncBenchmark;

[MemoryDiagnoser]
public class MulticastFuncBenchmark
{
    Func<int>? funcDelegate;
    MulticastFunc<int>? multicastFunc;
    readonly ArrayBufferWriter<int> bufferWriter = new(10);
    static int Method() => 42;

    public int InvocationCount = 10;

    [GlobalSetup]
    public void Setup()
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
    public int[] InvokeFuncDelegate()
    {
        return funcDelegate!.GetInvocationList().Cast<Func<int>>().Select(x => x.Invoke()).ToArray();
    }

    [Benchmark(Baseline = true)]
    public int[] InvokeMulticastFunc()
    {
        return multicastFunc!.Invoke();
    }

    [Benchmark]
    public ReadOnlySpan<int> InvokeMulticastFuncWithBuffer()
    {
        return multicastFunc!.Invoke(bufferWriter.GetSpan(multicastFunc!.Count));
    }
}