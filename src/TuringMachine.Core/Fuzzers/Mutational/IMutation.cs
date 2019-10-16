using System;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Fuzzers.Mutational
{
    public interface IMutation : IType, IEquatable<IMutation>
    {
        byte[] GetChunk(int size);
    }
}
