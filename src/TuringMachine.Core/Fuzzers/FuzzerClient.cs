using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TuringMachine.Core.Exceptions;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;
using TuringMachine.Core.Logs;

namespace TuringMachine.Core.Fuzzers
{
    public class FuzzerClient : IDisposable, IRandomValue<FuzzingConfigBase>, IRandomValue<FuzzingInputBase>
    {
        class FuzzerTask
        {
            public Exception Exception = null;
            public Action<Stream> Action = null;
            public FuzzingStream Stream = null;
        }

        #region Public properties

        /// <summary>
        /// Is started
        /// </summary>
        public bool IsStarted => _client != null;

        /// <summary>
        /// Execution timeout for tasks
        /// </summary>
        public TimeSpan ExecutionTimeOut { get; set; } = TimeSpan.FromSeconds(20);

        #endregion

        private Stream _client;
        private Task[] _tasks;
        private ConcurrentBag<FuzzerLog> _buffer = new ConcurrentBag<FuzzerLog>();
        private CancellationTokenSource _cancel;

        private FuzzingInputBase[] _inputs = new FuzzingInputBase[0];
        private FuzzingConfigBase[] _configurations = new FuzzingConfigBase[0];

        public EventHandler<FuzzingInputBase[]> OnReceiveInputs;
        public EventHandler<FuzzingConfigBase[]> OnReceiveConfigurations;
        public EventHandler<FuzzerLog> OnSendError;

        /// <summary>
        /// Public name
        /// </summary>
        public string PublicName { get; set; } = Assembly.GetEntryAssembly().GetName().Name;

        /// <summary>
        /// Start Pipe connection
        /// </summary>
        /// <param name="connection">Connection</param>
        public void Start(FuzzerConnectionBase connection)
        {
            if (IsStarted)
            {
                throw new Exception("Already started");
            }

            if (connection is FuzzerNamedPipeConnection pipe)
            {
                var client = new NamedPipeClientStream(".", pipe.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                client.Connect(10_000);
                _client = client;
            }
            else if (connection is FuzzerTcpConnection tcp)
            {
#pragma warning disable IDE0067 // Dispose objects before losing scope
                var client = new TcpClient();
#pragma warning restore IDE0067 // Dispose objects before losing scope
                client.Connect(tcp.EndPoint);
                _client = client.GetStream();
            }
            else
            {
                if (connection == null)
                {
                    throw new NullReferenceException(nameof(connection));
                }

                throw new ArgumentException(nameof(connection));
            }

            _cancel = new CancellationTokenSource();
            _tasks = new Task[]
            {
                new Task(() => UpdateFromStream(_client, _cancel.Token), _cancel.Token),
                new Task(() => FlushBuffer(_cancel.Token), _cancel.Token)
            };
            _tasks.All(u =>
            {
                u.Start();
                return true;
            });

            // Request Configs and inputs

            var send = new List<byte>(new byte[]
            {
                (byte)EFuzzerMessageType.GetAvailableConfigs,
                (byte)EFuzzerMessageType.GetAvailableInputs
            });

            if (!string.IsNullOrWhiteSpace(PublicName))
            {
                send.AddRange(GetMessage(EFuzzerMessageType.PushPublicName, PublicName));
            }

            lock (_client)
            {
                _client.Write(send.ToArray(), 0, send.Count);
                _client.Flush();
            }
        }

        /// <summary>
        /// Send current files
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="output">Output</param>
        /// <param name="deleteFile">Delete file</param>
        /// <returns>Sent logs</returns>
        public int SendCurrentFiles(Exception exception, string output, bool deleteFile = true)
        {
            var ret = 0;

            foreach (var file in Directory.GetFiles(".", "*.current", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    SendLog(FuzzerLog.FromCurrentFile(file, exception, output));
                    ret++;

                    if (deleteFile)
                    {
                        File.Delete(file);
                    }
                }
                catch { }
            }

            return ret;
        }

        /// <summary>
        /// Stop
        /// </summary>
        public void Stop()
        {
            _cancel?.Cancel();
            _buffer.Clear();
            try
            {
                _client.Write(new byte[] { (byte)EFuzzerMessageType.GoodByte }, 0, 1);
                _client.Flush();
            }
            catch { }
            try { _client?.Dispose(); } catch { }
            _tasks?.All(u =>
            {
                try { u.Dispose(); } catch { }
                return true;
            });

            _cancel?.Dispose();
            _tasks = null;
            _client = null;
            _cancel = null;
        }

        /// <summary>
        /// Get Random Input
        /// </summary>
        /// <returns>Input</returns>
        FuzzingInputBase IRandomValue<FuzzingInputBase>.Get() => GetInput();

        /// <summary>
        /// Get Random Input
        /// </summary>
        /// <returns>Input</returns>
        public FuzzingInputBase GetInput() => RandomHelper.GetRandom(_inputs);

        /// <summary>
        /// Get Random Config
        /// </summary>
        /// <returns>Config</returns>
        FuzzingConfigBase IRandomValue<FuzzingConfigBase>.Get() => GetConfig();

        /// <summary>
        /// Get Random Config
        /// </summary>
        /// <returns>Config</returns>
        public FuzzingConfigBase GetConfig() => RandomHelper.GetRandom(_configurations);

        /// <summary>
        /// Send log
        /// </summary>
        /// <param name="log">Log</param>
        public void SendLog(FuzzerLog log)
        {
            if (log == null)
            {
                return;
            }

            _buffer.Add(log);

            if (log.Error != null)
            {
                OnSendError?.Invoke(this, log);
            }
        }

        /// <summary>
        /// Flush buffer
        /// </summary>
        private void FlushBuffer(CancellationToken cancel)
        {
            var sleep = new ConditionalSleep();
            var logBuffer = new byte[1 + (16 * 2) + 2];
            logBuffer[0] = (byte)EFuzzerMessageType.PushLog;

            while (!cancel.IsCancellationRequested)
            {
                cancel.ThrowIfCancellationRequested();

                using (var ms = new MemoryStream())
                {
                    while (_buffer.TryTake(out var log))
                    {
                        // Serialization

                        if (log.Error != null)
                        {
                            var chunk = GetMessage(EFuzzerMessageType.PushLogWithError, log);
                            ms.Write(chunk, 0, chunk.Length);
                        }
                        else
                        {
                            // Faster than serialize (16b InputId + 16b ConfigId + 2b Coverage)

                            Array.Copy(log.InputId.ToByteArray(), 0, logBuffer, 1, 16);
                            Array.Copy(log.ConfigId.ToByteArray(), 0, logBuffer, 16 + 1, 16);
                            Array.Copy(BitHelper.GetBytes((ushort)(log.Coverage * 100)), 0, logBuffer, (16 * 2) + 1, 2);

                            ms.Write(logBuffer, 0, logBuffer.Length);
                        }
                    }

                    if (ms.Length > 0)
                    {
                        var chunk = ms.ToArray();

                        lock (_client)
                        {
                            _client.Write(chunk, 0, chunk.Length);
                            _client.Flush();
                        }
                    }

                    sleep.Sleep();
                }
            }
        }

        /// <summary>
        /// Get Message
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="msg">Message</param>
        /// <returns>Message</returns>
        internal static byte[] GetMessage(EFuzzerMessageType type, object msg)
        {
            var data = StreamHelper.WriteString(SerializationHelper.SerializeToJson(msg, false));
            var chunk = new byte[1 + data.Length];

            chunk[0] = (byte)type;
            Array.Copy(data, 0, chunk, 1, data.Length);

            return chunk;
        }

        /// <summary>
        /// Execute action
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="stream">Stream</param>
        /// <returns>Log</returns>
        public FuzzerLog Execute(Action<Stream> action, FuzzingStream stream)
        {
            var log = new FuzzerLog()
            {
                ConfigId = stream.ConfigId,
                InputId = stream.InputId
            };

            var task = new FuzzerTask()
            {
                Action = action,
                Stream = stream
            };

            try
            {
                var thread = new Thread(new ParameterizedThreadStart(AsyncTask))
                {
                    Priority = ThreadPriority.Normal,
                    Name = "Fuzzing Thread",
                    IsBackground = true
                };

                thread.Start(task);

                if (!thread.Join(ExecutionTimeOut))
                {
                    try { thread.Abort(); } catch { }
                    throw new TimeoutException(thread.ThreadState.ToString());
                }
                else
                {
                    if (task.Exception != null)
                    {
                        throw task.Exception;
                    }
                }
            }
            catch (Exception e)
            {
                var errorMsg = e.ToString();
                if (!string.IsNullOrEmpty(stream.ExtraLogInformation))
                {
                    errorMsg += "\nExtraInformation: " + stream.ExtraLogInformation;
                }

                var zip = new byte[0];
                var error = FuzzerError.EFuzzingErrorType.Fail;
                var result = FuzzerError.EExplotationResult.Unknown;
                var errorId = new Guid(HashHelper.Md5(Encoding.UTF8.GetBytes(errorMsg)));

                if (e is AggregateException agg)
                {
                    e = agg.InnerException;
                }

                if (e is FuzzerException wer)
                {
                    zip = wer.Zip;
                    result = wer.Result;
                    error = FuzzerError.EFuzzingErrorType.Crash;
                    errorId = new Guid(HashHelper.Md5(Encoding.UTF8.GetBytes(errorId.ToString() + wer.ErrorId.ToString())));
                }

                log.Error = new FuzzerError()
                {
                    Error = error,
                    ErrorId = errorId,
                    ExplotationResult = result,
                    ReplicationData = stream.GenerateZip(zip, errorMsg)
                };
            }

            // Update coverage & send

            log.Coverage = CoverageHelper.CurrentCoverage;

            return log;
        }

        /// <summary>
        /// Task thread
        /// </summary>
        /// <param name="obj">Object</param>
        private void AsyncTask(object obj)
        {
            var task = (FuzzerTask)obj;

            try
            {
                task.Action(task.Stream);
            }
            catch (Exception e)
            {
                task.Exception = e;
            }
        }

        /// <summary>
        /// Update from stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cancel">Cancel</param>
        private void UpdateFromStream(Stream stream, CancellationToken cancel)
        {
            var sleep = new ConditionalSleep();
            var buffer = new byte[0x01];

            while (!cancel.IsCancellationRequested)
            {
                cancel.ThrowIfCancellationRequested();
                var read = stream.ReadByte();

                if (read < 0)
                {
                    stream.Dispose();
                    continue;
                }

                switch ((EFuzzerMessageType)read)
                {
                    case EFuzzerMessageType.GoodByte:
                        {
                            Stop();
                            return;
                        }
                    case EFuzzerMessageType.AvailableConfigs:
                        {
                            var entries = SerializationHelper.DeserializeFromJson<FuzzingConfigBase[]>(StreamHelper.ReadString(stream));
                            var list = new List<FuzzingConfigBase>(_configurations);
                            list.AddRange(entries);

                            _configurations = list.ToArray();
                            OnReceiveConfigurations?.Invoke(this, _configurations);
                            break;
                        }
                    case EFuzzerMessageType.AvailableInputs:
                        {
                            var entries = SerializationHelper.DeserializeFromJson<FuzzingInputBase[]>(StreamHelper.ReadString(stream));
                            var list = new List<FuzzingInputBase>(_inputs);
                            list.AddRange(entries);

                            _inputs = list.ToArray();
                            OnReceiveInputs?.Invoke(this, _inputs);
                            break;
                        }
                }

                sleep.Sleep();
            }

            Stop();
        }

        /// <summary>
        /// Clean resources
        /// </summary>
        public void Dispose() => Stop();
    }
}
