using System;
using System.Text;

namespace TuringMachine.Core.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// Byte array to hex string
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>HexString</returns>
        public static string ByteArrayToHex(this byte[] value)
        {
            var hex = new StringBuilder(value.Length * 2);

            foreach (byte b in value)
            {
                hex.AppendFormat("{0:X2}", b);
            }

            return hex.ToString();
        }

        /// <summary>
        /// Hex string to byte array
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Byte array</returns>
        public static byte[] HexToByteArray(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new byte[0];
            }

            if (value.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                value = value.Substring(2);
            }

            var count = value.Length;
            var bytes = new byte[count / 2];

            for (int i = 0; i < count; i += 2)
                bytes[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);

            return bytes;
        }
    }
}
