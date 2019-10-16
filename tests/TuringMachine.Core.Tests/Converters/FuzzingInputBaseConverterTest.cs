using Newtonsoft.Json;
using NUnit.Framework;
using TuringMachine.Core.Converters;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Inputs;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Tests.Converters
{
    [TestFixture]
    public class FuzzingInputBaseConverterTest
    {
        class Test
        {
            [JsonConverter(typeof(FuzzingInputBaseConverter))]
            public FuzzingInputBase Value;
        }

        [Test]
        public void TestSerialization()
        {
            var input = new Test()
            {
                Value = new ManualFuzzingInput(new byte[] { 0x01, 0x02 })
                {
                    Description = "Test"
                }
            };

            var json = SerializationHelper.SerializeToJson(input);
            var copy = SerializationHelper.DeserializeFromJson<Test>(json);

            Assert.IsTrue(input.Value.Equals(copy.Value));

            copy.Value.Description += "Wrong";

            Assert.IsFalse(input.Value.Equals(copy.Value));
        }
    }
}
