using System;
using System.Diagnostics;
using System.Linq;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Fuzzers.Patch
{
	[DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
	public class PatchChange : IEquatable<PatchChange>
	{
		/// <summary>
		/// Description
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Offset
		/// </summary>
		public long Offset { get; set; }

		/// <summary>
		/// Remove
		/// </summary>
		public ushort Remove { get; set; }

		/// <summary>
		/// Append
		/// </summary>
		public byte[] Append { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public PatchChange() { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="description">Description</param>
		/// <param name="offset">Offset</param>
		/// <param name="remove">Remove</param>
		public PatchChange(string description, long offset, ushort remove)
		{
			Description = description;
			Offset = offset;
			Remove = remove;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="description">Description</param>
		/// <param name="offset">Offset</param>
		/// <param name="remove">Remove</param>
		/// <param name="append">Append</param>
		public PatchChange(string description, long offset, ushort remove, byte[] append) : this(description, offset, remove)
		{
			Append = append;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is PatchChange o))
			{
				return false;
			}

			return Equals(o);
		}

		public bool Equals(PatchChange obj)
		{
			if (obj == null) return false;

			return obj.Description == Description
				&& obj.Offset == Offset
				&& obj.Remove == Remove
				&& obj.Append.SequenceEqualWithNullCheck(Append);
		}

		/// <summary>
		/// GetHashCode
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode()
		{
			var hashCode = -106078415;
			hashCode = hashCode * -1521134295 + Description.GetHashCodeWithNullCheck();
			hashCode = hashCode * -1521134295 + Offset.GetHashCodeWithNullCheck();
			hashCode = hashCode * -1521134295 + Remove.GetHashCodeWithNullCheck();
			hashCode = hashCode * -1521134295 + Append.GetHashCodeWithNullCheck();
			return hashCode;
		}
	}
}
