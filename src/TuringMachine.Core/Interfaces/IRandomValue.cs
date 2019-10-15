namespace TuringMachine.Core.Interfaces
{
    public interface IRandomValue<T>
    {
        /// <summary>
        /// Get next value
        /// </summary>
        /// <returns>T</returns>
        T Get();
    }
}
