using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Interfaces
{
    [DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
    public abstract class FuzzingInputBase : IType, IIdentificable, IEquatable<FuzzingInputBase>
    {
        /// <summary>
        /// Id
        /// </summary>
        [Browsable(true)]
        [Category("1 - Info")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Description
        /// </summary>
        [Category("1 - Info")]
        public string Description { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [Category("1 - Info")]
        public string Type { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        protected FuzzingInputBase(string type)
        {
            Type = type;
        }

        /// <summary>
        /// Get Stream
        /// </summary>
        public abstract byte[] GetStream();

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public bool Equals(FuzzingInputBase obj)
        {
            if (obj == null) return false;

            return Type == obj.Type
                && Id.Equals(obj.Id)
                && Description == Description;
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public override abstract bool Equals(object obj);

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = 1636442590;
            hashCode = hashCode * -1521134295 + BitConverter.ToInt32(Id.ToByteArray(), 0);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
            return hashCode;
        }
    }
}
