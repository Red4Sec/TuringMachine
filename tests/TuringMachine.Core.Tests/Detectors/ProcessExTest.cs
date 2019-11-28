using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading;
using TuringMachine.Core.Detectors;

namespace TuringMachine.Core.Tests.Detectors
{
    [TestFixture]
    public class ProcessExTest
    {
        [Test]
        public void KillTest()
        {
            string file, args;

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                file = "bash";
                args = "-c \"sleep(5)\"";
            }
            else
            {
                file = "cmd";
                args = "/c sleep(5)";
            }

            using (var proc = new ProcessEx(new ProcessStartInfoEx()
            {
                FileName = file,
                Arguments = args
            }))
            {
                var pid = proc.ProcessId;

                Assert.IsNotNull(Process.GetProcessById(pid));

                proc.KillProcess();
                Thread.Sleep(200);
                Assert.IsTrue(proc.HasExited);

                Thread.Sleep(200);
                Assert.Catch<ArgumentException>(() =>
                {
                    var p = Process.GetProcessById(pid);
                    // In windows 'p' is != null but is Exited
                    if (p != null && p.HasExited) throw new ArgumentException();
                });
            }
        }

        [Test]
        public void OutputTest()
        {
            string file, args;

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                file = "bash";
                args = "-c \"date\"";
            }
            else
            {
                file = "cmd";
                args = "/c date";
            }

            using (var proc = new ProcessEx(new ProcessStartInfoEx()
            {
                FileName = file,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }))
            {
                var res = proc.WaitUntilOutput(DateTime.UtcNow.Year.ToString(), TimeSpan.FromSeconds(5));

                Assert.IsTrue(res);
                Assert.IsTrue(proc.Output.Contains(DateTime.UtcNow.Year.ToString()));
            }
        }
    }
}
