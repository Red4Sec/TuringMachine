using System;
using System.ComponentModel;
using System.Diagnostics;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Fuzzers.Patch;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Interfaces
{
    [DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
    public abstract class FuzzingConfigBase : IType, IGetPatch, IIdentificable, IEquatable<FuzzingConfigBase>
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
        protected FuzzingConfigBase(string type)
        {
            Type = type;
        }

        public abstract PatchChange Get(FuzzingStream stream);

        public abstract void InitFor(FuzzingStream stream);

        public virtual bool Equals(FuzzingConfigBase obj)
        {
            if (obj == null) return false;

            return obj.Type == Type
                && obj.Id == Id
                && obj.Description == Description;
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
            var hashCode = -106078415;
            hashCode = hashCode * -1521134295 + Type.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + Description.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + Id.GetHashCodeWithNullCheck();
            return hashCode;
        }
    }
}
