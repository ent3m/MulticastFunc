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
            if (a.Count == 0)
                return null!;
            return a;
        }

        public static MulticastFunc<T, TResult>? operator -(MulticastFunc<T, TResult>? a, Func<T, TResult> b)
        {
            a?.Remove(b);
            if (a != null && a.Count == 0)
                return null;
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
