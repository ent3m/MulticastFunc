using System;
using System.Collections.Generic;

namespace MulticastFunc
{
    public class MulticastFunc<TResult>
    {
        readonly List<Func<TResult>> list = new List<Func<TResult>>();
        Action<TResult[]>? funcs;

        public int Count => list.Count;

        public static MulticastFunc<TResult> operator +(MulticastFunc<TResult>? a, Func<TResult> b)
        {
            a ??= new MulticastFunc<TResult>();
            a.Add(b);
            return a;
        }

        public static MulticastFunc<TResult>? operator -(MulticastFunc<TResult>? a, Func<TResult> b)
        {
            if (a == null)
                return a;
            a.Remove(b);
            return a;
        }

        public static implicit operator MulticastFunc<TResult>(Func<TResult> f)
        {
            MulticastFunc<TResult> m = new MulticastFunc<TResult>();
            m.Add(f);
            return m;
        }

        public static explicit operator Func<TResult>?(MulticastFunc<TResult>? m)
        {
            Func<TResult>? f = null;
            if (m == null)
                return f;
            foreach (var func in m.list)
            {
                f += func;
            }
            return f;
        }

        public TResult[] Invoke()
        {
            var results = new TResult[list.Count];
            funcs!.Invoke(results);
            return results;
        }

        private void Add(Func<TResult> func)
        {
            var functions = func.GetInvocationList();
            foreach (var function in functions)
            {
                var f = (Func<TResult>)function;
                list.Add(f);
                int j = list.Count - 1;
                funcs += x => x[j] = f();
            }
        }

        private void Remove(Func<TResult> func)
        {
            var removal = new HashSet<Delegate>(func.GetInvocationList());
            list.RemoveAll(x => removal.Contains(x));
            funcs = null;
            for (int i = 0; i < list.Count; i++)
            {
                int j = i;
                funcs += x => x[j] = list[j]();
            }
        }

        private MulticastFunc() { }
    }
}
