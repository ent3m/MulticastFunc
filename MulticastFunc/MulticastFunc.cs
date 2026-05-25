using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MulticastFunc
{
    /// <summary>
    /// Represents an immutable, ordered collection of single-target delegates
    /// that can be invoked together, returning the result of each invocation.
    /// </summary>
    /// <typeparam name="TResult">The return type of the encapsulated delegates.</typeparam>
    public sealed class MulticastFunc<TResult>
    {
        #region Operators
        /// <summary>
        /// Combines two <c>MulticastFunc</c> instances into a new instance
        /// whose delegate list is the concatenation of <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        /// <param name="a">The left-hand operand. May be <see langword="null"/>.</param>
        /// <param name="b">The right-hand operand. May be <see langword="null"/>.</param>
        /// <returns>
        /// A new <c>MulticastFunc</c> containing all delegates from both operands,
        /// or <see langword="null"/> if both operands are <see langword="null"/>.
        /// </returns>
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

        /// <summary>
        /// Combines a <c>MulticastFunc</c> and a <c>Func</c> into a new instance
        /// whose delegate list appends all invocation targets of <paramref name="b"/>
        /// after those of <paramref name="a"/>.
        /// </summary>
        /// <param name="a">The left-hand operand. May be <see langword="null"/>.</param>
        /// <param name="b">The <c>Func</c> to append. May be <see langword="null"/>.</param>
        /// <returns>
        /// A new <c>MulticastFunc</c> containing all delegates from both operands,
        /// or <see langword="null"/> if both operands are <see langword="null"/>.
        /// </returns>
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

        /// <summary>
        /// Removes all delegates found in <paramref name="b"/> from <paramref name="a"/>.
        /// </summary>
        /// <param name="a">The source instance to remove delegates from. May be <see langword="null"/>.</param>
        /// <param name="b">The instance whose delegates should be removed. May be <see langword="null"/>.</param>
        /// <returns>
        /// A new <c>MulticastFunc</c> with the matching delegates removed,
        /// <paramref name="a"/> unchanged if no matches were found, or <see langword="null"/> if all
        /// delegates were removed or <paramref name="a"/> was <see langword="null"/>.
        /// </returns>
        [return: MaybeNull]
        public static MulticastFunc<TResult> operator -(
            [AllowNull] MulticastFunc<TResult> a,
            [AllowNull] MulticastFunc<TResult> b)
        {
            if (b == null)
                return a;
            return a?.Remove(b.delegates);
        }

        /// <summary>
        /// Removes all invocation targets of <paramref name="b"/> from <paramref name="a"/>.
        /// </summary>
        /// <param name="a">The source instance to remove delegates from. May be <see langword="null"/>.</param>
        /// <param name="b">The <c>Func</c> whose targets should be removed. May be <see langword="null"/>.</param>
        /// <returns>
        /// A new <c>MulticastFunc</c> with the matching delegates removed,
        /// <paramref name="a"/> unchanged if no matches were found, or <see langword="null"/> if all
        /// delegates were removed or <paramref name="a"/> was <see langword="null"/>.
        /// </returns>
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
        /// <summary>
        /// Implicitly converts a <c>Func</c> to a
        /// <c>MulticastFunc</c> wrapping all its invocation targets.
        /// </summary>
        /// <param name="f">The delegate to convert. May be <see langword="null"/>.</param>
        /// <returns>
        /// A new <c>MulticastFunc</c> containing all invocation targets of
        /// <paramref name="f"/>, or <see langword="null"/> if <paramref name="f"/> is <see langword="null"/>.
        /// </returns>
        [return: MaybeNull]
        [return: NotNullIfNotNull(nameof(f))]
        public static implicit operator MulticastFunc<TResult>(
            [AllowNull] Func<TResult> f)
            => f == null ? null : new MulticastFunc<TResult>(f.GetInvocationList());

        /// <summary>
        /// Explicitly converts a <c>MulticastFunc</c> to a single <c>Func</c>
        /// that, when invoked, calls all encapsulated delegates in order
        /// and returns the result of the last one.
        /// </summary>
        /// <param name="m">The instance to convert. May be <see langword="null"/>.</param>
        /// <returns>
        /// A <c>Func</c> combining all encapsulated delegates,
        /// or <see langword="null"/> if <paramref name="m"/> is <see langword="null"/>.
        /// </returns>
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
        /// <summary>
        /// Determines whether two <c>MulticastFunc</c> instances are equal,
        /// meaning they hold the same delegates in the same order.
        /// </summary>
        /// <param name="a">The left-hand operand. May be <see langword="null"/>.</param>
        /// <param name="b">The right-hand operand. May be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if both instances are equal; otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Determines whether two <c>MulticastFunc</c> instances are not equal.
        /// </summary>
        /// <param name="a">The left-hand operand. May be <see langword="null"/>.</param>
        /// <param name="b">The right-hand operand. May be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the instances differ; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(
            [AllowNull] MulticastFunc<TResult> a,
            [AllowNull] MulticastFunc<TResult> b)
            => !(a == b);

        /// <summary>
        /// Determines whether this instance is equal to another object.
        /// Two <c>MulticastFunc</c> instances are considered equal when they
        /// hold the same delegates in the same order.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <c>MulticastFunc</c> with an identical delegate list; otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Returns a hash code for this instance, derived from the contents of its delegate list.
        /// </summary>
        /// <returns>A hash code representing the current delegate list.</returns>
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
        /// The result of failed delegates will be <c>default(<typeparamref name="TResult"/>)</c>.
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
