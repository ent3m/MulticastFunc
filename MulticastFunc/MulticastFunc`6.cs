using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MulticastFunc
{
    /// <inheritdoc cref="MulticastFunc{TResult}"/>
    public sealed class MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>
    {
        #region Operators
        /// <inheritdoc cref="MulticastFunc{TResult}.op_Addition(MulticastFunc{TResult},MulticastFunc{TResult})"/>
        [return: MaybeNull]
        [return: NotNullIfNotNull(nameof(a))]
        [return: NotNullIfNotNull(nameof(b))]
        public static MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> operator +(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> a,
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> b)
        {
            if (b == null)
                return a;
            if (a == null)
                return b;
            return a.Combine(b.delegates);
        }

        /// <inheritdoc cref="MulticastFunc{TResult}.op_Addition(MulticastFunc{TResult},Func{TResult})"/>
        [return: MaybeNull]
        [return: NotNullIfNotNull(nameof(a))]
        [return: NotNullIfNotNull(nameof(b))]
        public static MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> operator +(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> a,
            [AllowNull] Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> b)
        {
            if (b == null)
                return a;
            if (a == null)
                return b;
            return a.Combine(b.GetInvocationList());
        }

        /// <inheritdoc cref="MulticastFunc{TResult}.op_Subtraction(MulticastFunc{TResult},MulticastFunc{TResult})"/>
        [return: MaybeNull]
        public static MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> operator -(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> a,
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.delegates);
        }

        /// <inheritdoc cref="MulticastFunc{TResult}.op_Subtraction(MulticastFunc{TResult},Func{TResult})"/>
        [return: MaybeNull]
        public static MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> operator -(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> a,
            [AllowNull] Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.GetInvocationList());
        }
        #endregion

        #region Conversions
        /// <inheritdoc cref="MulticastFunc{TResult}.op_Implicit(Func{TResult})"/>
        [return: MaybeNull]
        [return: NotNullIfNotNull(nameof(f))]
        public static implicit operator MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(
            [AllowNull] Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> f)
            => f == null ? null : new MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(f.GetInvocationList());

        /// <inheritdoc cref="MulticastFunc{TResult}.op_Explicit(MulticastFunc{TResult})"/>
        [return: MaybeNull]
        [return: NotNullIfNotNull(nameof(m))]
        public static explicit operator Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> m)
        {
            if (m == null)
                return null;

            if (m.Count == 1 && m.delegates[0] is Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> f)
                return f;

            return (Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>)Delegate.Combine(m.delegates)!;
        }
        #endregion

        #region Equality
        /// <inheritdoc cref="MulticastFunc{TResult}.op_Equality(MulticastFunc{TResult},MulticastFunc{TResult})"/>
        public static bool operator ==(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> a,
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            return a.Equals(b);
        }

        /// <inheritdoc cref="MulticastFunc{TResult}.op_Inequality(MulticastFunc{TResult},MulticastFunc{TResult})"/>
        public static bool operator !=(
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> a,
            [AllowNull] MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> b)
            => !(a == b);

        /// <inheritdoc cref="MulticastFunc{TResult}.Equals(object)"/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (!(obj is MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> m))
                return false;

            return delegates.ArrayEqual(m.delegates);
        }

        /// <inheritdoc cref="MulticastFunc{TResult}.GetHashCode"/>
        public override int GetHashCode()
            => delegates.GetArrayHash();
        #endregion

        /// <inheritdoc cref="MulticastFunc{TResult}.Count"/>
        public int Count => delegates.Length;

        /// <inheritdoc cref="MulticastFunc{TResult}.Invoke()"/>
        public TResult[] Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            var results = new TResult[Count];
            FillBuffer(arg1, arg2, arg3, arg4, arg5, arg6, results);
            return results;
        }

        /// <inheritdoc cref="Invoke(TArg1, TArg2, TArg3, TArg4, TArg5, TArg6)"/>
        public ReadOnlySpan<TResult> Invoke(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, Span<TResult> spanBuffer)
        {
            var length = FillBuffer(arg1, arg2, arg3, arg4, arg5, arg6, spanBuffer);
            return spanBuffer[..length];
        }

        /// <inheritdoc cref="MulticastFunc{TResult}.InvokeAll()"/>
        public TResult[] InvokeAll(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            var results = new TResult[Count];
            FillBufferAndAggregateExceptions(arg1, arg2, arg3, arg4, arg5, arg6, results);
            return results;
        }

        /// <inheritdoc cref="InvokeAll(TArg1, TArg2, TArg3, TArg4, TArg5, TArg6)"/>
        public ReadOnlySpan<TResult> InvokeAll(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, Span<TResult> spanBuffer)
        {
            FillBufferAndAggregateExceptions(arg1, arg2, arg3, arg4, arg5, arg6, spanBuffer);
            return spanBuffer[..delegates.Length];
        }

        private readonly Delegate[] delegates;

        private MulticastFunc(Delegate[] del)
        {
            delegates = del;
        }

        private MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> Combine(Delegate[] functions)
            => new MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(delegates.Combine(functions));

        private MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>? Remove(Delegate[] functions)
        {
            var results = delegates.Remove(functions);
            // Everything was removed
            if (results == null)
                return null;
            // Nothing was removed. No need to create a new MulticastFunc.
            if (ReferenceEquals(results, delegates))
                return this;
            // Some were removed
            return new MulticastFunc<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(results);
        }

        private int FillBuffer(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, Span<TResult> buffer)
        {
            var length = delegates.Length;
            if (buffer.Length < length)
                throw new ArgumentException("Buffer is too small", nameof(buffer));
            for (int i = 0; i < length; i++)
            {
                var func = (Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>)delegates[i];
                buffer[i] = func(arg1, arg2, arg3, arg4, arg5, arg6);
            }
            return length;
        }

        private void FillBufferAndAggregateExceptions(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, Span<TResult> buffer)
        {
            var length = delegates.Length;
            if (buffer.Length < length)
                throw new ArgumentException("Buffer is too small", nameof(buffer));

            List<Exception>? exceptions = null;
            for (int i = 0; i < length; i++)
            {
                var func = (Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>)delegates[i];
                try
                {
                    buffer[i] = func(arg1, arg2, arg3, arg4, arg5, arg6);
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
