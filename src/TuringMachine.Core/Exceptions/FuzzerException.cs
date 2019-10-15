using System;
using TuringMachine.Core.Logs;

namespace TuringMachine.Core.Exceptions
{
    public class FuzzerException : Exception
    {
        /// <summary>
        /// Error id
        /// </summary>
        public Guid ErrorId { get; }

        /// <summary>
        /// Zip
        /// </summary>
        public byte[] Zip { get; }

        /// <summary>
        /// Result
        /// </summary>
        public FuzzerError.EExplotationResult Result { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errorId">Error Id</param>
        /// <param name="zip">Zip</param>
        /// <param name="result">Result</param>
        public FuzzerException(Guid errorId, byte[] zip, FuzzerError.EExplotationResult result)
        {
            ErrorId = errorId;
            Result = result;
            Zip = zip;
        }
    }
}
