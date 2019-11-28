using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Return true if are equals, with null check
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>True if are equals</returns>
        public static bool EqualWithNullCheck<T>(this IEquatable<T> a, IEquatable<T> b)
        {
            if ((a == null) != (b == null))
            {
                return false;
            }

            return a == null || a.Equals(b);
        }

        /// <summary>
        /// Return true if are equals, with null check
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>True if are equals</returns>
        public static bool SequenceEqualWithNullCheck<T>(this IList<T> a, IList<T> b)
        {
            if (typeof(T) == typeof(byte[]))
            {
                return ChunkSequenceEqualWithNullCheck((IList<byte[]>)a, (IList<byte[]>)b);
            }

            if ((a == null) != (b == null))
            {
                return false;
            }

            return a == null || a.SequenceEqual(b);
        }

        /// <summary>
        /// Return true if are equals, with null check
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>True if are equals</returns>
        private static bool ChunkSequenceEqualWithNullCheck(this IList<byte[]> chunks1, IList<byte[]> chunks2)
        {
            if ((chunks1 == null) != (chunks2 == null)) return false;
            if (chunks1 == null) return true;

            if (chunks1.Count != chunks2.Count)
            {
                return false;
            }

            for (int x = chunks1.Count - 1; x >= 0; x--)
            {
                if (!chunks1[x].SequenceEqual(chunks2[x]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Return true if are equals, with null check
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>True if are equals</returns>
        public static bool SequenceEqualWithNullCheck(this byte[] a, byte[] b)
        {
            if ((a == null) != (b == null))
            {
                return false;
            }

            return a == null || a.SequenceEqual(b);
        }

        /// <summary>
        /// Return true if are equals, with null check
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>True if are equals</returns>
        public static bool EqualWithNullCheck(this IPEndPoint a, IPEndPoint b)
        {
            if ((a == null) != (b == null))
            {
                return false;
            }

            return a == null || a.Equals(b);
        }

        /// <summary>
        /// GetHashCode checking if is null
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>GetHashCode</returns>
        public static int GetHashCodeWithNullCheck(this object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            if (obj is string str)
            {
                return EqualityComparer<string>.Default.GetHashCode(str);
            }

            if (obj is Guid uuid)
            {
                return BitConverter.ToInt32(uuid.ToByteArray(), 0);
            }

            if (obj is byte[] buffer)
            {
                switch (buffer.Length)
                {
                    case 0: return 0;
                    case 1: return buffer[0];
                    case 2: return BitConverter.ToInt16(buffer, 0);
                    case 4: return BitConverter.ToInt32(buffer, 0);

                    case 3:
                    default: return BitConverter.ToInt32(HashHelper.Sha256(buffer), 0);
                }
            }

            if (obj is IEnumerable list)
            {
                var ret = 0L;

                foreach (var entry in list)
                {
                    ret += entry.GetHashCodeWithNullCheck();
                }

                return ret.GetHashCodeWithNullCheck();
            }

            if (obj is IPEndPoint ep)
            {
                var hashCode = ep.Address.GetAddressBytes().GetHashCodeWithNullCheck();
                hashCode = hashCode * -1521134295 + ep.Port.GetHashCodeWithNullCheck();
                return hashCode;
            }

            return obj.GetHashCode();
        }
    }
}
