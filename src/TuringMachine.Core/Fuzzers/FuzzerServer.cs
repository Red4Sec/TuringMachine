using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;
using TuringMachine.Core.Logs;

namespace TuringMachine.Core.Fuzzers
{
	public class FuzzerServer : IDisposable, IRandomValue<FuzzingConfigBase>, IRandomValue<FuzzingInputBase>
	{
		#region Public properties

		public readonly IDictionary<Guid, FuzzerStat<FuzzingInputBase>> Inputs = new Dictionary<Guid, FuzzerStat<FuzzingInputBase>>();
		public readonly IDictionary<Guid, FuzzerStat<FuzzingConfigBase>> Configurations = new Dictionary<Guid, FuzzerStat<FuzzingConfigBase>>();
		public readonly IDictionary<Guid, FuzzerLog> Logs = new ConcurrentDictionary<Guid, FuzzerLog>();
		public readonly ConcurrentDictionary<Guid, FuzzerStat<FuzzerClientInfo>> Connections = new ConcurrentDictionary<Guid, FuzzerStat<FuzzerClientInfo>>();

		/// <summary>
		/// Total Errors
		/// </summary>
		public int TotalErrors => _TotalErrors;

		/// <summary>
		/// Total Errors
		/// </summary>
		public int UniqueErrors => _UniqueErrors;

		/// <summary>
		/// Is started
		/// </summary>
		public bool IsStarted => _tasks != null;

		public EventHandler<IList<FuzzerLog>> OnReceiveLog;
		public EventHandler<FuzzerClientInfo> OnNewConnection;

		#endregion

		#region Private

		private class LogEntry
		{
			public FuzzerStat<FuzzerClientInfo> Stat;
			public FuzzerLog Log;
		}

		private Task[] _tasks;
		private CancellationTokenSource _cancel;
		private int _UniqueErrors = 0, _TotalErrors = 0;
		private ConcurrentBag<LogEntry> _processLogBag = new ConcurrentBag<LogEntry>();
		private ConcurrentBag<IDisposable> _disposables;

		#endregion

		/// <summary>
		/// Start Pipe server
		/// </summary>
		/// <param name="connection">Connection</param>
		public void Start(FuzzerConnectionBase connection)
		{
			if (IsStarted)
			{
				throw new Exception("Already started");
			}

			try
			{
				_cancel = new CancellationTokenSource();
				_disposables = new ConcurrentBag<IDisposable>();

				if (connection is FuzzerNamedPipeConnection pipe)
				{
					_tasks = new Task[5];
					_tasks[0] = new Task(() => ProcessLogBagAsync(_cancel.Token));

					for (int x = 1; x < _tasks.Length; x++)
					{
						_tasks[x] = new Task(() =>
						{
							var client = new FuzzerClientInfo()
							{
								Id = Guid.NewGuid(),
								Description = "." + pipe.PipeName,
								InternalName = "." + pipe.PipeName
							};

							while (!_cancel.Token.IsCancellationRequested)
							{
								NamedPipeServerStream server = null;

								try
								{
									_cancel.Token.ThrowIfCancellationRequested();

									server = new NamedPipeServerStream(pipe.PipeName, PipeDirection.InOut, _tasks.Length, PipeTransmissionMode.Byte);

									_disposables.Add(server);
									server.WaitForConnection();

									UpdateFromStream(new FuzzerStat<FuzzerClientInfo>(client), server, _cancel.Token);
								}
								catch // (Exception ex)
								{
									// Pipe error
								}
								finally
								{
									try { server?.Dispose(); } catch { }
									_cancel?.Token.ThrowIfCancellationRequested();
								}

								Thread.Sleep(50);
							}
						},
						_cancel.Token);
					}
				}
				else if (connection is FuzzerTcpConnection tcp)
				{
					_tasks = new Task[]
					{
						new Task(()=> ProcessLogBagAsync(_cancel.Token)),
						new Task(() =>
						{
							var server = new TcpListener(tcp.EndPoint);
							_disposables.Add(server.Server);

							server.Start();
							server.BeginAcceptTcpClient(OnNewTcpClient, server);
                            //var res = server.BeginAcceptTcpClient(OnNewTcpClient, server);
                            //_disposables.Add(res.AsyncWaitHandle);
                        },
						_cancel.Token)
					};
				}
				else
				{
					if (connection == null)
					{
						throw new NullReferenceException(nameof(connection));
					}

					throw new ArgumentException(nameof(connection));
				}

				_tasks.All(u =>
				{
					u.Start();
					return true;
				});
			}
			catch (Exception e)
			{
				Stop();
				throw e;
			}
		}

		/// <summary>
		/// Async method for receive TCP clients
		/// </summary>
		/// <param name="ar">Async result</param>
		private void OnNewTcpClient(IAsyncResult ar)
		{
			var server = (TcpListener)ar.AsyncState;

			try
			{
				if (server.Server == null || !server.Server.IsBound)
				{
					return;
				}

				using (var client = server.EndAcceptTcpClient(ar))
				{
					server.BeginAcceptTcpClient(OnNewTcpClient, server);
					//var res = server.BeginAcceptTcpClient(OnNewTcpClient, server);
					//_disposables.Add(res.AsyncWaitHandle);
					var cli = new FuzzerClientInfo()
					{
						Id = Guid.NewGuid(),
						Description = client.Client.RemoteEndPoint.ToString(),
						InternalName = client.Client.RemoteEndPoint.ToString()
					};
					UpdateFromStream(new FuzzerStat<FuzzerClientInfo>(cli), client.GetStream(), _cancel.Token);
				}
			}
			catch
			{
				// When stop the server could throw `ObjectDisposedException`
			}
		}

		/// <summary>
		/// Update from stream
		/// </summary>
		/// <param name="server">Stream</param>
		/// <param name="cancel">Cancel</param>
		private void UpdateFromStream(FuzzerStat<FuzzerClientInfo> stat, Stream stream, CancellationToken cancel)
		{
			var clientId = Guid.NewGuid();

			Connections.TryAdd(clientId, stat);
			OnNewConnection?.Invoke(this, stat.Source);

			var buffer = new byte[40];
			var uuidBuffer = new byte[16];
			var sleep = new ConditionalSleep();

			try
			{
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
								try { stream.Dispose(); } catch { }
								return;
							}
						case EFuzzerMessageType.PushPublicName:
							{
								var json = StreamHelper.ReadString(stream);
								if (string.IsNullOrWhiteSpace(json))
								{
									stat.Source.Description = stat.Source.InternalName;
								}
								else
								{
									stat.Source.Description = stat.Source.InternalName + " [" + SerializationHelper.DeserializeFromJson<string>(json) + "]";
								}
								break;
							}
						case EFuzzerMessageType.GetAvailableConfigs:
							{
								var data = Configurations.Select(u => u.Value.Source).ToArray();
								var msg = FuzzerClient.GetMessage(EFuzzerMessageType.AvailableConfigs, data);
								lock (stream)
								{
									stream.Write(msg, 0, msg.Length);
									stream.Flush();
								}
								break;
							}
						case EFuzzerMessageType.GetAvailableInputs:
							{
								var data = Inputs.Select(u => u.Value.Source).ToArray();
								var msg = FuzzerClient.GetMessage(EFuzzerMessageType.AvailableInputs, data);
								lock (stream)
								{
									stream.Write(msg, 0, msg.Length);
									stream.Flush();
								}
								break;
							}
						case EFuzzerMessageType.PushLog:
							{
								// Faster way to push a regular log

								if (StreamHelper.ReadFull(stream, uuidBuffer, 0, 16) != 16)
								{
									throw new IOException();
								}

								var i = new Guid(uuidBuffer);

								if (StreamHelper.ReadFull(stream, uuidBuffer, 0, 16) != 16 ||
									StreamHelper.ReadFull(stream, buffer, 0, 2) != 2)
								{
									throw new IOException();
								}

								_processLogBag.Add(new LogEntry()
								{
									Stat = stat,
									Log = new FuzzerLog()
									{
										InputId = i,
										ConfigId = new Guid(uuidBuffer),
										Coverage = BitHelper.ToUInt16(buffer, 0) / 100D
									}
								});
								break;
							}
						case EFuzzerMessageType.PushLogWithError:
							{
								var json = StreamHelper.ReadString(stream);
								var log = SerializationHelper.DeserializeFromJson<FuzzerLog>(json);

								if (log != null)
								{
									_processLogBag.Add(new LogEntry()
									{
										Stat = stat,
										Log = log
									});
								}
								break;
							}
					}

					sleep.Sleep();
				}

				Stop();
			}
			catch (Exception ex)
			{
				// Remove the client

				Connections.TryRemove(clientId, out var _);
				throw ex;
			}
		}

		/// <summary>
		/// Process log bag async
		/// </summary>
		/// <param name="cancel">Cancel</param>
		void ProcessLogBagAsync(CancellationToken cancel)
		{
			var sleep = new ConditionalSleep();

			while (!cancel.IsCancellationRequested)
			{
				cancel.ThrowIfCancellationRequested();

				var logs = new List<FuzzerLog>(_processLogBag.Count);
				while (_processLogBag.TryTake(out var entry))
				{
					entry.Stat.Source.Update(entry.Log);

					if (entry.Log.Error != null)
					{
						// Increase config stats

						if (Inputs.TryGetValue(entry.Log.InputId, out var sti))
						{
							sti.Increment(entry.Log.Error.Error);
						}

						if (Configurations.TryGetValue(entry.Log.ConfigId, out var stc))
						{
							stc.Increment(entry.Log.Error.Error);
						}

						// Increment logs

						Interlocked.Increment(ref _TotalErrors);

						if (!Logs.ContainsKey(entry.Log.Error.ErrorId))
						{
							Logs[entry.Log.Error.ErrorId] = entry.Log;
							Interlocked.Increment(ref _UniqueErrors);
						}

						entry.Stat.Increment(entry.Log.Error.Error);
					}
					else
					{
						if (Inputs.TryGetValue(entry.Log.InputId, out var sti))
						{
							sti.Increment();
						}

						if (Configurations.TryGetValue(entry.Log.ConfigId, out var stc))
						{
							stc.Increment();
						}

						entry.Stat.Increment();
					}

					logs.Add(entry.Log);
				}

				if (logs.Count > 0)
				{
					OnReceiveLog?.Invoke(this, logs);
				}

				sleep.Sleep();
			}
		}

		/// <summary>
		/// Stop
		/// </summary>
		public void Stop()
		{
			_cancel?.Cancel();
			_processLogBag.Clear();
			_disposables?.All(u =>
			{
				try { u.Dispose(); } catch { }
				return true;
			});
			_tasks?.All(u =>
			{
				try { u.Dispose(); } catch { }
				return true;
			});

			_cancel?.Dispose();
			_tasks = null;
			_cancel = null;
			_disposables = null;

			// Clear stats

			lock (Inputs)
			{
				foreach (var i in Inputs.Values)
				{
					i.Reset();
				}
			}

			lock (Configurations)
			{
				foreach (var i in Configurations.Values)
				{
					i.Reset();
				}
			}

			Logs.Clear();
			Connections.Clear();
			_UniqueErrors = _TotalErrors = 0;
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
		public FuzzingInputBase GetInput() => RandomHelper.GetRandom(Inputs.Values).Source;

		/// <summary>
		/// Get Random Config
		/// </summary>
		/// <returns>Config</returns>
		FuzzingConfigBase IRandomValue<FuzzingConfigBase>.Get() => GetConfig();

		/// <summary>
		/// Get Random Config
		/// </summary>
		/// <returns>Config</returns>
		public FuzzingConfigBase GetConfig() => RandomHelper.GetRandom(Configurations.Values).Source;

		/// <summary>
		/// Clean resources
		/// </summary>
		public void Dispose() => Stop();
	}
}
