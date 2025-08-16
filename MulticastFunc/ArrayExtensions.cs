using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MulticastFunc
{
    // Methods in this class are implemented as generic because the backing type of MulticastFunc may change in the future.
    internal static class ArrayExtensions
    {
        /// <summary>
        /// Combine the content of A and B without modifying A or B and return the result.
        /// </summary>
        internal static T[] Combine<T>(this T[] A, T[] B)
        {
            T[] result = new T[A.Length + B.Length];
            Array.Copy(A, 0, result, 0, A.Length);
            Array.Copy(B, 0, result, A.Length, B.Length);
            return result;
        }

        /// <summary>
        /// Remove the content of B from A without modifying A or B and return the result. Optionally, allow the mutation of B to reduce allocation.
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
            int toRemove = removals.Length;

            // Copy each item from A to a buffer, excluding those that exist in removals.
            var buffer = pool.Rent(A.Length);
            int written = 0;
            for (int i = 0; i < A.Length; i++)
            {
                // Copy the remainder of A and return early if there's nothing left to remove.
                if (toRemove == 0)
                {
                    var remainder = A.Length - i;
                    Array.Copy(A, i, buffer, written, remainder);
                    written += remainder;
                    break;
                }

                var index = Array.IndexOf(removals, A[i], 0, toRemove);
                if (index == -1)
                    buffer[written++] = A[i];
                // Avoid removing the same item twice.
                else
                    removals[index] = removals[--toRemove];
            }

            T[]? result;
            // Return null if all items were removed.
            if (written == 0)
            {
                result = null;
            }
            // Return A if nothing was removed.
            else if (toRemove == removals.Length)
            {
                result = A;
            }
            else
            {
                result = new T[written];
                Array.Copy(buffer, result, written);
            }

            pool.Return(buffer);
            if (!canMutateB)
                pool.Return(removals);
            return result;
        }

        /// <summary>
        /// Compare the contents of two arrays. Return true if all contents are equal.
        /// </summary>
        internal static bool ArrayEqual<T>(this T[] A, T[] B) where T : notnull
        {
            int length = A.Length;
            if (B.Length != length) 
                return false;

            // Use the standard approach because we know the type being compared (a Delegate) is not bitwise equatable.
            for (int i = 0; i < length; i++)
            {
                if (!A[i].Equals(B[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Generate Hash based on the content of the array.
        /// </summary>
        internal static int GetArrayHash<T>(this T[] A) where T : notnull
        {
            int length = A.Length;
            if (length == 1)
                return A[0].GetHashCode();

            // Apply the same algorithm as MulticastDelegate.
            int hash = 0;
            for (int i = 0; i < length; i++)
            {
                hash = hash * 33 + A[i].GetHashCode();
            }
            return hash;
        }
    }
}
