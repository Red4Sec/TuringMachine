using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Collections
{
    public class WeightCollection<T> : ObservableCollection<T>, IRandomValue<T> where T : IWeight
    {
        private T[] _Steps = new T[0];

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
    }
}
