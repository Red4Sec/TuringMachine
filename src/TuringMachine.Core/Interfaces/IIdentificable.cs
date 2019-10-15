using System;

namespace TuringMachine.Core.Interfaces
{
    public interface IIdentificable
    {
        /// <summary>
        /// Unique id
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        string Description { get; set; }
    }
}
