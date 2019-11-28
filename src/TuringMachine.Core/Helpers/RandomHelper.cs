using System;
using System.Collections.Generic;
using System.Linq;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Helpers
{
	public static class RandomHelper
	{
		/// <summary>
		/// https://stackoverflow.com/questions/3049467/is-c-sharp-random-number-generator-thread-safe
		/// </summary>
		[ThreadStatic]
		private static Random _rand;

		/// <summary>
		/// Thread safe Random
		/// </summary>
		private static Random Rand
		{
			get
			{
				if (_rand == null) _rand = new Random();
				return _rand;
			}
		}

		/// <summary>
		/// Fill with random bytes
		/// </summary>
		/// <param name="data">Data</param>
		public static void FillWithRandomBytes(byte[] data)
		{
			Rand.NextBytes(data);
		}

		/// <summary>
		/// Return next int
		/// </summary>
		/// <param name="from">From byte</param>
		/// <param name="to">To byte</param>
		public static int GetRandom(int from, int to)
		{
			if (from == to)
			{
				return to;
			}

			return Rand.Next(from, to + 1);
		}

		/// <summary>
		/// Return next int
		/// </summary>
		/// <param name="from">From byte</param>
		/// <param name="to">To byte</param>
		public static double GetRandom(double from, double to)
		{
			if (from == to)
			{
				return to;
			}

			return Rand.NextDouble() * (to - from) + from;
		}

		/// <summary>
		/// Return next long
		/// </summary>
		/// <param name="from">From byte</param>
		/// <param name="to">To byte</param>
		public static long GetRandom(long from, long to)
		{
			if (from == to)
			{
				return to;
			}

			var buf = new byte[8];
			Rand.NextBytes(buf);
			var longRand = BitConverter.ToInt64(buf, 0);

			return Math.Abs(longRand % ((to + 1) - from)) + from;
		}

		/// <summary>
		/// Return random percent is checked
		/// </summary>
		/// <param name="percent">Percent</param>
		public static bool IsRandomPercentOk(double percent)
		{
			if (percent >= 100)
			{
				return true;
			}
			else if (percent <= 0)
			{
				return false;
			}

			// max 2 decimal places
			return GetRandom(0, 10000) <= (percent * 100D);
		}

		/// <summary>
		/// Randomize array
		/// </summary>
		/// <param name="buffer">Buffer</param>
		/// <param name="index">Index</param>
		/// <param name="length">Count</param>
		/// <param name="get">Getter</param>
		public static void Randomize<T>(T[] buffer, int index, int length, IRandomValue<T> get)
		{
			for (; length > 0; index++)
			{
				buffer[index] = get.Get();
				length--;
			}
		}

		/// <summary>
		/// Get random elements
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="inputs">Collection</param>
		public static T GetRandom<T>(IList<T> inputs)
		{
			var count = inputs.Count;

			if (count <= 0)
			{
				return default;
			}

			return inputs[Rand.Next(count)];
		}

		/// <summary>
		/// Get random elements
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="inputs">Collection</param>
		public static T GetRandom<T>(ICollection<T> inputs)
		{
			var count = inputs.Count;

			if (count <= 0)
			{
				return default;
			}

			return inputs.Skip(Rand.Next(count)).FirstOrDefault();
		}
	}
}
