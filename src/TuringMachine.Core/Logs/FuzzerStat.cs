using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Logs
{
	[DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
	public class FuzzerStat<T> : IEquatable<FuzzerStat<T>>, IFuzzerStat
	   where T : IIdentificable
	{
		private int _Tests, _Crashes, _Errors;

		/// <summary>
		/// Source
		/// </summary>
		[JsonIgnore]
		public T Source { get; private set; }

		/// <summary>
		/// Id
		/// </summary>
		public Guid Id => Source == null ? Guid.Empty : Source.Id;

		/// <summary>
		/// Description
		/// </summary>
		public string Description => Source?.Description;

		/// <summary>
		/// Count
		/// </summary>
		public int Tests
		{
			get => _Tests;
			set
			{
				if (_Tests == value)
				{
					return;
				}

				_Tests = value;
			}
		}

		/// <summary>
		/// Crashes
		/// </summary>
		public int Crashes
		{
			get => _Crashes;
			set
			{
				if (_Crashes == value)
				{
					return;
				}

				_Crashes = value;
			}
		}

		/// <summary>
		/// Fails
		/// </summary>
		public int Errors
		{
			get => _Errors;
			set
			{
				if (_Errors == value)
				{
					return;
				}

				_Errors = value;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source">Source</param>
		public FuzzerStat(T source)
		{
			Source = source;
		}

		/// <summary>
		/// Increment
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Increment() => Tests++;

		/// <summary>
		/// Increment
		/// </summary>
		/// <param name="result">Result</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Increment(FuzzerError.EFuzzingErrorType result)
		{
			Tests++;

			switch (result)
			{
				case FuzzerError.EFuzzingErrorType.Crash: Crashes++; break;
				case FuzzerError.EFuzzingErrorType.Fail: Errors++; break;
			}
		}

		/// <summary>
		/// Reset stats
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			Tests = 0;
			Crashes = 0;
			Errors = 0;
		}

		/// <summary>
		/// Equals
		/// </summary>
		/// <param name="obj">Object</param>
		/// <returns>Return true if are equals</returns>
		public bool Equals(FuzzerStat<T> obj)
		{
			if (obj == null) return false;

			return Source.Equals(obj.Source)
				&& Tests == obj.Tests
				&& Crashes == obj.Crashes
				&& Errors == obj.Errors;
		}

		/// <summary>
		/// Equals
		/// </summary>
		/// <param name="obj">Object</param>
		/// <returns>Return true if are equals</returns>
		public override bool Equals(object obj)
		{
			if (obj is FuzzerStat<T> o)
			{
				return Equals(o);
			}

			return false;
		}

		/// <summary>
		/// GetHashCode
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode()
		{
			var hashCode = -869128546;
			hashCode = hashCode * -1521134295 + Source.GetHashCodeWithNullCheck();
			hashCode = hashCode * -1521134295 + Description.GetHashCodeWithNullCheck();
			hashCode = hashCode * -1521134295 + Tests.GetHashCodeWithNullCheck();
			hashCode = hashCode * -1521134295 + Crashes.GetHashCodeWithNullCheck();
			hashCode = hashCode * -1521134295 + Errors.GetHashCodeWithNullCheck();
			return hashCode;
		}
	}
}
