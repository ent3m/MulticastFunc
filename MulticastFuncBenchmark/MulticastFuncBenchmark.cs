using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using MulticastFunc;
using System.Buffers;

namespace MulticastFuncBenchmark;

[MemoryDiagnoser]
public class MulticastFuncBenchmark
{
    Func<int>? funcDelegate;
    MulticastFunc<int>? multicastFunc;
    readonly ArrayBufferWriter<int> bufferWriter = new(InvocationCount);
    static int Method() => 42;
    readonly Exception InvalidResultsException = new Exception("Invalid results produced.");

    public const int InvocationCount = 20;

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
    public int[] InvokeFuncLinq()
    {
        var results = funcDelegate!.GetInvocationList().Cast<Func<int>>().Select(x => x.Invoke()).ToArray();
        if (results.Length != InvocationCount)
            throw InvalidResultsException;
        return results;
    }

    [Benchmark(Baseline = true)]
    public int[] InvokeMulticastFunc()
    {
        var results = multicastFunc!.Invoke();
        if (results.Length != InvocationCount)
            throw InvalidResultsException;
        return results;
    }

    [Benchmark]
    public ReadOnlySpan<int> InvokeMulticastFuncWithBuffer()
    {
        var results = multicastFunc!.Invoke(bufferWriter.GetSpan(multicastFunc!.Count));
        if (results.Length != InvocationCount)
            throw InvalidResultsException;
        return results;
    }
}