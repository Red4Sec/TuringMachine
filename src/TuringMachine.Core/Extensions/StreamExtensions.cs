using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TuringMachine.Core.Extensions
{
	public static class StreamExtensions
	{
		/// <summary>
		/// Convert Stream to array
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <returns>Array</returns>
		public static byte[] ToArray(this Stream stream)
		{
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms, 1024);

				return ms.ToArray();
			}
		}

		/// <summary>
		/// Copy to
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <param name="endPoint">End Point</param>
		/// <param name="timeout">Timeout</param>
		public static bool ConnectoAndCopyTo(this Stream stream, IPEndPoint endPoint, TimeSpan timeout)
		{
			using (var client = new TcpClient())
			{
				var result = client.BeginConnect(endPoint.Address, endPoint.Port, null, null);
				var success = result.AsyncWaitHandle.WaitOne(timeout);

				if (success)
				{
					client.EndConnect(result);

					var s = client.GetStream();
					stream.CopyTo(s);
					s.Flush();

					return true;
				}
			}

			return false;
		}
	}
}
