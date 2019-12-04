using System;
using System.IO;
using System.Text;

namespace TuringMachine.Core.Helpers
{
	public static class StreamHelper
	{
		/// <summary>
		/// Max string length
		/// </summary>
		public const int MaxStringLength = 1024 * 1024 * 250;

		/// <summary>
		/// Read String
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <returns>JObject</returns>
		public static string ReadString(this Stream stream)
		{
			var buffer = new byte[sizeof(int)];
			if (ReadFull(stream, buffer, 0, buffer.Length) != buffer.Length)
			{
				throw new IOException();
			}

			var l = BitHelper.ToInt32(buffer, 0);

			if (l > MaxStringLength)
			{
				throw new IOException();
			}

			var str = new byte[l];

			if (ReadFull(stream, str, 0, l) != l)
			{
				throw new IOException();
			}

			return Encoding.UTF8.GetString(str);
		}

		/// <summary>
		/// Write String
		/// </summary>
		/// <param name="input">Input</param>
		/// <returns>Return length</returns>
		public static byte[] WriteString(this string input)
		{
			var data = Encoding.UTF8.GetBytes(input);
			var header = BitHelper.GetBytes(data.Length);

			var bulk = new byte[data.Length + header.Length];
			Array.Copy(header, 0, bulk, 0, header.Length);
			Array.Copy(data, 0, bulk, header.Length, data.Length);

			return bulk;
		}

		/// <summary>
		/// Write String
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <param name="input">Input</param>
		/// <returns>Return length</returns>
		public static int WriteString(this Stream stream, string input)
		{
			var bulk = WriteString(input);

			lock (stream)
			{
				stream.Write(bulk, 0, bulk.Length);
			}

			return bulk.Length;
		}

		/// <summary>
		/// Read all data
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <param name="data">Data</param>
		/// <param name="index">Index</param>
		/// <param name="count">Count</param>
		public static int ReadFull(this Stream stream, byte[] data, int index, int count)
		{
			var total = 0;

			while (count > 0)
			{
				var lee = stream.Read(data, index, count);
				if (lee <= 0)
				{
					break;
				}

				index += lee;
				count -= lee;
				total += lee;
			}

			return total;
		}

		/// <summary>
		/// Read all data
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <returns>Data</returns>
		public static byte[] ReadFull(this Stream stream)
		{
			using (var copy = new MemoryStream())
			{
				stream.CopyTo(copy, 4096);
				return copy.ToArray();
			}
		}
	}
}
