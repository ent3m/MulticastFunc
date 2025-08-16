using System;
using System.Diagnostics.CodeAnalysis;

namespace MulticastFunc
{
    public sealed class MulticastFunc<TResult>
    {
        [return: MaybeNull]
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

        /// <summary>
        /// The number of delegates this MulticastFunc is holding.
        /// </summary>
        public int Count => delegates.Length;

        public TResult[] Invoke()
        {
            var results = new TResult[Count];
            FillBuffer(results);
            return results;
        }

        public int Invoke(TResult[] buffer)
            => FillBuffer(buffer);

        public ReadOnlySpan<TResult> Invoke(Span<TResult> spanBuffer)
        {
            var length = FillBuffer(spanBuffer);
            return spanBuffer[..length];
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj)) 
                return true;
            if (!(obj is MulticastFunc<TResult> m))
                return false;
            
            return delegates.ArrayEqual(m.delegates);
        }

        public override int GetHashCode()
            => delegates.GetArrayHash();

        private MulticastFunc<TResult> Combine(Delegate[] functions)
            => new MulticastFunc<TResult>(delegates.Combine(functions));

        private MulticastFunc<TResult>? Remove(Delegate[] functions, bool canMutateFunctions)
        {
            var results = delegates.Remove(functions, canMutateFunctions);
            return results == null ? null : new MulticastFunc<TResult>(results);
        }

        private int FillBuffer(Span<TResult> buffer)
        {
            var length = delegates.Length;
            if (buffer.Length < length)
                throw new ArgumentException("Buffer is too small", nameof(buffer));
            for (int i = 0; i < length; i++)
            {
                var func = (Func<TResult>)delegates[i];
                buffer[i] = func();
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
