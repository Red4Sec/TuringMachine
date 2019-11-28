using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using TuringMachine.Core.Extensions;

namespace TuringMachine.Core.Logs
{
	public class FuzzerError : IEquatable<FuzzerError>
	{
		public enum EFuzzingErrorType : byte
		{
			/// <summary>
			/// Fail
			/// </summary>
			Fail = 0x00,

			/// <summary>
			/// Crash
			/// </summary>
			Crash = 0x01
		}

		public enum EExplotationResult : byte
		{
			/// <summary>
			/// Unknown
			/// </summary>
			Unknown = 0x00,

			/// <summary>
			/// Exploitable
			/// </summary>
			Exploitable = 0x01,
		}

		/// <summary>
		/// Error Type
		/// </summary>
		[JsonProperty("e")]
		public EFuzzingErrorType Error { get; set; }

		/// <summary>
		/// Error Id
		/// </summary>
		[JsonProperty("h")]
		public Guid ErrorId { get; set; }

		/// <summary>
		/// Replication Data
		/// </summary>
		[JsonProperty("d")]
		public byte[] ReplicationData { get; set; }

		/// <summary>
		/// Explotation result
		/// </summary>
		[JsonProperty("r")]
		public EExplotationResult ExplotationResult { get; set; }

		/// <summary>
		/// Log Error
		/// </summary>
		/// <param name="onlyIfUnique">Only is is unique</param>
		public void Save(bool onlyIfUnique)
		{
			if (ReplicationData == null || ReplicationData.Length <= 0)
			{
				return;
			}

			var step = Error == EFuzzingErrorType.Crash ? "crashes" : "failures";
			var name = $"{step}/{DateTime.UtcNow.ToString("yyyy-MM-dd")}/uniques/{ErrorId.ToString()}.zip";

			if (!File.Exists(name))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(name));
				File.WriteAllBytes(name, ReplicationData);
			}

			if (onlyIfUnique)
			{
				return;
			}

			name = $"{step}/{DateTime.UtcNow.ToString("yyyy-MM-dd")}/{Guid.NewGuid().ToString()}.zip";
			Directory.CreateDirectory(Path.GetDirectoryName(name));
			File.WriteAllBytes(name, ReplicationData);
		}

		/// <summary>
		/// Equals
		/// </summary>
		/// <param name="obj">Object</param>
		/// <returns>Return true if are equals</returns>
		public bool Equals(FuzzerError obj)
		{
			if (obj == null) return false;

			return Error == obj.Error
				&& ErrorId == obj.ErrorId
				&& ExplotationResult == obj.ExplotationResult
				&& ((ReplicationData == null && obj.ReplicationData == null) ||
				(obj.ReplicationData != null && ReplicationData.SequenceEqual(obj.ReplicationData)));
		}

		/// <summary>
		/// Equals
		/// </summary>
		/// <param name="obj">Object</param>
		/// <returns>Return true if are equals</returns>
		public override bool Equals(object obj)
		{
			if (obj is FuzzerError o)
			{
				return Equals(o);
			}

			return false;
		}

		/// <summary>
		/// GetHashCode
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode()
		{
			var hashCode = -1592154030;
			hashCode = hashCode * -1521134295 + Error.GetHashCodeWithNullCheck();
			hashCode = hashCode * -1521134295 + ExplotationResult.GetHashCodeWithNullCheck();
			hashCode = hashCode * -1521134295 + ErrorId.GetHashCodeWithNullCheck();
			hashCode = hashCode * -1521134295 + ReplicationData.GetHashCodeWithNullCheck();
			return hashCode;
		}
	}
}
