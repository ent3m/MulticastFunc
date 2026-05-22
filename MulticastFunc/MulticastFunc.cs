using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MulticastFunc
{
    public sealed class MulticastFunc<TResult>
    {
        #region Operators
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
            return a?.Remove(b.delegates);
        }

        [return: MaybeNull]
        public static MulticastFunc<TResult> operator -(
            [AllowNull] MulticastFunc<TResult> a,
            [AllowNull] Func<TResult> b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.GetInvocationList());
        }
        #endregion

        #region Conversions
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

            if (m.Count == 1 && m.delegates[0] is Func<TResult> f)
                return f;

            return (Func<TResult>)Delegate.Combine(m.delegates)!;
        }
        #endregion

        #region Equality
        public static bool operator ==(
            [AllowNull] MulticastFunc<TResult> a,
            [AllowNull] MulticastFunc<TResult> b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(
            [AllowNull] MulticastFunc<TResult> a,
            [AllowNull] MulticastFunc<TResult> b)
            => !(a == b);

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
        #endregion

        /// <summary>
        /// The number of delegates this MulticastFunc is holding.
        /// </summary>
        public int Count => delegates.Length;

        /// <summary>
        /// Invoke all delegates and return their results.
        /// </summary>
        public TResult[] Invoke()
        {
            var results = new TResult[Count];
            FillBuffer(results);
            return results;
        }

        /// <inheritdoc cref="Invoke()"/>
        public ReadOnlySpan<TResult> Invoke(Span<TResult> spanBuffer)
        {
            var length = FillBuffer(spanBuffer);
            return spanBuffer[..length];
        }

        /// <summary>
        /// Invokes all delegates regardless of individual failures.
        /// </summary>
        /// <remarks>
        /// All exceptions are collected and re-thrown as a single 
        /// <see cref="AggregateException"/> after every delegate has been invoked.
        /// The result of failed delegates will be default(<see cref="TResult"/>).
        /// </remarks>
        public TResult[] InvokeAll()
        {
            var results = new TResult[Count];
            FillBufferAndAggregateExceptions(results);
            return results;
        }

        /// <inheritdoc cref="InvokeAll()"/>
        public ReadOnlySpan<TResult> InvokeAll(Span<TResult> spanBuffer)
        {
            FillBufferAndAggregateExceptions(spanBuffer);
            return spanBuffer[..delegates.Length];
        }

        private readonly Delegate[] delegates;

        private MulticastFunc(Delegate[] del)
        {
            delegates = del;
        }

        private MulticastFunc<TResult> Combine(Delegate[] functions)
            => new MulticastFunc<TResult>(delegates.Combine(functions));

        private MulticastFunc<TResult>? Remove(Delegate[] functions)
        {
            var results = delegates.Remove(functions);
            // Everything was removed
            if (results == null)
                return null;
            // Nothing was removed. No need to create a new MulticastFunc.
            if (ReferenceEquals(results, delegates))
                return this;
            // Some were removed
            return new MulticastFunc<TResult>(results);
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

        private void FillBufferAndAggregateExceptions(Span<TResult> buffer)
        {
            var length = delegates.Length;
            if (buffer.Length < length)
                throw new ArgumentException("Buffer is too small", nameof(buffer));

            List<Exception>? exceptions = null;
            for (int i = 0; i < length; i++)
            {
                var func = (Func<TResult>)delegates[i];
                try
                {
                    buffer[i] = func();
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(ex);
                }
            }

            if (exceptions != null)
                throw new AggregateException("One or more delegates threw exceptions during invocation.", exceptions);
        }
    }
}
