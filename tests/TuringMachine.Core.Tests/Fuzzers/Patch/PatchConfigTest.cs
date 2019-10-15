using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.IO;
using TuringMachine.Core.Fuzzers.Patch;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Tests.Fuzzers.Patch
{
    [TestFixture]
    public class PatchConfigTest
    {
        [Test]
        public void Test_Patch_Serialization()
        {
            // Test deserialization

            var value = File.ReadAllText("Samples/PatchSample.fpatch");

            var config = SerializationHelper.DeserializeFromJson<PatchConfig>(value);

            Assert.AreEqual("8492c051-3acc-4681-8c42-51088cfa0f13", config.Id.ToString());
            Assert.AreEqual("Test", config.Description);
            Assert.AreEqual("Patch", config.Type);
            Assert.AreEqual(1, config.Changes.Count);

            var entry = config.Changes[0];

            Assert.IsTrue(new PatchChange("Buffer overflow (by char)", 16, 1,
                Convert.FromBase64String("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"))
                .Equals(entry));

            // Test PatchConfig and default constructor

            var json = SerializationHelper.SerializeToJson(config, true);
            var copy = SerializationHelper.DeserializeFromJson<PatchConfig>(json);
            var copy2 = new PatchConfig(copy.Description, copy.Changes.ToArray())
            {
                Id = copy.Id
            };

            Assert.AreEqual(JObject.Parse(value).ToString(Formatting.Indented), SerializationHelper.SerializeToJson(config, true));

            Assert.AreEqual(SerializationHelper.SerializeToJson(config, true), SerializationHelper.SerializeToJson(copy, true));
            Assert.AreEqual(SerializationHelper.SerializeToJson(copy, true), SerializationHelper.SerializeToJson(copy2, true));
            var copy3 = SerializationHelper.DeserializeFromJson<FuzzingConfigBase>(json);

            Assert.IsTrue(copy.Equals(copy3));

            // Get null

            var stream = new FuzzingStream(config, new byte[100]);
            config.InitFor(stream);

            var change = config.Get(stream);

            Assert.IsNull(change);

            // Seek Offset

            stream.Position = 16;
            change = config.Get(stream);

            Assert.AreEqual(change, config.Changes[0]);

            // Test PatchChange Equals

            Assert.IsTrue(entry.Equals(copy.Changes[0]));
            Assert.IsTrue(entry.Equals((object)copy.Changes[0]));
            Assert.IsFalse(entry.Equals(new object()));
            Assert.AreEqual(entry.GetHashCode(), copy.Changes[0].GetHashCode());

            entry.Offset++;

            Assert.AreNotEqual(entry.GetHashCode(), copy.Changes[0].GetHashCode());

            // Test PatchConfig Equals

            config = SerializationHelper.DeserializeFromJson<PatchConfig>(json);
            copy = SerializationHelper.DeserializeFromJson<PatchConfig>(json);

            Assert.IsTrue(config.Equals(copy));
            Assert.IsTrue(config.Equals((object)copy));
            Assert.IsFalse(config.Equals(new object()));
            Assert.AreEqual(config.GetHashCode(), copy.GetHashCode());

            config.Id = Guid.NewGuid();

            Assert.AreNotEqual(config.GetHashCode(), copy.GetHashCode());
        }
    }
}