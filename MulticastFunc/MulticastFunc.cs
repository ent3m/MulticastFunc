using System;
using System.Collections.Generic;

namespace MulticastFunc
{
    public class MulticastFunc<TResult>
    {
        readonly List<Delegate> list = new List<Delegate>();
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
            a?.Remove(b);
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
            if (m != null)
            {
                foreach (var func in m.list)
                {
                    f += (Func<TResult>)func;
                }
            }
            return f;
        }

        public TResult[] Invoke()
        {
            var results = new TResult[list.Count];
            funcs?.Invoke(results);
            return results;
        }

        private void Add(Func<TResult> func)
        {
            if (func == null) return;
            var functions = func.GetInvocationList();
            foreach (var function in functions)
            {
                list.Add(function);
                var f = (Func<TResult>)function;
                int j = list.Count - 1;
                funcs += x => x[j] = f();
            }
        }

        private void Remove(Func<TResult> func)
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
                    var f = (Func<TResult>)list[i];
                    funcs += x => x[j] = f();
                }
            }
        }

        private MulticastFunc() { }
    }
}
