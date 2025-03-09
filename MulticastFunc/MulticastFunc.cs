using System;
using System.Collections.Generic;

namespace MulticastFunc
{
    public class MulticastFunc<TResult>
    {
        readonly List<Func<TResult>> funcs = new List<Func<TResult>>();

        public int Count => funcs.Count;

        public static MulticastFunc<TResult> operator +(MulticastFunc<TResult>? a, Func<TResult> b)
        {
            a ??= new MulticastFunc<TResult>();
            a.Add(b);
            return a;
        }

        public static MulticastFunc<TResult>? operator -(MulticastFunc<TResult>? a, Func<TResult> b)
        {
            a?.Remove(b);
            return a;
        }

        public static implicit operator MulticastFunc<TResult>(Func<TResult> f) => new MulticastFunc<TResult>(f);

        public static explicit operator Func<TResult>?(MulticastFunc<TResult>? m)
        {
            Func<TResult>? f = default;
            if (m != null)
            {
                foreach (var func in m.funcs)
                {
                    f += func;
                }
            }
            return f;
        }

        public MulticastFunc(Func<TResult> func) => Add(func);
        private MulticastFunc() { }

        public TResult[] Invoke()
        {
            var results = new TResult[Count];
            Invoke(results);
            return results;
        }

        public ReadOnlySpan<TResult> Invoke(Span<TResult> buffer)
        {
            var count = funcs.Count;
            if (buffer.Length < count)
                throw new ArgumentException("Buffer is too small", nameof(buffer));
            for (int i = 0; i < count; i++)
            {
                buffer[i] = funcs[i]();
            }
            return buffer[..count];
        }

        private void Add(Func<TResult> func)
        {
            if (func == null) return;
            var functions = func.GetInvocationList();
            foreach (var function in functions)
            {
                funcs.Add((Func<TResult>)function);
            }
        }

        private void Remove(Func<TResult> func)
        {
            if (func == null) return;
            var removals = func.GetInvocationList();
            int removed = funcs.RemoveAll(x => Array.IndexOf(removals, x) != -1);
        }
    }
}
