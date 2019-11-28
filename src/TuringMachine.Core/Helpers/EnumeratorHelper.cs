using System.Collections.Generic;
using System.Linq;

namespace TuringMachine.Core.Helpers
{
	public static class EnumeratorHelper
	{
		/// <summary>
		/// To arryy
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="index">Index</param>
		/// <param name="count">Count</param>
		/// <returns>Array</returns>
		public static T[] ToArray<T>(this IEnumerable<T> input, int index = -1, int count = -1)
		{
			if (index > 0) input = input.Skip(index);
			if (count >= 0) input = input.Take(count);

			return Enumerable.ToArray(input);
		}
	}
}
