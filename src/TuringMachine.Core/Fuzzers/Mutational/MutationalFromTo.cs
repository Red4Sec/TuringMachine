using System;
using System.Diagnostics;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Fuzzers.Mutational
{
    [DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
    public class MutationalFromTo : FromToValue<byte>, IMutation, IRandomValue<byte>, IEquatable<MutationalFromTo>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MutationalFromTo() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public MutationalFromTo(byte value) : base(value) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="from">From</param>
        /// <param name="to">To</param>
        public MutationalFromTo(byte from, byte to) : base(from, to) { }

        /// <summary>
        /// Get
        /// </summary>
        /// <returns>Get random chunk</returns>
        public byte[] GetChunk(int size)
        {
            if (size <= 0) return new byte[0];

            var data = new byte[size];
            RandomHelper.Randomize(data, 0, size, this);
            return data;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MutationalFromTo o))
            {
                return false;
            }

            return Equals(o);
        }

        public bool Equals(IMutation obj)
        {
            if (!(obj is MutationalFromTo o))
            {
                return false;
            }

            return Equals(o);
        }

        public bool Equals(MutationalFromTo x) => Equals((FromToValue<byte>)x);

        public override int GetHashCode() => base.GetHashCode();
    }
}
