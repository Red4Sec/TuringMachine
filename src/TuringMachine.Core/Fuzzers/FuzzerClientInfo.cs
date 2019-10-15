using System;
using System.Threading;
using TuringMachine.Core.Interfaces;
using TuringMachine.Core.Logs;

namespace TuringMachine.Core.Fuzzers
{
    public class FuzzerClientInfo : IIdentificable
    {
        private int Logs = 0;
        private DateTime _lastSpeedCheck = DateTime.UtcNow;
        private double _coverage = 0;
        private double _lastSpeed = 0;

        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Internal Name
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// Coverage
        /// </summary>
        public double Coverage { get => _coverage; }

        /// <summary>
        /// Speed
        /// </summary>
        public double Speed
        {
            get
            {
                var time = DateTime.UtcNow;

                if ((time - _lastSpeedCheck).TotalMilliseconds >= 500)
                {
                    var factor = (time - _lastSpeedCheck).TotalMilliseconds / 1000D;
                    var current = Interlocked.Exchange(ref Logs, 0);
                    _lastSpeed = Math.Round(current / factor, 2);
                    _lastSpeedCheck = time;
                }

                return _lastSpeed;
            }
        }

        /// <summary>
        /// Update client
        /// </summary>
        /// <param name="log">Log</param>
        public void Update(FuzzerLog log)
        {
            Interlocked.Increment(ref Logs);
            _coverage = log.Coverage;
        }
    }
}
