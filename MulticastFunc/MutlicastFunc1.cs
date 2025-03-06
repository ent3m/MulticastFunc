using System;
using System.Collections.Generic;

namespace MulticastFunc
{
    public class MulticastFunc<T, TResult>
    {
        readonly List<Func<T, TResult>> list = new List<Func<T, TResult>>();
        Action<T, TResult[]>? funcs;

        public int Count => list.Count;

        public static MulticastFunc<T, TResult> operator +(MulticastFunc<T, TResult>? a, Func<T, TResult> b)
        {
            a ??= new MulticastFunc<T, TResult>();
            a.Add(b);
            return a;
        }

        public static MulticastFunc<T, TResult>? operator -(MulticastFunc<T, TResult>? a, Func<T, TResult> b)
        {
            if (a == null)
                return a;
            a.Remove(b);
            return a;
        }

        public static implicit operator MulticastFunc<T, TResult>(Func<T, TResult> f)
        {
            MulticastFunc<T, TResult> m = new MulticastFunc<T, TResult>();
            m.Add(f);
            return m;
        }

        public static explicit operator Func<T, TResult>?(MulticastFunc<T, TResult>? m)
        {
            Func<T, TResult>? f = null;
            if (m == null)
                return f;
            foreach (var func in m.list)
            {
                f += func;
            }
            return f;
        }

        public TResult[] Invoke(T arg)
        {
            var results = new TResult[list.Count];
            funcs!.Invoke(arg, results);
            return results;
        }

        private void Add(Func<T, TResult> func)
        {
            var functions = func.GetInvocationList();
            foreach (var function in functions)
            {
                var f = (Func<T, TResult>)function;
                list.Add(f);
                int j = list.Count - 1;
                funcs += (a, x) => x[j] = f(a);
            }
        }

        private void Remove(Func<T, TResult> func)
        {
            var removal = new HashSet<Delegate>(func.GetInvocationList());
            list.RemoveAll(x => removal.Contains(x));
            funcs = null;
            for (int i = 0; i < list.Count; i++)
            {
                int j = i;
                funcs += (a, x) => x[j] = list[j](a);
            }
        }

        private MulticastFunc() { }
    }
}
