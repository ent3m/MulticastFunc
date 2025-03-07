using BenchmarkDotNet.Attributes;
using MulticastFunc;

namespace MulticastFuncBenchmark;

[MemoryDiagnoser]
public class MulticastFuncBenchmark
{
    Func<int>? funcDelegate;
    MulticastFunc<int>? multicastFunc;
    static int Method() => 42;

    [Params(5, 50)]
    public int count = 5;

    [GlobalSetup]
    public void Setup()
    {
        funcDelegate = BuildFuncDelegate();
        multicastFunc = BuildMulticastFunc();
    }

    public Func<int>? BuildFuncDelegate()
    {
        Func<int>? func = default;
        for (int i = 0; i < count; i++)
        {
            func += Method;
        }
        return func;
    }

    public MulticastFunc<int>? BuildMulticastFunc()
    {
        MulticastFunc<int>? func = default;
        for (int i = 0; i < count; i++)
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
}