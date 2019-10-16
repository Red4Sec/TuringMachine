using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;
using TuringMachine.Core.Converters;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Fuzzers.Mutational.Filters
{
    [DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
    public class MixCaseFilter : IChunkFilter
    {
        [DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
        class MixCase
        {
            /// <summary>
            /// Values
            /// </summary>
            public byte UpperCase, LowerCase;

            /// <summary>
            /// Change the case
            /// </summary>
            /// <param name="input">Input</param>
            /// <returns>Return the different one</returns>
            public byte ChangeCase(byte input)
            {
                return input == UpperCase ? LowerCase : UpperCase;
            }
        }

        public enum MixCaseType : byte
        {
            ChangeCase = 0x00,
            ToUpperCase = 0x01,
            ToLowerCase = 0x02,
        }

        private readonly static MixCase[] _cases = new MixCase[byte.MaxValue];

        /// <summary>
        /// Weight for collision
        /// </summary>
        [Description("Set the weight")]
        [Category("3 - Select")]
        public ushort Weight { get; set; }

        /// <summary>
        /// Static constructor
        /// </summary>
        static MixCaseFilter()
        {
            for (int x = 0; x < byte.MaxValue; x++)
            {
                char upperCase = (char)x;
                char lowerCase = char.ToLowerInvariant(upperCase);

                if (lowerCase == upperCase)
                {
                    upperCase = char.ToUpperInvariant(upperCase);

                    if (lowerCase == upperCase) continue;
                }

                _cases[x] = new MixCase()
                {
                    UpperCase = (byte)upperCase,
                    LowerCase = (byte)lowerCase
                };
            }
        }

        /// <summary>
        /// Mix cases
        /// </summary>
        public string Type => "MixCase";

        /// <summary>
        /// Filter Percent
        /// </summary>
        [Category("1 - Condition")]
        [JsonConverter(typeof(IGetValueConverter))]
        public IGetValue<double> FilterPercent { get; set; }

        /// <summary>
        /// Mix type
        /// </summary>
        public MixCaseType MixType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MixCaseFilter()
        {
            Weight = 1;
            MixType = MixCaseType.ChangeCase;
        }

        /// <summary>
        /// Apply filter
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>Filtered input</returns>
        public byte[] ApplyFilter(byte[] input)
        {
            switch (MixType)
            {
                case MixCaseType.ToUpperCase: return ToUpperCase(input, FilterPercent);
                case MixCaseType.ToLowerCase: return ToLowerCase(input, FilterPercent);
                case MixCaseType.ChangeCase: return ChangeCase(input, FilterPercent);
            }

            return input;
        }

        /// <summary>
        /// Change case
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="filter">Filter</param>
        /// <returns>Changed case</returns>
        public static byte[] ChangeCase(byte[] input, IGetValue<double> filter)
        {
            for (int x = input.Length - 1; x >= 0; x--)
            {
                var entry = _cases[input[x]];

                if (entry != null && (filter == null || RandomHelper.IsRandomPercentOk(filter.Get())))
                {
                    input[x] = entry.ChangeCase(input[x]);
                }
            }

            return input;
        }

        /// <summary>
        /// To uppercase
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="filter">Filter</param>
        /// <returns>Uppercase input</returns>
        public static byte[] ToUpperCase(byte[] input, IGetValue<double> filter)
        {
            for (int x = input.Length - 1; x >= 0; x--)
            {
                var entry = _cases[input[x]];

                if (entry != null && (filter == null || RandomHelper.IsRandomPercentOk(filter.Get())))
                {
                    input[x] = entry.UpperCase;
                }
            }

            return input;
        }

        /// <summary>
        /// To lowercase
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="filter">Filter</param>
        /// <returns>Lowercase input</returns>
        public static byte[] ToLowerCase(byte[] input, IGetValue<double> filter)
        {
            for (int x = input.Length - 1; x >= 0; x--)
            {
                var entry = _cases[input[x]];

                if (entry != null && (filter == null || RandomHelper.IsRandomPercentOk(filter.Get())))
                {
                    input[x] = entry.LowerCase;
                }
            }

            return input;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MixCaseFilter o))
            {
                return false;
            }

            return Equals(o);
        }

        public bool Equals(IChunkFilter obj)
        {
            if (!(obj is MixCaseFilter o))
            {
                return false;
            }

            return Equals(o);
        }

        public bool Equals(MixCaseFilter obj)
        {
            if (obj == null) return false;

            return obj.Weight == Weight
               && obj.MixType == MixType
               && obj.FilterPercent.Equals(FilterPercent);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = -1435047217;
            hashCode = hashCode * -1521134295 + Weight.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + MixType.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + FilterPercent.GetHashCodeWithNullCheck();
            return hashCode;
        }
    }
}
