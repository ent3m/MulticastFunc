using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
            return new(a._delegates.Combine(b._delegates));
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
            return a + CreateMulticastFunc(b);
        }

        [return: MaybeNull]
        public static MulticastFunc<TResult> operator -(
            [AllowNull] MulticastFunc<TResult> a,
            [AllowNull] MulticastFunc<TResult> b)
        {
            if (b == null)
                return a;
            var dels = a?._delegates.Remove(b._delegates);
            return dels == null ? null : new(dels);
        }

        [return: MaybeNull]
        public static MulticastFunc<TResult> operator -(
            [AllowNull] MulticastFunc<TResult> a,
            [AllowNull] Func<TResult> b)
        {
            if (b == null)
                return a;
            return a - CreateMulticastFunc(b);
        }
        #endregion

        #region Conversions
        [return: MaybeNull]
        [return: NotNullIfNotNull(nameof(f))]
        public static implicit operator MulticastFunc<TResult>(
            [AllowNull] Func<TResult> f)
            => f == null ? null : CreateMulticastFunc(f);

        [return: MaybeNull]
        [return: NotNullIfNotNull(nameof(m))]
        public static explicit operator Func<TResult>(
            [AllowNull] MulticastFunc<TResult> m)
        {
            if (m == null)
                return null;

            if (m.Count == 1)
                return m._delegates[0].Value;

            return default!; // implement this
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
            if (obj is not MulticastFunc<TResult> m)
                return false;

            return _delegates.ArrayEqual(m._delegates); // Wrapper implements equality comparer
        }

        public override int GetHashCode()
            => _delegates.GetArrayHash(); // Implement this to compute hash based on Value
        #endregion

        /// <summary>
        /// The number of delegates this MulticastFunc is holding.
        /// </summary>
        public int Count => _delegates.Length;

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
            FillBuffer(spanBuffer);
            return spanBuffer[..Count];
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
            return spanBuffer[..Count];
        }

        private Wrapper<Func<TResult>>[] _delegates;

        private MulticastFunc(Wrapper<Func<TResult>>[] delegates)
        {
            _delegates = delegates;
        }

        private static MulticastFunc<TResult> CreateMulticastFunc(Func<TResult> del)
        {
            if (del.HasSingleTarget)
                return new([ new(del) ]);

            int i = 0;
            foreach (var d in Delegate.EnumerateInvocationList(del))
            {
                i++;
            }
            var delegates = new Wrapper<Func<TResult>>[i];
            i = 0;
            foreach (var d in Delegate.EnumerateInvocationList(del))
            {
                delegates[i] = new(d);
            }
            return new(delegates);
        }

        private void FillBuffer(Span<TResult> buffer)
        {
            if (buffer.Length < Count)
                throw new ArgumentException("Buffer is too small", nameof(buffer));

            for (int i = 0; i < Count; i++)
            {
                var func = _delegates[i].Value;
                buffer[i] = func();
            }
        }

        private void FillBufferAndAggregateExceptions(Span<TResult> buffer)
        {
            if (buffer.Length < Count)
                throw new ArgumentException("Buffer is too small", nameof(buffer));

            List<Exception>? exceptions = null;
            for (int i = 0; i < Count; i++)
            {
                var func = _delegates[i].Value;
                try
                {
                    buffer[i] = func();
                }
                catch (Exception ex)
                {
                    exceptions ??= [];
                    exceptions.Add(ex);
                }
            }

            if (exceptions != null)
                throw new AggregateException("One or more delegates threw exceptions during invocation.", exceptions);
        }
    }
}
