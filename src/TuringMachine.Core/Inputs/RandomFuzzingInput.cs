using System;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Inputs
{
    public class RandomFuzzingInput : FuzzingInputBase, IEquatable<RandomFuzzingInput>
    {
        /// <summary>
        /// Length
        /// </summary>
        public FromToValue<long> Length { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public RandomFuzzingInput() : base("Random") { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        public RandomFuzzingInput(FromToValue<long> length) : base("Random")
        {
            Length = length;
        }

        /// <summary>
        /// Get random stream
        /// </summary>
        /// <returns>Random data</returns>
        public override byte[] GetStream()
        {
            var data = new byte[(int)Length.Get()];
            RandomHelper.FillWithRandomBytes(data);
            return data;
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public bool Equals(RandomFuzzingInput obj)
        {
            if (obj == null) return false;

            return base.Equals(obj)
                && obj.Length.Equals(Length);
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public override bool Equals(object obj)
        {
            if (obj is RandomFuzzingInput o)
            {
                return Equals(o);
            }

            return false;
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public override bool Equals(FuzzingInputBase obj)
        {
            if (obj is RandomFuzzingInput o)
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
            var hashCode = -1289872652;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Length.GetHashCodeWithNullCheck();
            return hashCode;
        }
    }
}
