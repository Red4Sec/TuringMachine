namespace TuringMachine.Core.Interfaces
{
    public interface IGetValue<T> : IRandomValue<T>
    {
        /// <summary>
        /// Check if value its valid
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>True if is valid</returns>
        bool ItsValid(T value);
    }
}
