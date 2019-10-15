using System;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Fuzzers
{
    public class FuzzerNamedPipeConnection : FuzzerConnectionBase, IEquatable<FuzzerNamedPipeConnection>
    {
        /// <summary>
        /// PipeName
        /// </summary>
        public string PipeName { get; set; } = "TuringMachine";

        /// <summary>
        /// Constructor
        /// </summary>
        public FuzzerNamedPipeConnection() : base() { }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public bool Equals(FuzzerNamedPipeConnection obj)
        {
            if (obj == null) return false;

            return PipeName == obj.PipeName;
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public override bool Equals(object obj)
        {
            if (obj is FuzzerNamedPipeConnection log)
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
            hashCode = hashCode * -1521134295 + PipeName.GetHashCode();
            return hashCode;
        }
    }
}
