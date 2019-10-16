using System;
using System.Linq;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Inputs
{
    public class ManualFuzzingInput : FuzzingInputBase, IEquatable<ManualFuzzingInput>
    {
        /// <summary>
        /// Data
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ManualFuzzingInput() : base("Manual") { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">data</param>
        public ManualFuzzingInput(byte[] data) : this()
        {
            Data = data;
        }

        /// <summary>
        /// Get manual content
        /// </summary>
        /// <returns>Manual content</returns>
        public override byte[] GetStream() => Data;

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public bool Equals(ManualFuzzingInput obj)
        {
            if (obj == null) return false;

            return base.Equals(obj) 
                && ((Data == null && obj.Data == null) || Data.SequenceEqual(obj.Data));
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public override bool Equals(object obj)
        {
            if (obj is ManualFuzzingInput o)
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
            if (obj is ManualFuzzingInput o)
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
            hashCode = hashCode * -1521134295 + Data.GetHashCodeWithNullCheck();
            return hashCode;
        }
    }
}
