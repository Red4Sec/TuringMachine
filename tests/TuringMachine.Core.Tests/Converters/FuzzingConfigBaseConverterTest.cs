using Newtonsoft.Json;
using NUnit.Framework;
using TuringMachine.Core.Converters;
using TuringMachine.Core.Fuzzers.Patch;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Tests.Converters
{
	[TestFixture]
	public class FuzzingConfigBaseConverterTest
	{
		class Test
		{
			[JsonConverter(typeof(FuzzingConfigBaseConverter))]
			public FuzzingConfigBase Value;
		}

		[Test]
		public void TestSerialization()
		{
			var input = new Test()
			{
				Value = new PatchConfig()
				{
					Description = "Test",
					Changes = new System.Collections.Generic.List<PatchChange>
					(
						new PatchChange[] { new PatchChange("Change", 1, 2, new byte[] { 0x01 }) }
					)
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
