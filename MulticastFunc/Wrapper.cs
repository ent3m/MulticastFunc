using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MulticastFunc;

internal struct Wrapper<T>(T value) : IEqualityComparer<Wrapper<T>>
{
    public T Value = value;

    public bool Equals(Wrapper<T> x, Wrapper<T> y)
        => Value!.Equals(y);

    public int GetHashCode([DisallowNull] Wrapper<T> obj)
    {
        throw new NotImplementedException();
    }
}