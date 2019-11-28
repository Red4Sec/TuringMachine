using TuringMachine.Core.Fuzzers.Patch;

namespace TuringMachine.Core.Interfaces
{
    public interface IGetPatch
    {
        /// <summary>
        /// Get Patch
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>PatchChange</returns>
        PatchChange Get(FuzzingStream stream);

        /// <summary>
        /// Init for this stream
        /// </summary>
        /// <param name="stream">Stream</param>
        void InitFor(FuzzingStream stream);
    }
}
