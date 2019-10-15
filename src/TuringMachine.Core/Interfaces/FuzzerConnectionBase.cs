using System.Diagnostics;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Interfaces
{
    [DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
    public class FuzzerConnectionBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        internal FuzzerConnectionBase() { }
    }
}
