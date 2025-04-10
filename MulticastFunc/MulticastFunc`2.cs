using System;
using System.Collections.Generic;

namespace MulticastFunc
{
    public class MulticastFunc<T1, T2, TResult>
    {
        readonly List<Func<T1, T2, TResult>> funcs = new List<Func<T1, T2, TResult>>();

        public int Count => funcs.Count;

        public static MulticastFunc<T1, T2, TResult> operator +(MulticastFunc<T1, T2, TResult>? a, Func<T1, T2, TResult> b)
        {
            a ??= new MulticastFunc<T1, T2, TResult>();
            a.Add(b);
            if (a.Count == 0)
                return null!;
            return a;
        }

        public static MulticastFunc<T1, T2, TResult>? operator -(MulticastFunc<T1, T2, TResult> a, Func<T1, T2, TResult> b)
        {
            a?.Remove(b);
            if (a != null && a.Count == 0)
                return null;
            return a;
        }

        public static implicit operator MulticastFunc<T1, T2, TResult>(Func<T1, T2, TResult> f) => new MulticastFunc<T1, T2, TResult>(f);

        public static explicit operator Func<T1, T2, TResult>?(MulticastFunc<T1, T2, TResult>? m)
        {
            Func<T1, T2, TResult>? f = default;
            if (m != null)
            {
                foreach (var func in m.funcs)
                {
                    f += func;
                }
            }
            return f;
        }

        public MulticastFunc(Func<T1, T2, TResult> func) => Add(func);
        private MulticastFunc() { }

        public TResult[] Invoke(T1 arg1, T2 arg2)
        {
            var results = new TResult[Count];
            Invoke(arg1, arg2, results);
            return results;
        }

        public ReadOnlySpan<TResult> Invoke(T1 arg1, T2 arg2, Span<TResult> buffer)
        {
            var count = funcs.Count;
            if (buffer.Length < count)
                throw new ArgumentException("Buffer is too small", nameof(buffer));
            for (int i = 0; i < count; i++)
            {
                buffer[i] = funcs[i](arg1, arg2);
            }
            return buffer[..count];
        }

        private void Add(Func<T1, T2, TResult> func)
        {
            if (func == null) return;
            var functions = func.GetInvocationList();
            foreach (var function in functions)
            {
                funcs.Add((Func<T1, T2, TResult>)function);
            }
        }

        private void Remove(Func<T1, T2, TResult> func)
        {
            if (func == null) return;
            var removals = func.GetInvocationList();

            // Remove removals from the list of funcs using two pointer method
            int freeIndex = 0;   // the first free slot in items array
            int removalsCount = removals.Length;    // the number of items to remove

            // Return true if item is in removals
            bool Match(Delegate item)
            {
                int index = Array.IndexOf(removals, item, 0, removalsCount);
                if (index != -1)
                {
                    // avoid removing the same item twice
                    removals[index] = removals[--removalsCount];
                    return true;
                }
                return false;
            }

            // Find the first item which needs to be removed
            while (freeIndex < funcs.Count && !Match(funcs[freeIndex])) freeIndex++;
            if (freeIndex >= funcs.Count) return;

            int current = freeIndex + 1;
            while (current < funcs.Count)
            {
                // Find the first item which needs to be kept
                while (current < funcs.Count && Match(funcs[current])) current++;

                if (current < funcs.Count)
                {
                    // copy item to the free slot
                    funcs[freeIndex++] = funcs[current++];
                }
            }

            funcs.RemoveRange(freeIndex, funcs.Count - freeIndex);
            return;
        }
    }
}
