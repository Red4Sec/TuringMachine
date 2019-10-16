using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core
{
    [DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
    public class FromToValue<T> :
        IEquatable<FromToValue<T>>, IGetValue<T>, IType
        where T : IComparable, IEquatable<T>, IComparable<T>
    {
        T _From, _To;

        private readonly Type _Type;

        private readonly static Type TypeByte = typeof(byte);
        private readonly static Type TypeUInt16 = typeof(ushort);
        private readonly static Type TypeInt32 = typeof(int);
        private readonly static Type TypeInt64 = typeof(long);
        private readonly static Type TypeDouble = typeof(double);

        /// <summary>
        /// Class name
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [JsonProperty(Order = 0)]
        public string Type => "From-To";

        /// <summary>
        /// Excludes
        /// </summary>
        public List<T> Excludes { get; set; }

        /// <summary>
        /// From
        /// </summary>
        [Category("Values")]
        [JsonProperty(Order = 1)]
        public T From
        {
            get => _From;
            set
            {
                _From = value;
                AreSame = From.CompareTo(To) == 0;
            }
        }

        /// <summary>
        /// To
        /// </summary>
        [Category("Values")]
        [JsonProperty(Order = 2)]
        public T To
        {
            get => _To;
            set
            {
                _To = value;
                AreSame = From.CompareTo(To) == 0;
            }
        }

        /// <summary>
        /// Return if are the same
        /// </summary>
        [JsonIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        public bool AreSame { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Equal values</param>
        public FromToValue(T value) : this(value, value) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="from">From value</param>
        /// <param name="to">To value</param>
        public FromToValue(T from, T to) : this()
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FromToValue()
        {
            Excludes = new List<T>();
            To = _From = default;
            _Type = typeof(T);
        }

        /// <summary>
        /// Return if are between from an To
        /// </summary>
        /// <param name="o">Object</param>
        public bool ItsValid(T o)
        {
            if (From.CompareTo(o) <= 0 && To.CompareTo(o) >= 0)
            {
                if (Excludes.Contains(o))
                {
                    return false;
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Get next item
        /// </summary>
        public T Get()
        {
            if (AreSame)
            {
                return From;
            }

            T ret = default;

            do
            {
                if (_Type == TypeByte)
                {
                    ret = (T)Convert.ChangeType(RandomHelper.GetRandom(Convert.ToByte(From), Convert.ToByte(To)), _Type);
                }
                else if (_Type == TypeUInt16)
                {
                    ret = (T)Convert.ChangeType(RandomHelper.GetRandom(Convert.ToUInt16(From), Convert.ToUInt16(To)), _Type);
                }
                else if (_Type == TypeInt32)
                {
                    ret = (T)Convert.ChangeType(RandomHelper.GetRandom(Convert.ToInt32(From), Convert.ToInt32(To)), _Type);
                }
                else if (_Type == TypeInt64)
                {
                    ret = (T)Convert.ChangeType(RandomHelper.GetRandom(Convert.ToInt64(From), Convert.ToInt64(To)), _Type);
                }
                else if (_Type == TypeDouble)
                {
                    ret = (T)Convert.ChangeType(RandomHelper.GetRandom(Convert.ToDouble(From), Convert.ToDouble(To)), _Type);
                }
            }
            while (Excludes.Contains(ret));

            return ret;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FromToValue<T> o))
            {
                return false;
            }

            return Equals(o);
        }

        public bool Equals(IGetValue<T> obj)
        {
            if (!(obj is FromToValue<T> o))
            {
                return false;
            }

            return Equals(o);
        }

        public bool Equals(FromToValue<T> obj)
        {
            if (obj == null) return false;

            return obj.Type == Type
                && obj.From.Equals(From)
                && obj.To.Equals(To)
                && obj.Excludes.SequenceEqual(Excludes);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = -1596653049;
            hashCode = hashCode * -1521134295 + Type.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + From.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + To.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + Excludes.GetHashCodeWithNullCheck();
            return hashCode;
        }
    }
}
