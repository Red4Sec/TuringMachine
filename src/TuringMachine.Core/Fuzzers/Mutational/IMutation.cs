using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Fuzzers.Mutational
{
    public interface IMutation : IType
    {
        byte[] GetChunk(int size);
    }
}
