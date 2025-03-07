using System;
using System.Collections.Generic;

namespace MulticastFunc
{
    public class MulticastFunc<T, TResult>
    {
        readonly List<Delegate> list = new List<Delegate>();
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
            a?.Remove(b);
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
            if (m != null)
            {
                foreach (var func in m.list)
                {
                    f += (Func<T, TResult>)func;
                }
            }
            return f;
        }

        public TResult[] Invoke(T arg)
        {
            var results = new TResult[list.Count];
            funcs?.Invoke(arg, results);
            return results;
        }

        private void Add(Func<T, TResult> func)
        {
            if (func == null) return;
            var functions = func.GetInvocationList();
            foreach (var function in functions)
            {
                list.Add(function);
                var f = (Func<T, TResult>)function;
                int j = list.Count - 1;
                funcs += (a, x) => x[j] = f(a);
            }
        }

        private void Remove(Func<T, TResult> func)
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
                    var f = (Func<T, TResult>)list[i];
                    funcs += (a, x) => x[j] = f(a);
                }
            }
        }

        private MulticastFunc() { }
    }
}
