using System;
using System.Collections.Generic;

namespace MulticastFunc
{
    public class MulticastFunc<T1, T2, T3, TResult>
    {
        readonly List<Func<T1, T2, T3, TResult>> funcs = new List<Func<T1, T2, T3, TResult>>();

        public int Count => funcs.Count;

        public static MulticastFunc<T1, T2, T3, TResult> operator +(MulticastFunc<T1, T2, T3, TResult>? a, Func<T1, T2, T3, TResult> b)
        {
            a ??= new MulticastFunc<T1, T2, T3, TResult>();
            a.Add(b);
            return a;
        }

        public static MulticastFunc<T1, T2, T3, TResult>? operator -(MulticastFunc<T1, T2, T3, TResult>? a, Func<T1, T2, T3, TResult> b)
        {
            a?.Remove(b);
            return a;
        }

        public static implicit operator MulticastFunc<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> f) => new MulticastFunc<T1, T2, T3, TResult>(f);

        public static explicit operator Func<T1, T2, T3, TResult>?(MulticastFunc<T1, T2, T3, TResult>? m)
        {
            Func<T1, T2, T3, TResult>? f = default;
            if (m != null)
            {
                foreach (var func in m.funcs)
                {
                    f += func;
                }
            }
            return f;
        }

        public MulticastFunc(Func<T1, T2, T3, TResult> func) => Add(func);
        private MulticastFunc() { }

        public TResult[] Invoke(T1 arg1, T2 arg2, T3 arg3)
        {
            var results = new TResult[Count];
            Invoke(arg1, arg2, arg3, results);
            return results;
        }

        public ReadOnlySpan<TResult> Invoke(T1 arg1, T2 arg2, T3 arg3, Span<TResult> buffer)
        {
            var count = funcs.Count;
            if (buffer.Length < count)
                throw new ArgumentException("Buffer is too small", nameof(buffer));
            for (int i = 0; i < count; i++)
            {
                buffer[i] = funcs[i](arg1, arg2, arg3);
            }
            return buffer[..count];
        }

        private void Add(Func<T1, T2, T3, TResult> func)
        {
            if (func == null) return;
            var functions = func.GetInvocationList();
            foreach (var function in functions)
            {
                funcs.Add((Func<T1, T2, T3, TResult>)function);
            }
        }

        private void Remove(Func<T1, T2, T3, TResult> func)
        {
            if (func == null) return;
            var removals = func.GetInvocationList();
            int removed = funcs.RemoveAll(x => Array.IndexOf(removals, x) != -1);
        }
    }
}
