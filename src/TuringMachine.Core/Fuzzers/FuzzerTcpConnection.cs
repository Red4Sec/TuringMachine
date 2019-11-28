using System;
using System.Net;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Fuzzers
{
	public class FuzzerTcpConnection : FuzzerConnectionBase, IEquatable<FuzzerTcpConnection>
	{
		/// <summary>
		/// EndPoint
		/// </summary>
		public IPEndPoint EndPoint { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public FuzzerTcpConnection() : base() { }

		/// <summary>
		/// Equals
		/// </summary>
		/// <param name="obj">Object</param>
		/// <returns>Return true if are equals</returns>
		public bool Equals(FuzzerTcpConnection obj)
		{
			if (obj == null) return false;

			return EndPoint.ToString() == obj.EndPoint.ToString();
		}

		/// <summary>
		/// Equals
		/// </summary>
		/// <param name="obj">Object</param>
		/// <returns>Return true if are equals</returns>
		public override bool Equals(object obj)
		{
			if (obj is FuzzerTcpConnection log)
			{
				return Equals(log);
			}

			return false;
		}

		/// <summary>
		/// GetHashCode
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode()
		{
			var hashCode = 1594954113;
			hashCode = hashCode * -1521134295 + EndPoint.GetHashCodeWithNullCheck();
			return hashCode;
		}
	}
}
