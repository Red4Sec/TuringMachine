using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Net;
using TuringMachine.Core.Fuzzers.Patch;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Inputs;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Tests.Helpers
{
    [TestFixture]
    public class SerializationHelperTest
    {
        [Test]
        public void SerializationTest()
        {
            // Serialize

            var a = new IPEndPoint(IPAddress.Any, 123);

            var jsonI = SerializationHelper.SerializeToJson(a, true);
            var jsonNi = SerializationHelper.SerializeToJson(a, false);

            Assert.IsNotNull(jsonI);
            Assert.IsNotNull(jsonNi);
            Assert.IsNull(SerializationHelper.SerializeToJson(null, true));

            // Deserialize

            var b = SerializationHelper.DeserializeFromJson<IPEndPoint>(jsonI);

            Assert.AreEqual(a.Address.ToString(), b.Address.ToString());
            Assert.AreEqual(a.Port, b.Port);

            b = SerializationHelper.DeserializeFromJson<IPEndPoint>(jsonNi);

            Assert.AreEqual(a.Address.ToString(), b.Address.ToString());
            Assert.AreEqual(a.Port, b.Port);

            b = (IPEndPoint)SerializationHelper.DeserializeFromJson(jsonNi, typeof(IPEndPoint));

            Assert.AreEqual(a.Address.ToString(), b.Address.ToString());
            Assert.AreEqual(a.Port, b.Port);

            b = SerializationHelper.DeserializeFromJson<IPEndPoint>(null);

            Assert.IsNull(b);

            b = (IPEndPoint)SerializationHelper.DeserializeFromJson(null, typeof(IPEndPoint));

            Assert.IsNull(b);

            b = (IPEndPoint)SerializationHelper.DeserializeFromJson(jsonNi, null);

            Assert.IsNull(b);
        }

        [Test]
        public void SerializeFileTest()
        {
            // Serialize

            var a = new PatchConfig("Test");
            var json = SerializationHelper.SerializeToJson(a, true);

            Assert.IsNotNull(json);

            // Deserialize

            var file = Path.GetTempFileName();
            File.WriteAllText(file, json);
            var b = SerializationHelper.DeserializeFromFile<PatchConfig>(file).FirstOrDefault();
            File.Delete(file);

            Assert.IsTrue(a.Equals(b));

            // Deserialize wrong value

            file = Path.GetTempFileName();
            File.WriteAllText(file, "null");
            b = SerializationHelper.DeserializeFromFile<PatchConfig>(file).FirstOrDefault();
            File.Delete(file);

            Assert.IsNull(b);
        }

        [Test]
        public void SerializeArrayFileTest()
        {
            // Serialize array

            var a = new PatchConfig[] { new PatchConfig("Test") };
            var json = SerializationHelper.SerializeToJson(a, true);

            Assert.IsNotNull(json);

            // Deserialize

            var file = Path.GetTempFileName();
            File.WriteAllText(file, json);
            var b = SerializationHelper.DeserializeFromFile<PatchConfig>(file).ToArray();
            File.Delete(file);

            CollectionAssert.AreEqual(a, b);

            // Serialize empty array

            a = new PatchConfig[] { };
            json = SerializationHelper.SerializeToJson(a, true);

            Assert.IsNotNull(json);

            // Deserialize

            file = Path.GetTempFileName();
            File.WriteAllText(file, json);
            b = SerializationHelper.DeserializeFromFile<PatchConfig>(file).ToArray();
            File.Delete(file);

            CollectionAssert.AreEqual(a, b);
        }

        [Test]
        public void SerializeManualFileTest()
        {
            var a = new ManualFuzzingInput(new byte[] { 0x01 });

            // Deserialize

            var file = Path.GetTempFileName();
            File.WriteAllBytes(file, a.Data);
            var b = SerializationHelper.DeserializeFromFile<FuzzingInputBase>(file).FirstOrDefault();
            File.Delete(file);

            a.Id = b.Id;
            a.Description = b.Description;

            Assert.AreEqual(a, b);
        }

        [Test]
        public void SerializeErrorTest()
        {
            var file = Path.GetTempFileName();
            File.WriteAllBytes(file, new byte[] { 0x01 });
            var b = SerializationHelper.DeserializeFromFile<PatchConfig>(file).FirstOrDefault();
            File.Delete(file);

            Assert.IsNull(b);
        }
    }
}
