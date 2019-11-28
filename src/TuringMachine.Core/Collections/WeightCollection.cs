using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Collections
{
    public class WeightCollection<T> : ObservableCollection<T>, IRandomValue<T>, IEquatable<WeightCollection<T>>
        where T : IWeight, IEquatable<T>
    {
        private T[] _Steps = new T[0];

        /// <summary>
        /// Constructor
        /// </summary>
        public WeightCollection() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entries">Entries</param>
        public WeightCollection(params T[] entries)
        {
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    Add(entry);
                }
            }
        }

        /// <summary>
        /// Recalculate weight list
        /// </summary>
        /// <param name="e">Arguments</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            // Memory protection

            var count = Math.Min(1_000, this.Sum(a => a.Weight));
            _Steps = new T[count];

            if (count > 0)
            {
                var x = 0;
                foreach (var c in this)
                {
                    for (var w = 0; w < c.Weight && w < count; w++, x++)
                    {
                        _Steps[x] = c;
                    }
                }
            }
        }

        /// <summary>
        /// Get random T
        /// </summary>
        /// <returns>T</returns>
        public T Get() => RandomHelper.GetRandom(_Steps);

        public override bool Equals(object obj)
        {
            if (!(obj is WeightCollection<T> o))
            {
                return false;
            }

            return Equals(o);
        }

        public bool Equals(WeightCollection<T> obj)
        {
            if (obj == null) return false;

            return this.SequenceEqual(obj);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = -1435047217;
            hashCode = hashCode * -1521134295 + ((IEnumerable)this).GetHashCodeWithNullCheck();
            return hashCode;
        }
    }
}
