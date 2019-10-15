using System.Collections.Generic;
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
        public static string ByteArrayToHex(byte[] value)
        {
            var hex = new StringBuilder(value.Length * 2);

            foreach (byte b in value)
            {
                hex.AppendFormat("{0:X2}", b);
            }

            return hex.ToString();
        }
    }
}
