using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using TuringMachine.Core.Fuzzers.Patch;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Tests
{
    [TestFixture]
    public class FuzzingStreamTest
    {
        class Disposable : IDisposable
        {
            public int IsDisposed = 0;

            public void Dispose()
            {
                IsDisposed++;
            }
        }

        [Test]
        public void DisposeTest()
        {
            var original = new byte[90];
            var disposed = new Disposable();

            using (var stream = new FuzzingStream(null, original))
            {
                stream.Variables.Add(0, disposed);

                Assert.AreEqual(0, disposed.IsDisposed);
            }

            Assert.AreEqual(1, disposed.IsDisposed);
        }

        [Test]
        public void CleanTest()
        {
            var original = new byte[90];
            byte[] fuzzed;

            using (var mem = new MemoryStream())
            using (var stream = new FuzzingStream(null, original))
            {
                Assert.AreEqual(stream.CanRead, mem.CanRead);
                Assert.AreEqual(stream.CanWrite, mem.CanWrite);
                Assert.AreEqual(stream.CanSeek, mem.CanSeek);
                Assert.AreEqual(stream.CanTimeout, mem.CanTimeout);
                Assert.AreEqual(stream.Length, original.Length);
                Assert.Catch<InvalidOperationException>(() => { var x = stream.ReadTimeout; });
                Assert.Catch<InvalidOperationException>(() => { var x = stream.WriteTimeout; });
                Assert.Catch<InvalidOperationException>(() => stream.ReadTimeout = 1);
                Assert.Catch<InvalidOperationException>(() => stream.WriteTimeout = 1);

                stream.CopyTo(mem, 16);
                fuzzed = mem.ToArray();

                CollectionAssert.AreEqual(original, fuzzed);
            }
        }

        [Test]
        public void Test1()
        {
            var cfg = SerializationHelper.DeserializeFromJson<PatchConfig>("{\"Changes\":[" +
                "{\"Offset\":1,\"Remove\":7}," +
                "{\"Offset\":23,\"Remove\":11,\"Append\":\"XHVEODAwXHVEQzAwe3t9fQ==\"}],\"Type\":\"Patch\"}");

            var original = new byte[90];
            byte[] fuzzed;

            using (var mem = new MemoryStream())
            using (var stream = new FuzzingStream(cfg, original))
            {
                Assert.AreEqual(stream.CanRead, mem.CanRead);
                Assert.AreEqual(stream.CanWrite, mem.CanWrite);
                Assert.AreEqual(stream.CanSeek, mem.CanSeek);
                Assert.AreEqual(stream.CanTimeout, mem.CanTimeout);
                Assert.AreEqual(stream.Length, original.Length);
                Assert.Catch<InvalidOperationException>(() => { var x = stream.ReadTimeout; });
                Assert.Catch<InvalidOperationException>(() => { var x = stream.WriteTimeout; });
                Assert.Catch<InvalidOperationException>(() => stream.ReadTimeout = 1);
                Assert.Catch<InvalidOperationException>(() => stream.WriteTimeout = 1);

                stream.CopyTo(mem, 16);
                fuzzed = mem.ToArray();
            }

            using (var copyRead = new MemoryStream(original))
            using (var copy = new MemoryStream())
            {
                var buffer = new byte[128];

                // 1 - Offset

                copyRead.Read(buffer, 0, 1);
                copy.Write(buffer, 0, 1);

                // 1 - Remove

                copyRead.Read(buffer, 0, 7);

                // 2 - Offset

                var l = (int)(23 - copyRead.Position);
                copyRead.Read(buffer, 0, l);
                copy.Write(new byte[l], 0, l);

                // 2 - Remove

                copyRead.Read(buffer, 0, 11);

                // 2 - Append

                var data = Convert.FromBase64String("XHVEODAwXHVEQzAwe3t9fQ==");
                copy.Write(data, 0, data.Length);

                // Extra

                l = (int)(copyRead.Length - copyRead.Position);
                copyRead.Read(buffer, 0, l);
                copy.Write(new byte[l], 0, l);

                var dataCopy = copy.ToArray();
                CollectionAssert.AreEqual(dataCopy, fuzzed);
            }
        }

        [Test]
        public void Test2()
        {
            var cfg = SerializationHelper.DeserializeFromJson<PatchConfig>("{\"Changes\":[" +
                "{\"Offset\":16,\"Remove\":18,\"Append\":\"MTg0NDY3NDQwNzM3MDk1NTE2MTV7IiI6MH0=\"}," +
                "{\"Offset\":84,\"Remove\":0,\"Append\":\"LCIiIiI6\"}," +
                "{\"Offset\":104,\"Remove\":4,\"Append\":\"WzBd\"}],\"Type\":\"Patch\"}");

            var original = new byte[114];
            byte[] fuzzed;

            using (var mem = new MemoryStream())
            using (var stream = new FuzzingStream(cfg, original))
            {
                Assert.AreEqual(stream.CanRead, mem.CanRead);
                Assert.AreEqual(stream.CanWrite, mem.CanWrite);
                Assert.AreEqual(stream.CanSeek, mem.CanSeek);
                Assert.AreEqual(stream.CanTimeout, mem.CanTimeout);
                Assert.AreEqual(stream.Length, original.Length);
                Assert.Catch<InvalidOperationException>(() => { var x = stream.ReadTimeout; });
                Assert.Catch<InvalidOperationException>(() => { var x = stream.WriteTimeout; });
                Assert.Catch<InvalidOperationException>(() => stream.ReadTimeout = 1);
                Assert.Catch<InvalidOperationException>(() => stream.WriteTimeout = 1);

                stream.CopyTo(mem, 32);
                fuzzed = mem.ToArray();
            }

            using (var copyRead = new MemoryStream(original))
            using (var copy = new MemoryStream())
            {
                var buffer = new byte[128];

                // 1 - Offset

                copyRead.Read(buffer, 0, 16);
                copy.Write(buffer, 0, 16);

                // 1 - Remove

                copyRead.Read(buffer, 0, 18);

                // 1 - Append

                var data = Convert.FromBase64String("MTg0NDY3NDQwNzM3MDk1NTE2MTV7IiI6MH0=");
                copy.Write(data, 0, data.Length);

                // 2 - Offset

                var l = (int)(84 - copyRead.Position);
                copyRead.Read(buffer, 0, l);
                copy.Write(new byte[l], 0, l);

                // 2 - Append

                data = Convert.FromBase64String("LCIiIiI6");
                copy.Write(data, 0, data.Length);

                // 3 - Offset

                l = (int)(104 - copyRead.Position);
                copyRead.Read(buffer, 0, l);
                copy.Write(new byte[l], 0, l);

                // 3 - Remove

                copyRead.Read(buffer, 0, 4);

                // 3 - Append

                data = Convert.FromBase64String("WzBd");
                copy.Write(data, 0, data.Length);

                // Extra

                l = (int)(copyRead.Length - copyRead.Position);
                copyRead.Read(buffer, 0, l);
                copy.Write(new byte[l], 0, l);

                var dataCopy = copy.ToArray();
                CollectionAssert.AreEqual(dataCopy, fuzzed);
            }
        }

        [Test]
        public void CurrentStreamTest()
        {
            var cfg = SerializationHelper.DeserializeFromJson<PatchConfig>("{\"Changes\":[],\"Type\":\"Patch\"}");

            var fuzzed = new byte[10];
            var original = new byte[114];
            original[0] = 0x01;

            using (var mem = new MemoryStream())
            using (var current = new MemoryStream())
            using (var stream = new FuzzingStream(cfg, original, current))
            {
                Assert.AreEqual(0, current.Position);
                Assert.AreEqual(0, current.Length);

                var r = stream.Read(fuzzed, 0, 1);

                Assert.AreEqual(0x01, fuzzed[0]);
                Assert.AreEqual(1, current.Position);
                Assert.AreEqual(1, current.Length);

                stream.CopyTo(mem, 32);
                fuzzed = mem.ToArray();
            }

            CollectionAssert.AreEqual(original.Skip(1).ToArray(), fuzzed);
        }
    }
}
