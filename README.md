# MulticastFunc

[![NuGet Version](https://img.shields.io/nuget/v/MulticastFunc?logo=nuget&label=NuGet&color=004880)](https://www.nuget.org/packages/MulticastFunc)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MulticastFunc?logo=nuget&label=Downloads)](https://www.nuget.org/packages/MulticastFunc)
[![.NET](https://img.shields.io/badge/.NET-Standard_2.1-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)
[![Build Status](https://github.com/ent3m/MulticastFunc/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/ent3m/MulticastFunc/actions/workflows/ci.yml)

MulticastFunc is a high-performance, drop-in alternative to Func that retrieves the return values of all invocations instead of only the final one.

## The Problem
To collect results from all targets in a standard .NET `MulticastDelegate`, you have to extract the invocation list manually:
```csharp
T[]? results = myDelegate?.GetInvocationList().Cast<Func<T>>().Select(f => f.Invoke()).ToArray();
```
Even when hidden behind an extension method, the allocation cost of `GetInvocationList()` cannot be avoided. This approach is slow and generates unnecessary garbage.

**The Solution**: `MulticastFunc` solves this by returning an array of all results directly from `Invoke()` while matching native delegate performance. See [Benchmarks](#benchmarks).

## Features
- **Familiar Syntax**: Implements standard add/remove (`+=` / `-=`) operator behavior.
- **Thread-Safe**: Immutable design guarantees safe concurrent execution.
- **Allocation-Free Overloads**: Supports writing return values to a `Span` buffer.
- **Fault Tolerance**: Optional execution path runs all delegates regardless of individual errors, throwing an aggregated exception at the end.
- **Native AOT**: Native AOT compilation and trim compatible.

## Installation
[![NuGet](https://img.shields.io/nuget/v/MulticastFunc?logo=nuget&label=NuGet&color=004880)](https://www.nuget.org/packages/MulticastFunc/ "Download MulticastFunc from NuGet.org")
```
dotnet add package MulticastFunc
```
**Requirements**: .NET Standard 2.1 or newer

## Usage Examples

#### Basic Mechanics
```csharp
// Subscription
multicastFunc += MyMethod;
multicastFunc += () => "Hello World";

// Standard Invocation
string[]? results = multicastFunc?.Invoke();
```

#### Event Backing Field
```csharp
public event Func<string> EventHappened
{
    add => multicastFunc += value;
    remove => multicastFunc -= value;
}
private MulticastFunc<string>? multicastFunc = default;
```

#### Advanced Execution Modes
```csharp
// Fault-Tolerant: Invokes everything. Failing targets return default(T). Throws AggregateException.
string[]? results = multicastFunc?.InvokeAll();

// Allocation-Free: Write to a Span buffer for hot paths.
var buffer = ArrayPool<string>.Shared.Rent(multicastFunc.Count);
ReadOnlySpan<string> results = multicastFunc.Invoke(buffer);
```

## Technical Limitations
- **Subscription Overhead**: `MulticastFunc` is immutable to match the behavior of standard delegates. As a result, subscribing (`+`) generates more garbage than native delegates.
- **No Direct Method Group Assignments**: The compiler does not recognize `MulticastFunc` as a native delegate type. As a result, direct assignment from a method group `multicastFunc = MyMethod` is invalid; you must use compounding assignment `multicastFunc += MyMethod`.

## Benchmarks (.NET 8)
`MulticastFunc.Invoke()` performs on par with a raw `Func.Invoke()` and runs *~6 times* faster than equivalent LINQ invocations.

#### Invocation Performance
| Method                          | DelegateCount | Mean         | Ratio | Allocated |
|-------------------------------- |-------------- |-------------:|------:|----------:|
| Invoke_MulticastFunc            | 5             |    13.783 ns |  1.13 |      48 B |
| Invoke_MulticastFunc_SpanBuffer | 5             |     8.630 ns |  0.71 |         - |
| Invoke_Func_LastResultOnly      | 5             |    12.171 ns |  1.00 |         - |
| Invoke_Func_Linq                | 5             |   103.306 ns |  8.49 |     352 B |
|                                 |               |              |       |           |
| Invoke_MulticastFunc            | 25            |    50.962 ns |  0.91 |     128 B |
| Invoke_MulticastFunc_SpanBuffer | 25            |    38.537 ns |  0.69 |         - |
| Invoke_Func_LastResultOnly      | 25            |    56.183 ns |  1.00 |         - |
| Invoke_Func_Linq                | 25            |   336.678 ns |  5.99 |     792 B |
|                                 |               |              |       |           |
| Invoke_MulticastFunc            | 125           |   230.311 ns |  0.83 |     528 B |
| Invoke_MulticastFunc_SpanBuffer | 125           |   187.109 ns |  0.68 |         - |
| Invoke_Func_LastResultOnly      | 125           |   276.251 ns |  1.00 |         - |
| Invoke_Func_Linq                | 125           | 1,394.755 ns |  5.05 |    2424 B |

#### Subscription Performance (+=)
| Method                  | DelegateCount | Mean     | Ratio | Gen1   | Allocated | Alloc Ratio |
|------------------------ |-------------- |---------:|------:|-------:|----------:|------------:|
| Subscribe_Func          | 5             | 20.14 ns |  1.00 |      - |      64 B |        1.00 |
| Subscribe_MulticastFunc | 5             | 17.51 ns |  0.87 |      - |     128 B |        2.00 |
|                         |               |          |       |        |           |             |
| Subscribe_Func          | 25            | 20.29 ns |  1.00 |      - |      64 B |        1.00 |
| Subscribe_MulticastFunc | 25            | 21.58 ns |  1.06 |      - |     288 B |        4.50 |
|                         |               |          |       |        |           |             |
| Subscribe_Func          | 125           | 20.52 ns |  1.00 |      - |      64 B |        1.00 |
| Subscribe_MulticastFunc | 125           | 39.93 ns |  1.95 | 0.0002 |    1088 B |       17.00 |


#### Unsubscription Performance (-=)
| Method                    | DelegateCount | Mean      | Ratio | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |-------------- |----------:|------:|-------:|----------:|------------:|
| Unsubscribe_Func          | 5             |  24.98 ns |  1.00 |      - |     120 B |        1.00 |
| Unsubscribe_MulticastFunc | 5             |  18.09 ns |  0.72 |      - |     112 B |        0.93 |
|                           |               |           |       |        |           |             |
| Unsubscribe_Func          | 25            |  46.11 ns |  1.00 |      - |     344 B |        1.00 |
| Unsubscribe_MulticastFunc | 25            |  23.02 ns |  0.50 |      - |     272 B |        0.79 |
|                           |               |           |       |        |           |             |
| Unsubscribe_Func          | 125           | 148.88 ns |  1.00 | 0.0002 |    1112 B |        1.00 |
| Unsubscribe_MulticastFunc | 125           |  39.29 ns |  0.26 | 0.0002 |    1072 B |        0.96 |
