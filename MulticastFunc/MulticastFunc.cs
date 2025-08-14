using System;
using System.Diagnostics.CodeAnalysis;

namespace MulticastFunc
{
    public class MulticastFunc<TResult>
    {
        [return : MaybeNull]
        [return: NotNullIfNotNull(nameof(a))]
        [return: NotNullIfNotNull(nameof(b))]
        public static MulticastFunc<TResult> operator +(
            [AllowNull] MulticastFunc<TResult> a,
            [AllowNull] MulticastFunc<TResult> b)
        {
            if (b == null)
                return a;
            if (a == null)
                return b;
            return a.Combine(b.delegates);
        }

        [return: MaybeNull]
        [return: NotNullIfNotNull(nameof(a))]
        [return: NotNullIfNotNull(nameof(b))]
        public static MulticastFunc<TResult> operator +(
            [AllowNull] MulticastFunc<TResult> a,
            [AllowNull] Func<TResult> b)
        {
            if (b == null)
                return a;
            if (a == null)
                return b;
            return a.Combine(b.GetInvocationList());
        }

        [return: MaybeNull]
        public static MulticastFunc<TResult> operator -(
            [AllowNull] MulticastFunc<TResult> a,
            [AllowNull] MulticastFunc<TResult> b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.delegates, false);
        }

        [return: MaybeNull]
        public static MulticastFunc<TResult> operator -(
            [AllowNull] MulticastFunc<TResult> a,
            [AllowNull] Func<TResult> b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.GetInvocationList(), true);
        }

        [return: MaybeNull]
        [return: NotNullIfNotNull(nameof(f))]
        public static implicit operator MulticastFunc<TResult>(
            [AllowNull] Func<TResult> f)
            => f == null ? null : new MulticastFunc<TResult>(f.GetInvocationList());

        [return: MaybeNull]
        [return: NotNullIfNotNull(nameof(m))]
        public static explicit operator Func<TResult>(
            [AllowNull] MulticastFunc<TResult> m)
        {
            if (m == null)
                return null;

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

        public ReadOnlySpan<TResult> Invoke(Span<TResult> buffer)
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

        private MulticastFunc<TResult>? Remove(Delegate[] functions, bool canMutateFunctions)
        {
            var results = delegates.Remove(functions, canMutateFunctions);
            return results == null ? null : new MulticastFunc<TResult>(results);
        }

        public int Count => delegates.Length;

        private readonly Delegate[] delegates;
    }
}
