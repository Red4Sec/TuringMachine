using System.IO;
using TuringMachine.Core;

namespace StackOverflowException.Payload
{
    class Program
    {
        static void Main()
        {
            Fuzzer.Run(Fuzz, new FuzzerRunArgs()
            {
                StoreCurrent = true,
                Supervisor = FuzzerRunArgs.SupervisorType.RegularSupervisor
            });
        }

        /// <summary>
        /// Fuzz logic
        /// </summary>
        /// <param name="stream">Stream</param>
        private static void Fuzz(Stream stream)
        {
            var data = new byte[2];
            var read = stream.Read(data, 0, data.Length);

            if (read == data.Length && data[0] == 0xFF && data[1] == 0xFF)
            {
                ForceStackOverflowException();
            }
        }

        /// <summary>
        /// Force StackOverflowException
        /// </summary>
        public static void ForceStackOverflowException() => ForceStackOverflowException();
    }
}
