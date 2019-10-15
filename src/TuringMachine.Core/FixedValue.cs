using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using TuringMachine.Core.Converters;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core
{
    [JsonConverter(typeof(JsonFromToConverter))]
    [DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
    public class FixedValue<T> : IGetValue<T>, IEquatable<FixedValue<T>>
        where T : IComparable, IEquatable<T>, IComparable<T>
    {
        /// <summary>
        /// From
        /// </summary>
        [Category("Values")]
        public List<T> Allowed { get; set; }

        /// <summary>
        /// Class Name
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public string Type => "Fixed";

        /// <summary>
        /// Constructor
        /// </summary>
        public FixedValue()
        {
            Allowed = new List<T>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="values">Start values</param>
        public FixedValue(params T[] values) : this()
        {
            if (values != null)
            {
                Allowed.AddRange(values);
            }
        }

        /// <summary>
        /// Return if are between from an To
        /// </summary>
        /// <param name="o">Object</param>
        public bool ItsValid(T o) => Allowed.Contains(o);

        /// <summary>
        /// Get next item
        /// </summary>
        public T Get()
        {
            if (Allowed.Count <= 0)
            {
                return default;
            }

            return RandomHelper.GetRandom(Allowed);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FixedValue<T> o))
            {
                return false;
            }

            return Equals(o);
        }

        public bool Equals(FixedValue<T> obj)
        {
            if (obj == null) return false;

            return obj.Type == Type
                && obj.Allowed.SequenceEqual(Allowed);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = -972130872;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + Allowed.Sum(u => u.GetHashCode());
            return hashCode;
        }
    }
}
