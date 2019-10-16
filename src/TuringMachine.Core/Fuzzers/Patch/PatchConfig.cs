using System;
using System.Collections.Generic;
using System.ComponentModel;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Fuzzers.Patch
{
    public class PatchConfig : FuzzingConfigBase, IEquatable<PatchConfig>
    {
        /// <summary>
        /// Mutations
        /// </summary>
        [Category("1 - Collection")]
        public IList<PatchChange> Changes { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PatchConfig() : base("Patch")
        {
            Changes = new List<PatchChange>();
            Description = "Unnamed";
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="entries">Changes</param>
        public PatchConfig(string description, params PatchChange[] entries) : this()
        {
            Description = description;

            foreach(var entry in entries)
            {
                Changes.Add(entry);
            }
        }

        /// <summary>
        /// Get fixed patch
        /// </summary>
        /// <param name="stream">Stream</param>
        public override PatchChange Get(FuzzingStream stream)
        {
            var offset = stream.Position;

            foreach (var p in Changes)
            {
                if (p.Offset == offset)
                {
                    return p;
                }
            }

            return null;
        }

        public override void InitFor(FuzzingStream stream) { }

        public override bool Equals(object obj)
        {
            if (!(obj is PatchConfig o))
            {
                return false;
            }

            return Equals(o);
        }

        public override bool Equals(FuzzingConfigBase obj)
        {
            if (!(obj is PatchConfig o))
            {
                return false;
            }

            return Equals(o);
        }

        public bool Equals(PatchConfig obj)
        {
            if (obj == null) return false;

            return base.Equals(obj)
                && obj.Changes.SequenceEqualWithNullCheck(obj.Changes);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = -106078415;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Changes.GetHashCodeWithNullCheck();
            return hashCode;
        }
    }
}
