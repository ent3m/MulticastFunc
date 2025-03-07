using System;
using System.Collections.Generic;

namespace MulticastFunc
{
    public class MulticastFunc<T1, T2, T3, T4, T5, TResult>
    {
        readonly List<Delegate> list = new List<Delegate>();
        Action<T1, T2, T3, T4, T5, TResult[]>? funcs;

        public int Count => list.Count;

        public static MulticastFunc<T1, T2, T3, T4, T5, TResult> operator +(MulticastFunc<T1, T2, T3, T4, T5, TResult>? a, Func<T1, T2, T3, T4, T5, TResult> b)
        {
            a ??= new MulticastFunc<T1, T2, T3, T4, T5, TResult>();
            a.Add(b);
            return a;
        }

        public static MulticastFunc<T1, T2, T3, T4, T5, TResult>? operator -(MulticastFunc<T1, T2, T3, T4, T5, TResult>? a, Func<T1, T2, T3, T4, T5, TResult> b)
        {
            a?.Remove(b);
            return a;
        }

        public static implicit operator MulticastFunc<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> f)
        {
            MulticastFunc<T1, T2, T3, T4, T5, TResult> m = new MulticastFunc<T1, T2, T3, T4, T5, TResult>();
            m.Add(f);
            return m;
        }

        public static explicit operator Func<T1, T2, T3, T4, T5, TResult>?(MulticastFunc<T1, T2, T3, T4, T5, TResult>? m)
        {
            Func<T1, T2, T3, T4, T5, TResult>? f = null;
            if (m != null)
            {
                foreach (var func in m.list)
                {
                    f += (Func<T1, T2, T3, T4, T5, TResult>)func;
                }
            }
            return f;
        }

        public TResult[] Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var results = new TResult[list.Count];
            funcs?.Invoke(arg1, arg2, arg3, arg4, arg5, results);
            return results;
        }

        private void Add(Func<T1, T2, T3, T4, T5, TResult> func)
        {
            if (func == null) return;
            var functions = func.GetInvocationList();
            foreach (var function in functions)
            {
                list.Add(function);
                var f = (Func<T1, T2, T3, T4, T5, TResult>)function;
                int j = list.Count - 1;
                funcs += (a1, a2, a3, a4, a5, x) => x[j] = f(a1, a2, a3, a4, a5);
            }
        }

        private void Remove(Func<T1, T2, T3, T4, T5, TResult> func)
        {
            if (func == null) return;
            var removals = func.GetInvocationList();
            int removed = list.RemoveAll(x => Array.IndexOf(removals, x) != -1);
            if (removed > 0)
            {
                funcs = null;
                for (int i = 0; i < list.Count; i++)
                {
                    int j = i;
                    var f = (Func<T1, T2, T3, T4, T5, TResult>)list[i];
                    funcs += (a1, a2, a3, a4, a5, x) => x[j] = f(a1, a2, a3, a4, a5);
                }
            }
        }

        private MulticastFunc() { }
    }
}
