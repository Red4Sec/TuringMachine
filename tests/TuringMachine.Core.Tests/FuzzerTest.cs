using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using TuringMachine.Core.Exceptions;
using TuringMachine.Core.Fuzzers;
using TuringMachine.Core.Fuzzers.Patch;
using TuringMachine.Core.Inputs;
using TuringMachine.Core.Interfaces;
using TuringMachine.Core.Logs;

namespace TuringMachine.Core.Tests
{
    [TestFixture]
    public class FuzzerTest
    {
        private class TestDummyConnection : FuzzerConnectionBase { }

        [Test]
        public void TestNull()
        {
            Assert.Catch<NullReferenceException>(() => Fuzzer.Run(null, null));
        }

        [Test]
        public void TestNamedPipeConnection()
        {
            var clientConnection = new FuzzerNamedPipeConnection();

            Test(clientConnection, clientConnection);
        }

        [Test]
        public void TestTcpConnection()
        {
            var clientConnection = new FuzzerTcpConnection()
            {
                EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7870)
            };
            var serverConnection = new FuzzerTcpConnection()
            {
                EndPoint = new IPEndPoint(IPAddress.Any, clientConnection.EndPoint.Port)
            };

            Test(serverConnection, clientConnection);
        }

        [Test]
        public void TestNullConnection()
        {
            Test(null, null);
        }

        [Test]
        public void TestInvalidConnection()
        {
            Test(new TestDummyConnection(), new TestDummyConnection());
        }

        private void Test(FuzzerConnectionBase serverConnection, FuzzerConnectionBase clientConnection)
        {
            Fuzzer.Client.ExecutionTimeOut = TimeSpan.FromMilliseconds(5_000);

            using (var server = new FuzzerServer())
            using (var client = new FuzzerClient())
            {
                // Ensure no error

                client.SendLog(null);

                // Dummy input

                var entryI = new FuzzerStat<FuzzingInputBase>(new ManualFuzzingInput(new byte[0]) { Description = "1" });
                server.Inputs.Add(entryI.Source.Id, entryI);
                entryI = new FuzzerStat<FuzzingInputBase>(new RandomFuzzingInput(new FromToValue<long>(1, 2)) { Description = "2" });
                server.Inputs.Add(entryI.Source.Id, entryI);

                // Dummy configurations

                var entryC = new FuzzerStat<FuzzingConfigBase>(new PatchConfig("1", new PatchChange("1", 1, 1, new byte[0])));
                server.Configurations.Add(entryC.Source.Id, entryC);
                entryC = new FuzzerStat<FuzzingConfigBase>(new PatchConfig("2", new PatchChange("2", 2, 2, new byte[0])));
                server.Configurations.Add(entryC.Source.Id, entryC);

                // Check server

                CheckConfig(() => ((IRandomValue<FuzzingConfigBase>)server).Get(), server.Configurations.Select(u => u.Value.Source).ToArray());
                CheckConfig(() => ((IRandomValue<FuzzingInputBase>)server).Get(), server.Inputs.Select(u => u.Value.Source).ToArray());
                CheckConfig(() => server.GetConfig(), server.Configurations.Select(u => u.Value.Source).ToArray());
                CheckConfig(() => server.GetInput(), server.Inputs.Select(u => u.Value.Source).ToArray());

                // Start

                Assert.IsFalse(server.IsStarted);
                Assert.IsFalse(client.IsStarted);

                if (serverConnection == null && clientConnection == null)
                {
                    Assert.Catch<NullReferenceException>(() => server.Start(serverConnection));
                    Assert.Catch<NullReferenceException>(() => client.Start(clientConnection));
                    return;
                }
                else
                {
                    if (serverConnection is TestDummyConnection && clientConnection is TestDummyConnection)
                    {
                        Assert.Catch<ArgumentException>(() => server.Start(serverConnection));
                        Assert.Catch<ArgumentException>(() => client.Start(clientConnection));
                        return;
                    }
                }

                var waitInput = new ManualResetEvent(false);
                var waitConfigs = new ManualResetEvent(false);
                var waitLog = new ManualResetEvent(false);
                var waitLogError = new ManualResetEvent(false);

                client.OnReceiveInputs += (s, e) => waitInput.Set();
                client.OnReceiveConfigurations += (s, e) => waitConfigs.Set();
                server.OnReceiveLog += (s, e) => (e.Any(u => u.Error != null) ? waitLogError : waitLog).Set();

                server.Start(serverConnection);
                Thread.Sleep(250); // Wait for server
                client.Start(clientConnection);

                Assert.IsTrue(server.IsStarted);
                Assert.IsTrue(client.IsStarted);

                // Already started

                Assert.Catch<Exception>(() => server.Start(serverConnection));
                Assert.Catch<Exception>(() => client.Start(clientConnection));

                // Check client

                Assert.IsTrue(waitConfigs.WaitOne(TimeSpan.FromSeconds(10)), "Waiting for configs");
                Assert.IsTrue(waitInput.WaitOne(TimeSpan.FromSeconds(10)), "Waiting for inputs");

                Assert.AreEqual(1, server.Connections.Count);
                Assert.IsTrue(!string.IsNullOrEmpty(server.Connections.Values.FirstOrDefault()?.Source.Description));
                Assert.AreNotEqual(Guid.Empty, server.Connections.Values.FirstOrDefault()?.Source.Id);
                Assert.AreNotEqual(Guid.Empty, server.Connections.Values.FirstOrDefault()?.Id);

                CheckConfig(() => ((IRandomValue<FuzzingConfigBase>)client).Get(), server.Configurations.Select(u => u.Value.Source).ToArray());
                CheckConfig(() => ((IRandomValue<FuzzingInputBase>)client).Get(), server.Inputs.Select(u => u.Value.Source).ToArray());
                CheckConfig(() => client.GetConfig(), server.Configurations.Select(u => u.Value.Source).ToArray());
                CheckConfig(() => client.GetInput(), server.Inputs.Select(u => u.Value.Source).ToArray());

                // Send log

                var cfg = client.GetConfig();
                var input = client.GetInput();
                var log = new FuzzerLog()
                {
                    ConfigId = cfg.Id,
                    InputId = input.Id,
                };

                var sIn = server.Inputs.Select(u => u.Value).Where(u => u.Source.Id == log.InputId).FirstOrDefault();
                var sCfg = server.Configurations.Select(u => u.Value).Where(u => u.Source.Id == log.ConfigId).FirstOrDefault();

                Assert.AreEqual(0, server.Logs.Count);
                Assert.AreEqual(0, server.UniqueErrors);
                Assert.AreEqual(0, server.TotalErrors);

                client.SendLog(log);

                Assert.IsTrue(waitLog.WaitOne(TimeSpan.FromSeconds(10)), "Waiting for log");

                Assert.AreEqual(0, server.Logs.Count);
                Assert.AreEqual(0, server.UniqueErrors);
                Assert.AreEqual(0, server.TotalErrors);

                // Check stats

                Assert.AreEqual(1, sIn.Tests);
                Assert.AreEqual(1, sCfg.Tests);
                Assert.AreEqual(0, sIn.Crashes);
                Assert.AreEqual(0, sCfg.Crashes);
                Assert.AreEqual(0, sIn.Errors);
                Assert.AreEqual(0, sCfg.Errors);

                // Send error

                log = new FuzzerLog()
                {
                    ConfigId = cfg.Id,
                    InputId = input.Id,
                    Error = new FuzzerError()
                    {
                        ErrorId = Guid.NewGuid(),
                        Error = FuzzerError.EFuzzingErrorType.Crash,
                        ExplotationResult = FuzzerError.EExplotationResult.Exploitable,
                        ReplicationData = new byte[0],
                    }
                };

                Assert.AreEqual(0, server.UniqueErrors);
                Assert.AreEqual(0, server.TotalErrors);
                client.SendLog(log);

                waitLogError.Reset();
                Assert.IsTrue(waitLogError.WaitOne(TimeSpan.FromSeconds(10)), "Waiting for error");

                Assert.AreEqual(1, server.Logs.Count);
                Assert.IsTrue(server.Logs.TryGetValue(log.Error.ErrorId, out var peekLog));
                Assert.IsTrue(log.Equals(peekLog));
                Assert.AreEqual(1, server.UniqueErrors);
                Assert.AreEqual(1, server.TotalErrors);

                // Check stats

                Assert.AreEqual(2, sIn.Tests);
                Assert.AreEqual(2, sCfg.Tests);
                Assert.AreEqual(1, sIn.Crashes);
                Assert.AreEqual(1, sCfg.Crashes);
                Assert.AreEqual(0, sIn.Errors);
                Assert.AreEqual(0, sCfg.Errors);

                // Generic MultiClient

                FuzzerLog gerr = null;
                Fuzzer.Client.Stop();
                if (serverConnection != clientConnection)
                {
                    // Test default
                    Fuzzer.Client.Start(clientConnection);
                }

                waitLogError.Reset();

                Fuzzer.Run(FuzWERSample, new FuzzerRunArgs()
                {
                    OnLog = (l, c) =>
                    {
                        if (l.Error != null)
                        {
                            c.Cancel = true;
                            gerr = l;
                        }
                    }
                });

                // Could spend more time because are more tests

                Assert.IsTrue(waitLogError.WaitOne(TimeSpan.FromSeconds(30)), "Waiting for error");
                Assert.AreEqual(2, server.Logs.Count);
                Assert.IsTrue(server.Logs.TryGetValue(gerr.Error.ErrorId, out peekLog));
                Assert.IsTrue(gerr.Equals(peekLog));
                Assert.IsTrue(gerr.Error.ReplicationData.Length > 0);
                Assert.AreEqual(FuzzerError.EExplotationResult.Exploitable, gerr.Error.ExplotationResult);
                Assert.AreEqual(FuzzerError.EFuzzingErrorType.Crash, gerr.Error.Error);
                Assert.AreEqual(2, server.UniqueErrors);
                Assert.AreEqual(2, server.TotalErrors);

                // Generic MultiThread Client

                gerr = null;
                Fuzzer.Client.Stop();
                Thread.Sleep(250); // Wait some time for pipes

                if (serverConnection != clientConnection)
                {
                    // Test default
                    Fuzzer.Client.Start(clientConnection);
                }

                waitLogError.Reset();

                Fuzzer.Run(1, FuzMultiThreadSample, new FuzzerRunArgs()
                {
                    OnLog = (l, c) =>
                    {
                        if (l.Error != null)
                        {
                            c.Cancel = true;
                            gerr = l;
                        }
                    }
                });

                // Could spend more time because are more tests

                Assert.IsTrue(waitLogError.WaitOne(TimeSpan.FromSeconds(30)), "Waiting for error");
                Assert.AreEqual(3, server.Logs.Count);
                Assert.IsTrue(server.Logs.TryGetValue(gerr.Error.ErrorId, out peekLog));
                Assert.IsTrue(gerr.Equals(peekLog));
                Assert.IsTrue(gerr.Error.ReplicationData.Length > 0);
                Assert.AreEqual(3, server.UniqueErrors);
                Assert.AreEqual(3, server.TotalErrors);

                // Test timeout

                gerr = null;
                waitLogError.Reset();

                Fuzzer.Client.ExecutionTimeOut = TimeSpan.FromMilliseconds(250);
                Fuzzer.Run(FuzTimeoutSample, new FuzzerRunArgs()
                {
                    OnLog = (l, c) =>
                    {
                        c.Cancel = true;

                        if (l.Error != null)
                        {
                            gerr = l;
                        }
                    }
                });

                Assert.IsTrue(waitLogError.WaitOne(TimeSpan.FromSeconds(5)), "Waiting for error");
                Assert.AreEqual(4, server.Logs.Count);
                Assert.IsTrue(server.Logs.TryGetValue(gerr.Error.ErrorId, out peekLog));
                Assert.IsTrue(gerr.Equals(peekLog));
                Assert.IsTrue(gerr.Error.ReplicationData.Length > 0);
                Assert.AreEqual(4, server.UniqueErrors);
                Assert.AreEqual(4, server.TotalErrors);

                // Current stream

                var logReaded = new byte[255];
                var current = new byte[logReaded.Length];
                FuzzingStream fuzStream = null;

                Fuzzer.Run((stream) =>
                {
                    Array.Resize(ref current, stream.Read(current, 0, current.Length));
                    fuzStream = (FuzzingStream)stream;
                    Assert.IsNotNull(fuzStream.CurrentStream);
                },
                new FuzzerRunArgs()
                {
                    StoreCurrent = true,
                    OnLog = (l, c) =>
                    {
                        logReaded = File.ReadAllBytes(((FileStream)fuzStream.CurrentStream).Name);
                        c.Cancel = true;
                    },
                });

                Assert.IsNotNull(fuzStream);
                Assert.IsNotNull(fuzStream.CurrentStream);
                CollectionAssert.AreEqual(current, logReaded);

                // Clean

                Fuzzer.Client.Stop();

                waitInput.Dispose();
                waitConfigs.Dispose();
                waitLog.Dispose();
                waitLogError.Dispose();
            }
        }

        private void FuzTimeoutSample(Stream stream)
        {
            Thread.Sleep(20_000);
        }

        private void FuzMultiThreadSample(Stream stream, int taskId)
        {
            if (stream.ReadByte() < 50)
            {
                throw new Exception("Win MultiThreaded");
            }
        }

        private void FuzWERSample(Stream stream)
        {
            if (stream.ReadByte() < 50)
            {
                // Crash

                throw new FuzzerException(Guid.Empty, new byte[0], FuzzerError.EExplotationResult.Exploitable);
            }
        }

        #region Checks

        private void CheckConfig(Func<IIdentificable> action, IIdentificable[] checks)
        {
            // Check inputs

            var bools = new bool[checks.Length];

            for (int x = 0; x < 200 && !bools.All(u => u); x++)
            {
                var value = action();
                Assert.IsNotNull(value);
                bools[Find(checks, value)] = true;
            }

            Assert.IsTrue(bools.All(u => u));
        }

        private int Find(IIdentificable[] checks, IIdentificable value)
        {
            for (int x = 0; x < checks.Length; x++)
            {
                if (checks[x].Equals(value))
                {
                    return x;
                }
            }

            return -1;
        }

        #endregion
    }
}
