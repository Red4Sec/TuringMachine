using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuringMachine.Core.Detectors;
using TuringMachine.Core.Fuzzers;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Logs;

namespace TuringMachine.Core
{
    /// <summary>
    /// Compatible with the schema of https://github.com/Metalnem/sharpfuzz/blob/master/src/SharpFuzz/Fuzzer.cs
    /// </summary>
    public class Fuzzer
    {
        #region Public properties

        /// <summary>
        /// Fuzzer client
        /// </summary>
        public static readonly FuzzerClient Client = new FuzzerClient();

        #endregion

        private const int StackOverflowExceptionCode = unchecked((int)0xC00000FD);

        /// <summary>
        /// Run fuzzer
        /// </summary>
        /// <param name="taskCount">Task Count</param>
        /// <param name="action">Action</param>
        /// <param name="args">Arguments</param>
        public static void Run(int taskCount, Action<Stream, int> action, FuzzerRunArgs args = null)
        {
            CoverageHelper.CreateCoverageListener();

            if (!Client.IsStarted)
            {
                // If you want other connection you must call this method before Run

                Client.Start(CommandLineOptions.Parse().GetConnection());
            }

            var tasks = new Task[taskCount];

            for (var x = 0; x < taskCount; x++)
            {
                var arg = new FuzzerRunArgs() { TaskId = x };

                if (args != null)
                {
                    arg.StoreCurrent = args.StoreCurrent;
                    arg.OnLog = args.OnLog;
                }

                tasks[x] = new Task(() => Run((s) => action(s, arg.TaskId), arg));
                tasks[x].Start();
            }

            Task.WaitAll(tasks);
        }

        /// <summary>
        /// Run fuzzer
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="args">Arguments</param>
        public static void Run(Action<Stream> action, FuzzerRunArgs args = null)
        {
            CoverageHelper.CreateCoverageListener();

            if (action == null)
            {
                throw new NullReferenceException(nameof(action));
            }

            // Check supervisor

            Mutex mutex;
            var supervisor = FuzzerRunArgs.SupervisorType.None;

            if (args != null && args.Supervisor != FuzzerRunArgs.SupervisorType.None)
            {
                // Check if is the listener or the task with mutex

                mutex = new Mutex(false, "TuringMachine.Supervisor." + Client.PublicName, out var isNew);

                if (!isNew)
                {
                    Client.PublicName += $".{Process.GetCurrentProcess().Id}:{args.TaskId}";
                }
                else
                {
                    Client.PublicName += ".Supervisor";
                    supervisor = args.Supervisor;
                }
            }
            else
            {
                mutex = null;
            }

            if (!Client.IsStarted)
            {
                // If you want other connection you must call this method before Run

                Client.Start(CommandLineOptions.Parse().GetConnection());
            }

            // Send current files

            Client.SendCurrentFiles(new OperationCanceledException(), null, true);

            // Fuzz

            var cancel = new CancelEventArgs();
            var handler = new ConsoleCancelEventHandler((o, s) =>
            {
                cancel.Cancel = true;
                s.Cancel = true;
            });
            Console.CancelKeyPress += handler;

            // Ensure data

            while (Client.GetInput() == null || Client.GetConfig() == null)
            {
                Thread.Sleep(50);
            }

            switch (supervisor)
            {
                case FuzzerRunArgs.SupervisorType.RegularSupervisor:
                    {
                        var pi = new ProcessStartInfoEx()
                        {
                            FileName = "dotnet",
                            Arguments = string.Join(" ", Environment.GetCommandLineArgs().Select(u => u)),
                            WindowStyle = ProcessWindowStyle.Normal,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = false,
                        };

                        while (!cancel.Cancel)
                        {
                            using (var pr = new ProcessEx(pi))
                            {
                                pr.WaitForExit();

                                Exception exception;

                                switch (pr.ExitCode)
                                {
                                    case StackOverflowExceptionCode:
                                        {
                                            exception = new StackOverflowException($"Unhandled exception: {pr.ExitCode}");
                                            break;
                                        }
                                    default:
                                        {
                                            exception = new InvalidProgramException($"Unhandled exception: {pr.ExitCode}");
                                            break;
                                        }
                                }

                                if (Client.SendCurrentFiles(exception, pr.Output, true) == 0)
                                {
                                    Client.SendLog(new FuzzerLog()
                                    {
                                        Coverage = CoverageHelper.CurrentCoverage,
                                        InputId = Guid.Empty,
                                        ConfigId = Guid.Empty,
                                    });
                                }
                            }
                        }
                        break;
                    }
                case FuzzerRunArgs.SupervisorType.None:
                    {
                        while (!cancel.Cancel)
                        {
                            var input = Client.GetInput();
                            var config = Client.GetConfig();

                            string currentStreamPath = null;
                            FileStream storeCurrentStream = null;

                            if (args?.StoreCurrent == true)
                            {
                                // Free current stream

                                currentStreamPath = $"{input.Id}.{config.Id}.{Process.GetCurrentProcess().Id}.{args.TaskId}.current";
                                storeCurrentStream = new FileStream(currentStreamPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                            }

                            using (var stream = new FuzzingStream(config, input, storeCurrentStream)
                            {
                                ExtraLogInformation = "TaskId: " + args.TaskId
                            })
                            {
                                var log = Client.Execute(action, stream);

                                if (log != null)
                                {
                                    Client.SendLog(log);
                                    args?.OnLog?.Invoke(log, cancel);
                                }
                            }

                            if (storeCurrentStream != null)
                            {
                                // Delete current stream

                                storeCurrentStream.Close();
                                storeCurrentStream.Dispose();
                                File.Delete(currentStreamPath);
                            }
                        }
                        break;
                    }
            }

            Console.CancelKeyPress -= handler;
            mutex?.Dispose();
        }
    }
}
