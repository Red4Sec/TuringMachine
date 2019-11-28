using TuringMachine.Core.Logs;

namespace TuringMachine.Core.Interfaces
{
	public interface IFuzzerStat
	{
		/// <summary>
		/// Tests
		/// </summary>
		int Tests { get; set; }

		/// <summary>
		/// Errors
		/// </summary>
		int Errors { get; set; }

		/// <summary>
		/// Crashes
		/// </summary>
		int Crashes { get; set; }

		/// <summary>
		/// Increment
		/// </summary>
		void Increment();

		/// <summary>
		/// Increment
		/// </summary>
		/// <param name="result">Result</param>
		void Increment(FuzzerError.EFuzzingErrorType result);
	}
}
