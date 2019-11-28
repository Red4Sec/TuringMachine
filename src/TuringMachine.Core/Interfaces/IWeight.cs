using System.ComponentModel;

namespace TuringMachine.Core.Interfaces
{
	public interface IWeight
	{
		/// <summary>
		/// Weight for collision
		/// </summary>
		[Description("Set the weight")]
		[Category("3 - Select")]
		ushort Weight { get; set; }
	}
}
