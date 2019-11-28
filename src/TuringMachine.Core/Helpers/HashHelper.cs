using System.Security.Cryptography;

namespace TuringMachine.Core.Helpers
{
	public static class HashHelper
	{
		/// <summary>
		/// Get SHA256 hash
		/// </summary>
		/// <param name="data">Input</param>
		/// <returns>SHA256</returns>
		public static byte[] Sha256(this byte[] data)
		{
			using (var hasher = SHA256.Create())
			{
				return hasher.ComputeHash(data);
			}
		}

		/// <summary>
		/// Get MD5 hash
		/// </summary>
		/// <param name="data">Input</param>
		/// <returns>MD5</returns>
		public static byte[] Md5(this byte[] data)
		{
			using (var hasher = MD5.Create())
			{
				return hasher.ComputeHash(data);
			}
		}
	}
}
