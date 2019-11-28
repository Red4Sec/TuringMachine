using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using TuringMachine.Core.Collections;
using TuringMachine.Core.Converters;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Fuzzers.Patch;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Fuzzers.Mutational
{
    [DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
    public class MutationalChange : IEquatable<MutationalChange>, IWeight
    {
        /// <summary>
        /// Description
        /// </summary>
        [Description("Set the description")]
        [Category("4 - Info")]
        public string Description { get; set; }

        /// <summary>
        /// Weight for collision
        /// </summary>
        [Description("Set the weight")]
        [Category("3 - Select")]
        public ushort Weight { get; set; }

        /// <summary>
        /// Byte to append: 0x41='A'
        /// </summary>
        [Category("1 - Append")]
        [Description("Set the kind of bytes for add")]
        [JsonConverter(typeof(IMutationConverter))]
        public IMutation Append { get; set; }

        /// <summary>
        /// Append x bytes: 5000
        /// </summary>
        [Category("1 - Append")]
        [Description("Set the append length value")]
        [JsonConverter(typeof(IGetValueConverter))]
        public IGetValue<ushort> AppendIterations { get; set; }

        /// <summary>
        /// Remove x bytes: 1
        /// </summary>
        [Category("2 - Remove")]
        [Description("Set the remove length value")]
        [JsonConverter(typeof(IGetValueConverter))]
        public IGetValue<ushort> RemoveLength { get; set; }

        /// <summary>
        /// Filter chunk
        /// </summary>
        [Category("2 - Filter")]
        [Description("Set a filter for the chunk")]
        public WeightCollection<IChunkFilter> Filter { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MutationalChange()
        {
            //Append = new MutationalFromTo(byte.MinValue, byte.MaxValue);
            //RemoveLength = new FromToValue<ushort>(1);
            //AppendIterations = new FromToValue<ushort>(1);
            Description = "Unnamed";
            Weight = 1;
        }

        /// <summary>
        /// Do the fuzz process
        /// </summary>
        /// <param name="realOffset">Real offset</param>
        public PatchChange Process(long realOffset)
        {
            // Removes

            var remove = RemoveLength != null ? RemoveLength.Get() : (ushort)0;

            // Appends

            var size = AppendIterations != null ? AppendIterations.Get() : 0;
            byte[] data = size > 0 && Append != null ? Append.GetChunk(size) : null;

            if (data == null || data.Length == 0)
            {
                return remove > 0 ? new PatchChange(Description, realOffset, remove) : null;
            }

            // Apply the filter

            if (Filter != null)
            {
                var filter = Filter.Get();
                if (filter != null)
                {
                    data = filter.ApplyFilter(data);
                }
            }

            return new PatchChange(Description, realOffset, remove, data);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MutationalChange o))
            {
                return false;
            }

            return Equals(o);
        }

        public bool Equals(MutationalChange obj)
        {
            if (obj == null) return false;

            return obj.Weight == Weight
                && obj.Description == Description
                && obj.RemoveLength.EqualWithNullCheck(RemoveLength)
                && obj.AppendIterations.EqualWithNullCheck(AppendIterations)
                && obj.Append.EqualWithNullCheck(Append)
                && obj.Filter.EqualWithNullCheck(Filter);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = -1435047217;
            hashCode = hashCode * -1521134295 + Description.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + Weight.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + Append.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + AppendIterations.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + RemoveLength.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + Filter.GetHashCodeWithNullCheck();
            return hashCode;
        }
    }
}
