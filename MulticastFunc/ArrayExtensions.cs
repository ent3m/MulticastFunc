using System;
using System.Buffers;

namespace MulticastFunc
{
    internal static class ArrayExtensions
    {
        /// <summary>
        /// Combine the content of A and B without modifying A or B and return the result.
        /// </summary>
        internal static T[] Combine<T>(this T[] A, T[] B)
        {
            T[] result = new T[A.Length + B.Length];
            for (int i = 0; i < A.Length; i++)
            {
                result[i] = A[i];
            }
            for (int i = 0; i < B.Length; i++)
            {
                result[i + A.Length] = B[i];
            }
            return result;
        }

        /// <summary>
        /// Remove the content of B from A without modifying A or B and return the result.
        /// </summary>
        internal static T[]? Remove<T>(this T[] A, T[] B, bool canMutateB)
        {
            var pool = ArrayPool<T>.Shared;
            T[] removals;
            if (canMutateB)
            {
                removals = B;
            }
            else
            {
                removals = pool.Rent(B.Length);
                B.CopyTo(removals, 0);
            }
            int removalCount = removals.Length;

            var buffer = pool.Rent(A.Length);
            int length = 0;
            for (int i = 0; i < A.Length; i++)
            {
                var index = Array.IndexOf(removals, A[i], 0, removalCount);
                if (index == -1)
                    buffer[length++] = A[i];
                else
                    removals[index] = removals[--removalCount];
            }

            if (length == 0)
                return null;

            var result = new T[length];
            Array.Copy(buffer, result, length);
            pool.Return(buffer);
            if (!canMutateB)
                pool.Return(removals);
            return result;
        }
    }
}
