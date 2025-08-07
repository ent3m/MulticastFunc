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
            if (a.Count == 0)
                return null!;
            return a;
        }

        public static MulticastFunc<TResult>? operator -(MulticastFunc<TResult>? a, Func<TResult> b)
        {
            a?.Remove(b);
            if (a != null && a.Count == 0)
                return null;
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
