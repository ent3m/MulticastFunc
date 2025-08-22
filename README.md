# Description
MulticastFunc is designed to be an alternative to Func. It provides a simple and efficient way to to retrieve the return values of all invocations instead of only the final invocation.

# The Problem with Func
To retrieve the results of all invocations in a MulticastDelegate, one must get a list of invocations, cast and invoke each delegate individually, and store the results in an array.
The code looks like this with LINQ:
```csharp
T[]? results = myDelegate?.GetInvocationList().Cast<Func<T>>().Select(f => f.Invoke()).ToArray();
```
You can create an extension method to avoid typing that everytime. However, the allocation cost of `GetInvocationList` *cannot* be avoided. Invoking a `Func<T>` or other MulticastDelegates this way is slow and generates a lot of garbage over time.</br></br>

`MulticastFunc` solves this problem by having `Invoke` return an array of results without compromising on [performance](#Benchmarks). In addition, it behaves similarly to a delegate with its immutability and add/remove syntax, making it a suitable replacement for any `Func` delegate.

# Usage
Adding and removing:
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

Use as a backing field for events:
```csharp
public event Func<string> EventHappened
{
  add => multicastFunc += value;
  remove => multicastFunc -= value;
}
private MulticastFunc<string>? multicastFunc = default;
```

Get delegate count:
```csharp
int count = multicastFunc.Count;
```

Store the results in a buffer:
```csharp
Task[] buffer = ArrayPool<Task>.Shared.Rent(multicastFunc.Count);

int written = multicastFunc.Invoke(buffer);
// or
ReadOnlySpan<Task> tasks = multicastFunc.Invoke(buffer.AsSpan());
```

# Installation
Install with [nuget](https://www.nuget.org/packages/MulticastFunc/) or download and copy the project into your solution.

# Technical Details
MulticastFunc maintains a `Delegate[]` that is updated whenever a function is added or removed. Invoking consists of iterating over the delegate, invoking each function, and storing the results, bypassing `GetInvocationList`. This makes the invocation process fast and garbage-free, especially if the overload that accepts a buffer is used.</br></br>
It overloads the `+` and `-` operators to imitate the add/remove behavior of a delegate. Similar to a delegate, it is immutable, meaning invoking is thread-safe. The trade off is that adding and removing is slower with a MulticastFunc than with a delegate.</br></br>
Another limitation is that, because the compiler does not recognize MulticastFunc as a delegate type, you cannot assign a method group to it directly. This means that `multicastFunc = MyMethod` is not allowed, but `multicastFunc += MyMethod` is allowed.

# Benchmarks
The performance of `MulticastFunc.Invoke()` is roughly equals to that of `Func.Invoke()` and is *~6 times* faster than invoking with LINQ.
| Method                          | DelegateCount | Mean         | Ratio | Allocated | Alloc Ratio |
|-------------------------------- |-------------- |-------------:|------:|----------:|------------:|
| Invoke_Func_Linq                | 25            |   332.711 ns |  6.17 |     792 B |        6.19 |
| Invoke_Func                     | 25            |    55.636 ns |  1.03 |         - |        0.00 |
| Invoke_MulticastFunc            | 25            |    53.958 ns |  1.00 |     128 B |        1.00 |
| Invoke_MulticastFunc_SpanBuffer | 25            |    39.281 ns |  0.73 |         - |        0.00 |
