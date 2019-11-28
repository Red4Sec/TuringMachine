using System;
using System.Net;

namespace TuringMachine.Core.Helpers
{
	public static class IPHelper
	{
		/// <summary>
		/// String to IpEndPoint
		/// </summary>
		/// <param name="value">Value</param>
		/// <returns>IpEndPoint</returns>
		public static IPEndPoint ToIpEndPoint(this string value)
		{
			var sp = value.Split(new char[] { ',', ';', '=' }, StringSplitOptions.RemoveEmptyEntries);
			return new IPEndPoint(IPAddress.Parse(sp[0]), Convert.ToInt32(sp[1]));
		}
	}
}
