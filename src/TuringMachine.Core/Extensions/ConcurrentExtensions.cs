using System.Collections.Concurrent;

namespace TuringMachine.Core.Extensions
{
	public static class ConcurrentExtensions
	{
		/// <summary>
		/// Clear concurrent bag
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="bag">Bag</param>
		public static void Clear<T>(this ConcurrentBag<T> bag)
		{
			while (!bag.IsEmpty)
			{
				bag.TryTake(out var _);
			}
		}
	}
}
