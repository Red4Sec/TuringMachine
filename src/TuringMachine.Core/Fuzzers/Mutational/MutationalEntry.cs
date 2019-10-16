using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using TuringMachine.Core.Collections;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Fuzzers.Mutational
{
    [DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
    public class MutationalEntry : IEquatable<MutationalEntry>
    {
        /// <summary>
        /// Description
        /// </summary>
        [Category("3 - Info")]
        public string Description { get; set; }

        /// <summary>
        /// Valid offset
        /// </summary>
        [Category("1 - Condition")]
        public IGetValue<long> ValidOffset { get; set; }

        /// <summary>
        /// Changes
        /// </summary>
        [Category("2 - Collection")]
        public WeightCollection<MutationalChange> Changes { get; set; }

        /// <summary>
        /// Fuzz Percent
        /// </summary>
        [Category("1 - Condition")]
        public IGetValue<double> FuzzPercent { get; set; }

        /// <summary>
        /// Fuzz Percent Type
        /// </summary>
        [Category("1 - Condition")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EFuzzingPercentType FuzzPercentType { get; set; }

        /// <summary>
        /// MaxChanges
        /// </summary>
        [Category("1 - Condition")]
        public IGetValue<ushort> MaxChanges { get; set; }

        private class Step
        {
            public int MaxChanges = 0;
            public readonly List<long> FuzzIndex = new List<long>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MutationalEntry()
        {
            Description = "Unnamed";
            //FuzzPercent = new FromToValue<double>(100);
            //MaxChanges = new FromToValue<ushort>(0, 2);
            //ValidOffset = new FromToValue<long>(0, long.MaxValue);
            Changes = new WeightCollection<MutationalChange>();
        }

        /// <summary>
        /// Init for
        /// </summary>
        /// <param name="stream">Stream</param>
        internal void InitFor(FuzzingStream stream, int index)
        {
            var s = new Step
            {
                // Max changes
                MaxChanges = MaxChanges == null ? 1 : Math.Max(1, (int)MaxChanges.Get())
            };

            if (FuzzPercentType == EFuzzingPercentType.PeerStream)
            {
                // Fill indexes

                var length = stream.Length;
                var changes = Math.Min(s.MaxChanges, (long)(length * (FuzzPercent == null ? 100D : FuzzPercent.Get()) / 100.0));

                while (changes > s.FuzzIndex.Count)
                {
                    long value;

                    do
                    {
                        value = RandomHelper.GetRandom(0, length);
                    }
                    while (s.FuzzIndex.Contains(value) || ValidOffset?.ItsValid(value) == false);

                    s.FuzzIndex.Add(value);
                }

                s.FuzzIndex.Sort();
            }

            stream.Variables[index] = s;
        }

        /// <summary>
        /// Get next mutation change (if happend)
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="offset">Offset</param>
        /// <param name="index">Index</param>
        public MutationalChange Get(FuzzingStream stream, long offset, int index)
        {
            if (ValidOffset?.ItsValid(offset) == false)
            {
                return null;
            }

            switch (FuzzPercentType)
            {
                case EFuzzingPercentType.PeerByte:
                    {
                        // Check Max changes
                        if (MaxChanges != null &&
                            stream.Log.Length >= MaxChanges.Get())
                        {
                            return null;
                        }

                        // Check Percent
                        if (FuzzPercent != null)
                        {
                            var value = FuzzPercent.Get();

                            if (!RandomHelper.IsRandomPercentOk(value))
                            {
                                return null;
                            }
                        }

                        // Get Item
                        return Changes.Get();
                    }
                case EFuzzingPercentType.PeerStream:
                    {
                        // Check Max changes
                        var s = (Step)stream.Variables[index];
                        if (stream.Log.Length >= s.MaxChanges)
                        {
                            return null;
                        }

                        if (!s.FuzzIndex.Contains(offset))
                        {
                            return null;
                        }

                        // Get Item
                        return Changes.Get();
                    }
            }

            return null;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MutationalEntry o))
            {
                return false;
            }

            return Equals(o);
        }

        public bool Equals(MutationalEntry obj)
        {
            if (obj == null) return false;

            return obj.Description == Description
                && obj.FuzzPercentType == FuzzPercentType
                && obj.ValidOffset.EqualWithNullCheck(ValidOffset)
                && obj.FuzzPercent.EqualWithNullCheck(FuzzPercent)
                && obj.MaxChanges.EqualWithNullCheck(MaxChanges)
                && obj.Changes.EqualWithNullCheck(obj.Changes);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = 1043142361;
            hashCode = hashCode * -1521134295 + FuzzPercentType.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + Description.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + ValidOffset.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + FuzzPercent.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + MaxChanges.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + Changes.GetHashCodeWithNullCheck();
            return hashCode;
        }
    }
}
