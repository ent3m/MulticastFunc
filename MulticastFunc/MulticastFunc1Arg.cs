using System;
using System.Collections.Generic;

namespace MulticastFunc
{
    public class MulticastFunc<T, TResult>
    {
        readonly List<Func<T, TResult>> funcs = new List<Func<T, TResult>>();

        public int Count => funcs.Count;

        public static MulticastFunc<T, TResult> operator +(MulticastFunc<T, TResult>? a, Func<T, TResult> b)
        {
            a ??= new MulticastFunc<T, TResult>();
            a.Add(b);
            return a;
        }

        public static MulticastFunc<T, TResult>? operator -(MulticastFunc<T, TResult>? a, Func<T, TResult> b)
        {
            a?.Remove(b);
            return a;
        }

        public static implicit operator MulticastFunc<T, TResult>(Func<T, TResult> f) => new MulticastFunc<T, TResult>(f);

        public static explicit operator Func<T, TResult>?(MulticastFunc<T, TResult>? m)
        {
            Func<T, TResult>? f = default;
            if (m != null)
            {
                foreach (var func in m.funcs)
                {
                    f += func;
                }
            }
            return f;
        }

        public MulticastFunc(Func<T, TResult> func) => Add(func);
        private MulticastFunc() { }

        public TResult[] Invoke(T arg)
        {
            var results = new TResult[Count];
            Invoke(arg, results);
            return results;
        }

        public ReadOnlySpan<TResult> Invoke(T arg, Span<TResult> buffer)
        {
            var count = funcs.Count;
            if (buffer.Length < count)
                throw new ArgumentException("Buffer is too small", nameof(buffer));
            for (int i = 0; i < count; i++)
            {
                buffer[i] = funcs[i](arg);
            }
            return buffer[..count];
        }

        private void Add(Func<T, TResult> func)
        {
            if (func == null) return;
            var functions = func.GetInvocationList();
            foreach (var function in functions)
            {
                funcs.Add((Func<T, TResult>)function);
            }
        }

        private void Remove(Func<T, TResult> func)
        {
            if (func == null) return;
            var removals = func.GetInvocationList();
            int removed = funcs.RemoveAll(x => Array.IndexOf(removals, x) != -1);
        }
    }
}
