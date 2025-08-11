using System;

namespace MulticastFunc
{
    public class MulticastFunc<TResult>
    {
        public static MulticastFunc<TResult> operator +(MulticastFunc<TResult>? a, MulticastFunc<TResult> b)
        {
            if (b == null)
                return a!;
            if (a == null)
                return b;
            return a.Combine(b.delegates);
        }

        public static MulticastFunc<TResult> operator +(MulticastFunc<TResult>? a, Func<TResult> b)
        {
            if (b == null)
                return a!;
            if (a == null)
                return b;
            return a.Combine(b.GetInvocationList());
        }

        public static MulticastFunc<TResult>? operator -(MulticastFunc<TResult>? a, MulticastFunc<TResult> b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.delegates);
        }

        public static MulticastFunc<TResult>? operator -(MulticastFunc<TResult>? a, Func<TResult> b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.GetInvocationList());
        }

        public static implicit operator MulticastFunc<TResult>(Func<TResult> f) => new MulticastFunc<TResult>(f.GetInvocationList());

        public static explicit operator Func<TResult>(MulticastFunc<TResult> m)
        {
            var dels = m.delegates;
            Func<TResult>? result = default;
            for (int i = 0; i < dels.Length; i++)
            {
                result += (Func<TResult>)dels[i];
            }
            return result!;
        }

        private MulticastFunc(Delegate[] del)
        {
            delegates = del;
        }

        public TResult[] Invoke()
        {
            var results = new TResult[Count];
            Invoke(results);
            return results;
        }

        public Span<TResult> Invoke(Span<TResult> buffer)
        {
            var length = delegates.Length;
            if (buffer.Length < length)
                throw new ArgumentException("Buffer is too small", nameof(buffer));
            for (int i = 0; i < length; i++)
            {
                var func = (Func<TResult>)delegates[i];
                buffer[i] = func();
            }
            return buffer[..length];
        }

        private MulticastFunc<TResult> Combine(Delegate[] functions)
            => new MulticastFunc<TResult>(delegates.Combine(functions));

        private MulticastFunc<TResult>? Remove(Delegate[] functions)
        {
            var results = delegates.Remove(functions);
            return results == null ? null : new MulticastFunc<TResult>(results);
        }

        public int Count => delegates.Length;

        private readonly Delegate[] delegates;
    }
}
