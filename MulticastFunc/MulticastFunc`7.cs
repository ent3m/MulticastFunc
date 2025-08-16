using System;
using System.Diagnostics.CodeAnalysis;

namespace MulticastFunc
{
    public sealed class MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>
    {
        [return: MaybeNull]
        [return: NotNullIfNotNull(nameof(a))]
        [return: NotNullIfNotNull(nameof(b))]
        public static MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> operator +(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> a,
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> b)
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
        public static MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> operator +(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> a,
            [AllowNull] Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> b)
        {
            if (b == null)
                return a;
            if (a == null)
                return b;
            return a.Combine(b.GetInvocationList());
        }

        [return: MaybeNull]
        public static MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> operator -(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> a,
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.delegates, false);
        }

        [return: MaybeNull]
        public static MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> operator -(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> a,
            [AllowNull] Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.GetInvocationList(), true);
        }

        [return: MaybeNull]
        [return: NotNullIfNotNull(nameof(f))]
        public static implicit operator MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(
            [AllowNull] Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> f)
            => f == null ? null : new MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(f.GetInvocationList());

        [return: MaybeNull]
        [return: NotNullIfNotNull(nameof(m))]
        public static explicit operator Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> m)
        {
            if (m == null)
                return null;

            var dels = m.delegates;
            Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>? result = default;
            for (int i = 0; i < dels.Length; i++)
            {
                result += (Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>)dels[i];
            }
            return result!;
        }

        /// <summary>
        /// The number of delegates this MulticastFunc is holding.
        /// </summary>
        public int Count => delegates.Length;

        public TResult[] Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7)
        {
            var results = new TResult[Count];
            FillBuffer(arg1, arg2, arg3, arg4, arg5, arg6, arg7, results);
            return results;
        }

        public int Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TResult[] buffer)
            => FillBuffer(arg1, arg2, arg3, arg4, arg5, arg6, arg7, buffer);

        public ReadOnlySpan<TResult> Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, Span<TResult> spanBuffer)
        {
            var length = FillBuffer(arg1, arg2, arg3, arg4, arg5, arg6, arg7, spanBuffer);
            return spanBuffer[..length];
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (!(obj is MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> m))
                return false;

            return delegates.ArrayEqual(m.delegates);
        }

        public override int GetHashCode()
            => delegates.GetArrayHash();

        private MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> Combine(Delegate[] functions)
            => new MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(delegates.Combine(functions));

        private MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>? Remove(Delegate[] functions, bool canMutateFunctions)
        {
            var results = delegates.Remove(functions, canMutateFunctions);
            return results == null ? null : new MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(results);
        }

        private int FillBuffer(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, Span<TResult> buffer)
        {
            var length = delegates.Length;
            if (buffer.Length < length)
                throw new ArgumentException("Buffer is too small", nameof(buffer));
            for (int i = 0; i < length; i++)
            {
                var func = (Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>)delegates[i];
                buffer[i] = func(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
            return length;
        }

        private readonly Delegate[] delegates;

        private MulticastFunc(Delegate[] del)
        {
            delegates = del;
        }
    }
}
