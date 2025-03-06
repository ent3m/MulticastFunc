using System;
using System.Collections.Generic;

namespace MulticastFunc
{
    public class MulticastFunc<T1, T2, T3, TResult>
    {
        readonly List<Func<T1, T2, T3, TResult>> list = new List<Func<T1, T2, T3, TResult>>();
        Action<T1, T2, T3, TResult[]>? funcs;

        public int Count => list.Count;

        public static MulticastFunc<T1, T2, T3, TResult> operator +(MulticastFunc<T1, T2, T3, TResult>? a, Func<T1, T2, T3, TResult> b)
        {
            a ??= new MulticastFunc<T1, T2, T3, TResult>();
            a.Add(b);
            return a;
        }

        public static MulticastFunc<T1, T2, T3, TResult>? operator -(MulticastFunc<T1, T2, T3, TResult>? a, Func<T1, T2, T3, TResult> b)
        {
            if (a == null)
                return a;
            a.Remove(b);
            return a;
        }

        public static implicit operator MulticastFunc<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> f)
        {
            MulticastFunc<T1, T2, T3, TResult> m = new MulticastFunc<T1, T2, T3, TResult>();
            m.Add(f);
            return m;
        }

        public static explicit operator Func<T1, T2, T3, TResult>?(MulticastFunc<T1, T2, T3, TResult>? m)
        {
            Func<T1, T2, T3, TResult>? f = null;
            if (m == null)
                return f;
            foreach (var func in m.list)
            {
                f += func;
            }
            return f;
        }

        public TResult[] Invoke(T1 arg1, T2 arg2, T3 arg3)
        {
            var results = new TResult[list.Count];
            funcs!.Invoke(arg1, arg2, arg3, results);
            return results;
        }

        private void Add(Func<T1, T2, T3, TResult> func)
        {
            var functions = func.GetInvocationList();
            foreach (var function in functions)
            {
                var f = (Func<T1, T2, T3, TResult>)function;
                list.Add(f);
                int j = list.Count - 1;
                funcs += (a1, a2, a3, x) => x[j] = f(a1, a2, a3);
            }
        }

        private void Remove(Func<T1, T2, T3, TResult> func)
        {
            var removal = new HashSet<Delegate>(func.GetInvocationList());
            list.RemoveAll(x => removal.Contains(x));
            funcs = null;
            for (int i = 0; i < list.Count; i++)
            {
                int j = i;
                funcs += (a1, a2, a3, x) => x[j] = list[j](a1, a2, a3);
            }
        }

        private MulticastFunc() { }
    }
}
