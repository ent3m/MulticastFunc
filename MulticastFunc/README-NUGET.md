MulticastFunc is a high-performance, drop-in alternative to Func that retrieves the return values of all invocations instead of only the final one.

## Features
- **Familiar Syntax**: Implements standard add/remove (`+=` / `-=`) operator behavior.
- **Thread-Safe**: Immutable design guarantees safe concurrent execution.
- **Allocation-Free Overloads**: Supports writing return values to a `Span` buffer.
- **Fault Tolerance**: Optional execution path runs all delegates regardless of individual errors, throwing an aggregated exception at the end.
- **Native AOT**: Native AOT compilation and trim compatible.

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