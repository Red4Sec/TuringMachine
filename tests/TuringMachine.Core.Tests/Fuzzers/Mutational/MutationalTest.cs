using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TuringMachine.Core.Collections;
using TuringMachine.Core.Fuzzers.Mutational;
using TuringMachine.Core.Fuzzers.Mutational.Filters;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Inputs;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Tests.Fuzzers.Mutational
{
    [TestFixture]
    public class MutationalTests
    {
        [Test]
        public void MutationalChunkEqualTest()
        {
            var config = new MutationalChunk(new byte[][] { new byte[] { 0x01, 0x02 } });
            var copy = new MutationalChunk(new byte[][] { new byte[] { 0x01, 0x02 } });

            Assert.IsTrue(config.Equals(copy));
            Assert.IsTrue(config.Equals((object)copy));
            Assert.IsTrue(config.Equals((IMutation)copy));
            Assert.IsFalse(config.Equals(new object()));
            Assert.IsFalse(config.Equals(new MutationalFromTo()));
            Assert.AreEqual(config.GetHashCode(), copy.GetHashCode());

            var json = SerializationHelper.SerializeToJson(config);
            config = SerializationHelper.DeserializeFromJson<MutationalChunk>(json);

            Assert.IsTrue(config.Equals(copy));
            Assert.IsTrue(config.Equals((object)copy));
            Assert.IsFalse(config.Equals(new object()));
            Assert.AreEqual(config.GetHashCode(), copy.GetHashCode());

            copy = new MutationalChunk(new byte[][] { new byte[] { 0x01 } });

            Assert.IsFalse(config.Equals(copy));

            copy = new MutationalChunk("a", "b");

            Assert.IsFalse(config.Equals(copy));
            Assert.IsFalse(config.Equals((IMutation)copy));
        }

        [Test]
        public void MutationalFromToTest()
        {
            var config = new MutationalFromTo(1, 2);
            var copy = new MutationalFromTo(1, 2);

            Assert.IsTrue(config.Equals(copy));
            Assert.IsTrue(config.Equals((object)copy));
            Assert.IsFalse(config.Equals(new object()));
            Assert.IsFalse(config.Equals(new MutationalChunk()));
            Assert.AreEqual(config.GetHashCode(), copy.GetHashCode());

            var json = SerializationHelper.SerializeToJson(config);
            config = SerializationHelper.DeserializeFromJson<MutationalFromTo>(json);

            Assert.IsTrue(config.Equals(copy));
            Assert.IsTrue(config.Equals((IMutation)copy));
            Assert.IsTrue(config.Equals((object)copy));
            Assert.IsFalse(config.Equals(new object()));
            Assert.AreEqual(config.GetHashCode(), copy.GetHashCode());

            config.Excludes.Add(2);
            Assert.IsFalse(config.Equals(copy));

            copy = new MutationalFromTo(1);

            Assert.IsFalse(config.Equals(copy));
            Assert.IsFalse(config.Equals((IMutation)copy));
        }

        [Test]
        public void MutationalChangeEmptyTest()
        {
            var value = new MutationalChange()
            {
                Append = null,
                AppendIterations = new FromToValue<ushort>(0),
                Description = "Empty",
                RemoveLength = new FromToValue<ushort>(1),
                Weight = 1
            };

            // With remove

            var result = value.Process(0);

            Assert.IsNotNull(result);
            Assert.AreEqual("Empty", result.Description);
            Assert.IsNull(result.Append);
            Assert.AreEqual(1, result.Remove);
            Assert.AreEqual(0, result.Offset);

            // Null

            value.RemoveLength = new FromToValue<ushort>(0);

            result = value.Process(0);

            Assert.IsNull(result);
        }

        [Test]
        public void MutationalEmptyTest()
        {
            var config = new MutationConfig();

            var data = new byte[200];
            for (byte x = 0; x < data.Length; x++)
            {
                data[x] = x;
            }

            // Without entry

            using (var copy = new MemoryStream())
            using (var stream = new FuzzingStream(config, data))
            {
                stream.CopyTo(copy, 64);

                CollectionAssert.AreEqual(data, copy.ToArray());
            }

            // With entry

            config.Mutations.Add(new MutationalEntry());

            using (var copy = new MemoryStream())
            using (var stream = new FuzzingStream(config, data))
            {
                stream.CopyTo(copy, 64);

                CollectionAssert.AreEqual(data, copy.ToArray());
            }
        }

        [Test]
        public void MutationalSerializationTest()
        {
            // Test deserialization

            var value = File.ReadAllText("Samples/MutationalSample.fmut");
            var config = SerializationHelper.DeserializeFromJson<MutationConfig>(value);

            Assert.AreEqual("ceb9d9e9-37d1-4a4a-9cb9-d2f4d31c1d22", config.Id.ToString());
            Assert.AreEqual("Test", config.Description);
            Assert.AreEqual("Mutational", config.Type);
            Assert.AreEqual(1, config.Mutations.Count);

            var entry = config.Mutations[0];

            Assert.AreEqual("First change", entry.Description);
            Assert.AreEqual(EFuzzingPercentType.PeerByte, entry.FuzzPercentType);
            Assert.IsTrue(new FixedValue<double>(5).Equals(entry.FuzzPercent));
            Assert.IsTrue(new FromToValue<ushort>(0, 2).Equals(entry.MaxChanges));
            Assert.IsTrue(new FromToValue<long>(0, long.MaxValue).Equals(entry.ValidOffset));

            Assert.AreEqual(2, entry.Changes.Count);

            Assert.IsTrue(new MutationalChange()
            {
                Description = "FromTo test",
                Weight = 1,
                AppendIterations = new FixedValue<ushort>(1),
                RemoveLength = new FixedValue<ushort>(1),
                Append = new MutationalFromTo(1, 2),
                Filter = new WeightCollection<IChunkFilter>
                (
                    new MixCaseFilter()
                    {
                        FilterPercent = new FromToValue<double>(0, 10),
                        MixType = MixCaseFilter.MixCaseType.ToLowerCase,
                        Weight = 1
                    },
                    new MixCaseFilter()
                    {
                        FilterPercent = new FromToValue<double>(0, 10),
                        MixType = MixCaseFilter.MixCaseType.ToUpperCase,
                        Weight = 1
                    },
                    new MixCaseFilter()
                    {
                        FilterPercent = new FromToValue<double>(0, 10),
                        MixType = MixCaseFilter.MixCaseType.ChangeCase,
                        Weight = 1
                    }
                )
            }
            .Equals(entry.Changes[0]));

            Assert.IsTrue(new MutationalChange()
            {
                Description = "Chunks",
                Weight = 1,
                AppendIterations = new FixedValue<ushort>(1),
                RemoveLength = new FixedValue<ushort>(1),
                Append = new MutationalChunk(new byte[][] { new byte[] { 0x01, 0x02 } })
            }
            .Equals(entry.Changes[1]));

            // Test serialization

            var json = SerializationHelper.SerializeToJson(config, true);
            Assert.AreEqual(JObject.Parse(value).ToString(Formatting.Indented), json);

            // Test Equals

            var copy = SerializationHelper.DeserializeFromJson<MutationConfig>(json);
            var copy2 = SerializationHelper.DeserializeFromJson<FuzzingConfigBase>(json);

            Assert.IsTrue(copy.Equals(copy2));

            // Test PatchChange Equals

            Assert.IsTrue(config.Equals(copy));
            Assert.IsTrue(config.Equals((object)copy));
            Assert.IsFalse(config.Equals(new object()));
            Assert.AreEqual(config.GetHashCode(), copy.GetHashCode());

            Assert.IsTrue(config.Mutations[0].Equals(copy.Mutations[0]));
            Assert.IsTrue(config.Mutations[0].Equals((object)copy.Mutations[0]));
            Assert.IsFalse(config.Mutations[0].Equals(new object()));
            Assert.AreEqual(config.Mutations[0].GetHashCode(), copy.Mutations[0].GetHashCode());

            Assert.IsTrue(config.Mutations[0].Changes[0].Equals(copy.Mutations[0].Changes[0]));
            Assert.IsTrue(config.Mutations[0].Changes[0].Equals((object)copy.Mutations[0].Changes[0]));
            Assert.IsFalse(config.Mutations[0].Changes[0].Equals(new object()));
            Assert.AreEqual(config.Mutations[0].Changes[0].GetHashCode(), copy.Mutations[0].Changes[0].GetHashCode());

            Assert.IsTrue(config.Mutations[0].Changes[1].Equals(copy.Mutations[0].Changes[1]));
            Assert.IsTrue(config.Mutations[0].Changes[1].Equals((object)copy.Mutations[0].Changes[1]));
            Assert.IsFalse(config.Mutations[0].Changes[1].Equals(new object()));
            Assert.AreEqual(config.Mutations[0].Changes[1].GetHashCode(), copy.Mutations[0].Changes[1].GetHashCode());

            copy.Description += "X";

            Assert.AreNotEqual(config.GetHashCode(), copy.GetHashCode());

            copy.Mutations[0].Description += "X";

            Assert.AreNotEqual(config.Mutations[0].GetHashCode(), copy.Mutations[0].GetHashCode());

            copy.Mutations[0].Changes[0].Description += "X";
            copy.Mutations[0].Changes[1].Description += "X";

            Assert.AreNotEqual(config.Mutations[0].Changes[0].GetHashCode(), copy.Mutations[0].Changes[0].GetHashCode());
            Assert.AreNotEqual(config.Mutations[0].Changes[1].GetHashCode(), copy.Mutations[0].Changes[1].GetHashCode());
        }

        [Test]
        public void MutationalChangeSingleByteTest()
        {
            var c = new MutationalChange()
            {
                Description = "Test",
                Append = new MutationalFromTo(1, 100),
                RemoveLength = new FromToValue<ushort>(4),
                AppendIterations = new FromToValue<ushort>(4),
                Weight = 1
            };

            var data = new List<string>();

            for (int x = 0; x < 100; x++)
            {
                var ret = c.Process(0);
                var hex = StringHelper.ByteArrayToHex(ret.Append);

                Assert.IsFalse(data.Contains(hex));
                data.Add(hex);

                Assert.AreEqual(0, ret.Offset);
                Assert.AreEqual(4, ret.Remove);
                Assert.AreEqual(4, ret.Append.Length);
                Assert.AreEqual("Test", ret.Description);
                Assert.IsTrue(ret.Append[0] >= 1 && ret.Append[0] <= 100);
                Assert.IsTrue(ret.Append[1] >= 1 && ret.Append[1] <= 100);
                Assert.IsTrue(ret.Append[2] >= 1 && ret.Append[2] <= 100);
                Assert.IsTrue(ret.Append[3] >= 1 && ret.Append[3] <= 100);
            }
        }

        [Test]
        public void MutationalChangeChunkTest()
        {
            var c = new MutationalChange()
            {
                Description = "Test",
                Append = new MutationalChunk(new byte[][] { new byte[] { 0x01, 0x02 }, new byte[] { 0x03, 0x04 } }),
                RemoveLength = new FromToValue<ushort>(0),
                AppendIterations = new FromToValue<ushort>(1),
                Weight = 1
            };

            var data = new List<string>
            {
                StringHelper.ByteArrayToHex(new byte[] { 0x01, 0x02 }),
                StringHelper.ByteArrayToHex(new byte[] { 0x03, 0x04 })
            };

            for (int x = 0; x < 100; x++)
            {
                var ret = c.Process(0);
                var hex = StringHelper.ByteArrayToHex(ret.Append);

                Assert.IsTrue(data.Contains(hex));
                Assert.AreEqual(0, ret.Offset);
                Assert.AreEqual(0, ret.Remove);
                Assert.AreEqual(2, ret.Append.Length);
                Assert.AreEqual("Test", ret.Description);
            }
        }

        [Test]
        public void MutationalEntryPeerByteTest()
        {
            var config = new MutationConfig() { Description = "Test" };
            var entry = new MutationalEntry()
            {
                FuzzPercent = new FromToValue<double>(100),
                ValidOffset = new FromToValue<long>(0, long.MaxValue),
                MaxChanges = new FromToValue<ushort>(ushort.MaxValue),
                FuzzPercentType = EFuzzingPercentType.PeerByte
            };

            // Config

            config.Mutations.Add(entry);
            entry.Changes.Add(new MutationalChange()
            {
                Weight = 5,
                Append = new MutationalFromTo((byte)'A'),
                RemoveLength = new FromToValue<ushort>(1),
                AppendIterations = new FromToValue<ushort>(1)
            });
            entry.Changes.Add(new MutationalChange()
            {
                // Remmove
                Weight = 1,
                RemoveLength = new FromToValue<ushort>(1),
                AppendIterations = new FromToValue<ushort>(1)
            });

            // 100%

            var input = new ManualFuzzingInput(new byte[200]);
            using (var stream = new FuzzingStream(config, input.GetStream()))
            {
                for (long x = 0; x < 200; x++)
                {
                    Assert.IsNotNull(entry.Get(stream, x, 0));
                }
            }

            // 0%

            entry.FuzzPercent = new FromToValue<double>(0);

            input = new ManualFuzzingInput(new byte[200]);
            using (var stream = new FuzzingStream(config, input.GetStream()))
            {
                for (long x = 0; x < 200; x++)
                {
                    Assert.IsNull(entry.Get(stream, x, 0));
                }
            }

            // Argument excepcion

            entry.FuzzPercentType = (EFuzzingPercentType)197;
            Assert.Throws<ArgumentException>(() => entry.Get(null, 0, 0));

            // Only offset 5

            entry.FuzzPercentType = EFuzzingPercentType.PeerByte;
            entry.FuzzPercent = new FromToValue<double>(100);
            entry.ValidOffset = new FromToValue<long>(5);

            input = new ManualFuzzingInput(new byte[100]);
            using (var stream = new FuzzingStream(config, input.GetStream()))
            {
                for (long x = 0; x < 100; x++)
                {
                    var next = entry.Get(stream, x, 0);

                    if (x == 5)
                    {
                        Assert.IsNotNull(next);
                    }
                    else
                    {
                        Assert.IsNull(next);
                    }
                }
            }

            // Max changes 2

            entry.ValidOffset = new FromToValue<long>(0, long.MaxValue);
            entry.MaxChanges = new FromToValue<ushort>(2);

            input = new ManualFuzzingInput(new byte[100]);
            using (var stream = new FuzzingStream(config, input.GetStream()))
            {
                stream.CopyTo(new MemoryStream(), 100);

                Assert.AreEqual(2, stream.Log.Length);
                Assert.AreEqual(0, stream.Log[0].Offset);
                Assert.AreEqual(1, stream.Log[1].Offset);
            }
        }

        [Test]
        public void MutationalEntryPeerStreamTest()
        {
            var config = new MutationConfig() { Description = "Test" };
            var entry = new MutationalEntry()
            {
                FuzzPercent = new FromToValue<double>(100),
                ValidOffset = new FromToValue<long>(0, long.MaxValue),
                MaxChanges = new FromToValue<ushort>(50),
                FuzzPercentType = EFuzzingPercentType.PeerStream
            };

            // Config

            config.Mutations.Add(entry);
            entry.Changes.Add(new MutationalChange()
            {
                Weight = 1,
                Append = new MutationalFromTo(0x01),
                RemoveLength = new FromToValue<ushort>(),
                AppendIterations = new FromToValue<ushort>(1)
            });

            // 100% / 50 changes

            var input = new ManualFuzzingInput(new byte[200]);
            using (var copy = new MemoryStream())
            using (var stream = new FuzzingStream(config, input.GetStream()))
            {
                stream.CopyTo(copy, 200);
                Assert.AreEqual(50, copy.ToArray().Count(u => u == 0x01));
            }

            // 0%

            entry.FuzzPercent = new FromToValue<double>(0);

            input = new ManualFuzzingInput(new byte[200]);
            using (var copy = new MemoryStream())
            using (var stream = new FuzzingStream(config, input.GetStream()))
            {
                stream.CopyTo(copy, 200);
                Assert.AreEqual(200, copy.ToArray().Count(u => u == 0x00));
            }

            // Only offset 5

            entry.FuzzPercent = new FromToValue<double>(100);
            entry.ValidOffset = new FromToValue<long>(5);
            entry.MaxChanges = new FromToValue<ushort>(1);

            input = new ManualFuzzingInput(new byte[200]);
            using (var stream = new FuzzingStream(config, input.GetStream()))
            {
                for (long x = 0; x < 200; x++)
                {
                    var next = entry.Get(stream, x, 0);

                    if (x == 5)
                    {
                        Assert.IsNotNull(next);
                    }
                    else
                    {
                        Assert.IsNull(next);
                    }
                }
            }

            // Max changes 2

            entry.ValidOffset = new FromToValue<long>(0, long.MaxValue);
            entry.MaxChanges = new FromToValue<ushort>(2);

            input = new ManualFuzzingInput(new byte[200]);
            using (var stream = new FuzzingStream(config, input.GetStream()))
            {
                stream.CopyTo(new MemoryStream(), 200);

                Assert.AreEqual(2, stream.Log.Length);
            }
        }
    }
}
