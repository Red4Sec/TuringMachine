using System;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Fuzzers.Mutational
{
    public interface IChunkFilter : IType, IWeight, IEquatable<IChunkFilter>
    {
        /// <summary>
        /// Apply the filter
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>Filtered data</returns>
        byte[] ApplyFilter(byte[] input);
    }
}
