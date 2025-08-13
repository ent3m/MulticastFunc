using System;

namespace MulticastFunc
{
    public class MulticastFunc<TArg1, TArg2, TResult>
    {
        public static MulticastFunc<TArg1, TArg2, TResult> operator +(MulticastFunc<TArg1, TArg2, TResult>? a, MulticastFunc<TArg1, TArg2, TResult>? b)
        {
            if (b == null)
                return a!;
            if (a == null)
                return b;
            return a.Combine(b.delegates);
        }

        public static MulticastFunc<TArg1, TArg2, TResult> operator +(MulticastFunc<TArg1, TArg2, TResult>? a, Func<TArg1, TArg2, TResult>? b)
        {
            if (b == null)
                return a!;
            if (a == null)
                return b;
            return a.Combine(b.GetInvocationList());
        }

        public static MulticastFunc<TArg1, TArg2, TResult>? operator -(MulticastFunc<TArg1, TArg2, TResult>? a, MulticastFunc<TArg1, TArg2, TResult>? b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.delegates, false);
        }

        public static MulticastFunc<TArg1, TArg2, TResult>? operator -(MulticastFunc<TArg1, TArg2, TResult>? a, Func<TArg1, TArg2, TResult>? b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.GetInvocationList(), true);
        }

        public static implicit operator MulticastFunc<TArg1, TArg2, TResult>(Func<TArg1, TArg2, TResult> f) => new MulticastFunc<TArg1, TArg2, TResult>(f.GetInvocationList());

        public static explicit operator Func<TArg1, TArg2, TResult>(MulticastFunc<TArg1, TArg2, TResult> m)
        {
            var dels = m.delegates;
            Func<TArg1, TArg2, TResult>? result = default;
            for (int i = 0; i < dels.Length; i++)
            {
                result += (Func<TArg1, TArg2, TResult>)dels[i];
            }
            return result!;
        }

        private MulticastFunc(Delegate[] del)
        {
            delegates = del;
        }

        public TResult[] Invoke(TArg1 arg1, TArg2 arg2)
        {
            var results = new TResult[Count];
            Invoke(arg1, arg2, results);
            return results;
        }

        public ReadOnlySpan<TResult> Invoke(TArg1 arg1, TArg2 arg2, Span<TResult> buffer)
        {
            var length = delegates.Length;
            if (buffer.Length < length)
                throw new ArgumentException("Buffer is too small", nameof(buffer));
            for (int i = 0; i < length; i++)
            {
                var func = (Func<TArg1, TArg2, TResult>)delegates[i];
                buffer[i] = func(arg1, arg2);
            }
            return buffer[..length];
        }

        private MulticastFunc<TArg1, TArg2, TResult> Combine(Delegate[] functions)
            => new MulticastFunc<TArg1, TArg2, TResult>(delegates.Combine(functions));

        private MulticastFunc<TArg1, TArg2, TResult>? Remove(Delegate[] functions, bool canMutateFunctions)
        {
            var results = delegates.Remove(functions, canMutateFunctions);
            return results == null ? null : new MulticastFunc<TArg1, TArg2, TResult>(results);
        }

        public int Count => delegates.Length;

        private readonly Delegate[] delegates;
    }
}
