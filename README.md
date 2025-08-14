# Description
MulticastFunc is designed to be an alternative to Func. It provides a simple and efficient way to to retrieve the return values of all invocations instead of only the final invocation.

# The Problem with Func
To retrieve the results of all invocations in a MulticastDelegate, one must get a list of invocations, cast and invoke each delegate individually, and store the results in an array.
The code looks like this with LINQ:
```csharp
T[]? results = myDelegate?.GetInvocationList().Cast<Func<T>>().Select(f => f.Invoke()).ToArray();
```
You can create an extension method to avoid typing all of that everytime. However, the allocation cost of `GetInvocationList` *cannot* be avoided. Invoking a `Func<T>` or other MulticastDelegates this way is slow and generates a lot of garbage over time.</br></br>

`MulticastFunc` solves this problem by making `Invoke` return an array of results while keeping the invocation process [fast](#Benchmarks).

# Usage
`MulticastFunc` behaves similarly to a `Func`. The usage is largely the same.</br>

Adding and removing invocations:
```csharp
multicastFunc += MyMethod;
multicastFunc += () => "Hello World";
multicastFunc -= MyMethod;
multicastFunc -= MyFunc;
```

Invoking:
```csharp
T[]? results = multicastFunc?.Invoke();
```

As a backing field for events:
```csharp
public event Func<string> EventHappened
{
  add => multicastFunc += value;
  remove => multicastFunc -= value;
}
private MulticastFunc<string>? multicastFunc = default;
```

You can also store the results in a buffer:
```csharp
Task[] buffer = ArrayPool<Task>.Shared.Rent(multicastFunc.Count);
var tasks = multicastFunc.Invoke(buffer);
await Task.WhenAll(tasks).ContinueWith(_ => ArrayPool<Task>.Shared.Return(buffer));
```

# Installation
Install with [nuget](https://www.nuget.org/packages/MulticastFunc/) or download and copy the project into your solution.

# Technical Details
MulticastFunc maintains a `Delegate[]` that is updated whenever a function is added or removed. Invoking consists of iterating over the delegate, invoking each function, and storing the results, bypassing `GetInvocationList`. This makes the invocation process fast and garbage-free, especially if the overload that accepts a buffer is used.</br></br>
It overloads the `+` and `-` operators to imitate the add/remove behavior of a delegate. Similar to a delegate, it is immutable, meaning invoking is thread-safe. The trade off is that adding generates more garbage and removing is slower with a MulticastFunc than with a delegate.</br></br>
Another limitation is that, because the compiler does not recognize MulticastFunc as a delegate type, you cannot assign a method group to it directly. This means that `multicastFunc = MyMethod` is not allowed, but `multicastFunc += MyMethod` is allowed.

# Benchmarks
The performance of `MulticastFunc.Invoke()` is roughly equals to that of `Func.Invoke()` and is *~6 times* faster than invoking with LINQ.
| Method                        | InvocationCount | Mean        | StdDev    | Median      | Ratio | Allocated | Alloc Ratio |
|------------------------------ |---------------- |------------:|----------:|------------:|------:|----------:|------------:|
| InvokeFuncLinq                | 25              |   339.65 ns |  6.315 ns |   339.09 ns |  6.96 |     792 B |        6.19 |
| InvokeFunc                    | 25              |    56.53 ns |  0.083 ns |    56.48 ns |  1.16 |         - |        0.00 |
| InvokeMulticastFunc           | 25              |    48.86 ns |  2.089 ns |    47.39 ns |  1.00 |     128 B |        1.00 |
| InvokeMulticastFuncWithBuffer | 25              |    42.01 ns |  2.039 ns |    41.57 ns |  0.86 |         - |        0.00 |
