using System;
using System.IO;
using System.Net;
using System.Threading;
using TuringMachine.Core;
using TuringMachine.Core.Detectors;
using TuringMachine.Core.Extensions;

namespace VulnServer.Payload
{
    class Program
    {
        const int Port = 9000;
        const string Path = "C:\\Temporal\\vulnserver.exe";

        static void Main()
        {
            Fuzzer.Run(1, Fuzz);
        }

        private static void Fuzz(Stream obj, int taskId)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port + taskId);

            using (var detector = new WERDetector(new ProcessStartInfoEx()
            {
                FileName = Path,
                Arguments = endPoint.Port.ToString(),
                //RedirectStandardError = true,
                //RedirectStandardOutput = true,
                UseShellExecute = false
            }))
            {
                Thread.Sleep(100);

                //if (!detector.CreatedProcess[0].WaitUntilOutput("Waiting for client connections...", TimeSpan.FromSeconds(2)))
                //{
                //    return;
                //}

                try
                {
                    obj.ConnectoAndCopyTo(endPoint, TimeSpan.FromSeconds(3));
                }
                catch { }

                detector.ThrowIfCrashed(() => ItsAlive(endPoint));
            }
        }

        static bool ItsAlive(IPEndPoint endPoint)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    return stream.ConnectoAndCopyTo(endPoint, TimeSpan.FromSeconds(2));
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
