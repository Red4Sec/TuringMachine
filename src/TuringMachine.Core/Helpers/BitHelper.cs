using System;

namespace TuringMachine.Core.Helpers
{
    public static class BitHelper
    {
        /// <summary>
        /// Get bytes of input
        /// </summary>
        /// <param name="value">Input</param>
        public static byte[] GetBytes(this int value)
        {
            var buffer = BitConverter.GetBytes(value);
#if ALLOW_BIGENDIAN
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0, 4);
            }
#endif
            return buffer;
        }

        /// <summary>
        /// Get bytes of input
        /// </summary>
        /// <param name="value">Input</param>
        public static byte[] GetBytes(this ushort value)
        {
            var buffer = BitConverter.GetBytes(value);
#if ALLOW_BIGENDIAN
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0, 2);
            }
#endif
            return buffer;
        }

        /// <summary>
        /// Convert buffer to UInt16
        /// </summary>
        /// <param name="value">Buffer</param>
        /// <param name="index">Index</param>
        public static ushort ToUInt16(this byte[] value, int index)
        {
#if ALLOW_BIGENDIAN
            if (!BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt32(new byte[] { value[index + 1], value[index] }, 0);
            }
#endif
            return BitConverter.ToUInt16(value, index);
        }

        /// <summary>
        /// Convert buffer to Int32
        /// </summary>
        /// <param name="value">Buffer</param>
        /// <param name="index">Index</param>
        public static int ToInt32(this byte[] value, int index)
        {
#if ALLOW_BIGENDIAN
            if (!BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt32(new byte[] { value[index + 3],  value[index + 2],  value[index + 1], value[index] }, 0);
            }
#endif
            return BitConverter.ToInt32(value, index);
        }
    }
}
