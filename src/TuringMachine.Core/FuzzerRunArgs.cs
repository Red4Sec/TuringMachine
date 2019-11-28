using System;
using System.ComponentModel;
using System.Diagnostics;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Logs;

namespace TuringMachine.Core
{
	[DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
	public class FuzzerRunArgs
	{
		public enum SupervisorType : byte
		{
			/// <summary>
			/// Without isolation
			/// </summary>
			None = 0x00,

			/// <summary>
			/// Start regular supervisor
			/// </summary>
			RegularSupervisor = 0x01,
		};

		/// <summary>
		/// Task id
		/// </summary>
		internal int TaskId { get; set; } = 0;

		/// <summary>
		/// Store current stream
		/// </summary>
		public bool StoreCurrent { get; set; } = false;

		/// <summary>
		/// Allow to catch exceptions like `StackOverflowException` with the help of a supervisor
		/// Should require `StoreCurrent`
		/// </summary>
		public SupervisorType Supervisor { get; set; } = SupervisorType.None;

		/// <summary>
		/// On log
		/// </summary>
		public Action<FuzzerLog, CancelEventArgs> OnLog { get; set; } = null;
	}
}
