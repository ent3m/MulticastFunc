using System;

namespace MulticastFunc
{
    internal static class ArrayExtensions
    {
        /// <summary>
        /// Remove B from A and return the result, matching <see cref="Delegate.Remove"/> behavior exactly:
        /// finds the last contiguous subsequence of A that equals B and removes it.
        /// Returns null if A and B are equal. Returns A if B is not found.
        /// </summary>
        internal static T[]? Remove<T>(this T[] a, T[] b) where T : notnull
        {
            if (b.Length == 0)
                return a;

            // Fast path: removing a single item. Scan backward and remove the last occurrence.
            if (b.Length == 1)
            {
                T item = b[0];
                for (int i = a.Length - 1; i >= 0; i--)
                {
                    if (item.Equals(a[i]))
                    {
                        if (a.Length == 1)
                            return null;

                        T[] single = new T[a.Length - 1];
                        Array.Copy(a, 0, single, 0, i);
                        Array.Copy(a, i + 1, single, i, a.Length - i - 1);
                        return single;
                    }
                }
                return a;
            }

            // General path: scan backward for the last contiguous subsequence of A that equals B.
            if (b.Length > a.Length)
                return a;

            for (int i = a.Length - b.Length; i >= 0; i--)
            {
                bool match = true;
                for (int j = 0; j < b.Length; j++)
                {
                    if (!a[i + j].Equals(b[j]))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    int remaining = a.Length - b.Length;
                    if (remaining == 0)
                        return null;

                    T[] result = new T[remaining];
                    Array.Copy(a, 0, result, 0, i);
                    Array.Copy(a, i + b.Length, result, i, a.Length - i - b.Length);
                    return result;
                }
            }

            return a;
        }

        /// <summary>
        /// Combine A and B and return the result.
        /// </summary>
        internal static T[] Combine<T>(this T[] a, T[] b)
        {
            T[] result = new T[a.Length + b.Length];
            Array.Copy(a, 0, result, 0, a.Length);
            Array.Copy(b, 0, result, a.Length, b.Length);
            return result;
        }

        /// <summary>
        /// Compare the contents of two arrays. Return true if all contents are equal.
        /// </summary>
        internal static bool ArrayEqual<T>(this T[] a, T[] b) where T : notnull
        {
            int length = a.Length;
            if (b.Length != length)
                return false;

            // Use the standard approach because we know the type being compared
            // (a Delegate) is not bitwise equatable and cannot use vectorized comparisons.
            for (int i = 0; i < length; i++)
            {
                if (!a[i].Equals(b[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Generate Hash based on the content of the array.
        /// </summary>
        internal static int GetArrayHash<T>(this T[] a) where T : notnull
        {
            int hash = 0;
            for (int i = 0; i < a.Length; i++)
            {
                hash = hash * 33 + a[i].GetHashCode();
            }
            return hash;
        }
    }
}
