using System;
using System.Diagnostics.CodeAnalysis;

namespace MulticastFunc
{
    public class MulticastFunc<TArg1, TArg2, TArg3, TResult>
    {
        [return: MaybeNull]
        public static MulticastFunc<TArg1, TArg2, TArg3, TResult> operator +(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TResult> a,
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TResult> b)
        {
            if (b == null)
                return a;
            if (a == null)
                return b;
            return a.Combine(b.delegates);
        }

        [return: MaybeNull]
        public static MulticastFunc<TArg1, TArg2, TArg3, TResult> operator +(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TResult> a,
            [AllowNull] Func<TArg1, TArg2, TArg3, TResult> b)
        {
            if (b == null)
                return a;
            if (a == null)
                return b;
            return a.Combine(b.GetInvocationList());
        }

        [return: MaybeNull]
        public static MulticastFunc<TArg1, TArg2, TArg3, TResult> operator -(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TResult> a,
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TResult> b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.delegates, false);
        }

        [return: MaybeNull]
        public static MulticastFunc<TArg1, TArg2, TArg3, TResult> operator -(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TResult> a,
            [AllowNull] Func<TArg1, TArg2, TArg3, TResult> b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.GetInvocationList(), true);
        }

        [return: MaybeNull]
        public static implicit operator MulticastFunc<TArg1, TArg2, TArg3, TResult>(
            [AllowNull] Func<TArg1, TArg2, TArg3, TResult> f)
            => f == null ? null : new MulticastFunc<TArg1, TArg2, TArg3, TResult>(f.GetInvocationList());

        [return: MaybeNull]
        public static explicit operator Func<TArg1, TArg2, TArg3, TResult>(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TResult> m)
        {
            if (m == null)
                return null;

            var dels = m.delegates;
            Func<TArg1, TArg2, TArg3, TResult>? result = default;
            for (int i = 0; i < dels.Length; i++)
            {
                result += (Func<TArg1, TArg2, TArg3, TResult>)dels[i];
            }
            return result;
        }

        private MulticastFunc(Delegate[] del)
        {
            delegates = del;
        }

        public TResult[] Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var results = new TResult[Count];
            Invoke(arg1, arg2, arg3, results);
            return results;
        }

        public ReadOnlySpan<TResult> Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, Span<TResult> buffer)
        {
            var length = delegates.Length;
            if (buffer.Length < length)
                throw new ArgumentException("Buffer is too small", nameof(buffer));
            for (int i = 0; i < length; i++)
            {
                var func = (Func<TArg1, TArg2, TArg3, TResult>)delegates[i];
                buffer[i] = func(arg1, arg2, arg3);
            }
            return buffer[..length];
        }

        private MulticastFunc<TArg1, TArg2, TArg3, TResult> Combine(Delegate[] functions)
            => new MulticastFunc<TArg1, TArg2, TArg3, TResult>(delegates.Combine(functions));

        private MulticastFunc<TArg1, TArg2, TArg3, TResult>? Remove(Delegate[] functions, bool canMutateFunctions)
        {
            var results = delegates.Remove(functions, canMutateFunctions);
            return results == null ? null : new MulticastFunc<TArg1, TArg2, TArg3, TResult>(results);
        }

        public int Count => delegates.Length;

        private readonly Delegate[] delegates;
    }
}
