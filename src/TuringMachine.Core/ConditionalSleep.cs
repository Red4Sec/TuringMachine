using System.Threading;

namespace TuringMachine.Core
{
    public class ConditionalSleep
    {
        /// <summary>
        /// Store the number of ticks
        /// </summary>
        private int _ticks = 0;

        /// <summary>
        /// Milliseconds Timeout
        /// </summary>
        public int MillisecondsTimeout { get; set; } = 1;

        /// <summary>
        /// Ticks
        /// </summary>
        public int Ticks { get; set; } = 50;

        /// <summary>
        /// Sleep if is needed
        /// </summary>
        public void Sleep()
        {
            if (Interlocked.Increment(ref _ticks) > Ticks)
            {
                Interlocked.Exchange(ref _ticks, 0);
                Thread.Sleep(MillisecondsTimeout);
            }
        }
    }
}
