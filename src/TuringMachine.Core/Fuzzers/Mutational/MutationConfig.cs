using System;
using System.Collections.Generic;
using System.ComponentModel;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Fuzzers.Patch;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Fuzzers.Mutational
{
	public class MutationConfig : FuzzingConfigBase, IEquatable<MutationConfig>
	{
		/// <summary>
		/// Mutations
		/// </summary>
		[Category("1 - Collection")]
		public List<MutationalEntry> Mutations { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public MutationConfig() : base("Mutational")
		{
			Mutations = new List<MutationalEntry>();
			Description = "Unnamed";
		}

		/// <summary>
		/// Init for
		/// </summary>
		/// <param name="stream">Stream</param>
		public override void InitFor(FuzzingStream stream)
		{
			if (Mutations == null) return;

			int x = 0;
			foreach (var cond in Mutations)
			{
				cond.InitFor(stream, x);
				x++;
			}
		}

		/// <summary>
		/// Get next mutation
		/// </summary>
		/// <param name="stream">Stream</param>
		public override PatchChange Get(FuzzingStream stream)
		{
			if (Mutations == null) return null;

			var offset = stream.Position;

			// Fuzzer

			int x = 0;
			foreach (var cond in Mutations)
			{
				var change = cond.Get(stream, offset, x);

				if (change != null)
				{
					return change.Process(offset);
				}

				x++;
			}

			return null;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MutationConfig o))
			{
				return false;
			}

			return Equals(o);
		}

		public override bool Equals(FuzzingConfigBase obj)
		{
			if (!(obj is MutationConfig o))
			{
				return false;
			}

			return Equals(o);
		}

		public bool Equals(MutationConfig obj)
		{
			if (obj == null) return false;

			return base.Equals(obj)
				&& obj.Mutations.SequenceEqualWithNullCheck(obj.Mutations);
		}

		/// <summary>
		/// GetHashCode
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode()
		{
			var hashCode = -106078415;
			hashCode = hashCode * -1521134295 + base.GetHashCode();
			hashCode = hashCode * -1521134295 + Mutations.GetHashCodeWithNullCheck();
			return hashCode;
		}
	}
}
