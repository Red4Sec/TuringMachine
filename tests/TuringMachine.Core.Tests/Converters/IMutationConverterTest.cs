using Newtonsoft.Json;
using NUnit.Framework;
using TuringMachine.Core.Converters;
using TuringMachine.Core.Fuzzers.Mutational;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Tests.Converters
{
	[TestFixture]
	public class IMutationConverterTest
	{
		class Test
		{
			[JsonConverter(typeof(IMutationConverter))]
			public IMutation ValueA;

			[JsonConverter(typeof(IMutationConverter))]
			public IMutation ValueB;
		}

		[Test]
		public void TestSerialization()
		{
			var input = new Test()
			{
				ValueA = new MutationalChunk("5", "6"),
				ValueB = new MutationalFromTo(5, 6)
			};

			var json = SerializationHelper.SerializeToJson(input);
			var copy = SerializationHelper.DeserializeFromJson<Test>(json);

			Assert.IsTrue(input.ValueA.Equals(copy.ValueA));
			Assert.IsTrue(input.ValueB.Equals(copy.ValueB));

			copy.ValueA = new MutationalChunk("5", "7");
			copy.ValueB = new MutationalFromTo(5, 7);

			Assert.IsFalse(input.ValueA.Equals(copy.ValueA));
			Assert.IsFalse(input.ValueB.Equals(copy.ValueB));
		}
	}
}
