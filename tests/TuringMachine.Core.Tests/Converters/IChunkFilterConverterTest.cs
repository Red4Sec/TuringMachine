using Newtonsoft.Json;
using NUnit.Framework;
using TuringMachine.Core.Converters;
using TuringMachine.Core.Fuzzers.Mutational;
using TuringMachine.Core.Fuzzers.Mutational.Filters;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Tests.Converters
{
	[TestFixture]
	public class IChunkFilterConverterTest
	{
		class Test
		{
			[JsonConverter(typeof(IChunkFilterConverter))]
			public IChunkFilter Value;
		}

		[Test]
		public void TestSerialization()
		{
			var input = new Test()
			{
				Value = new MixCaseFilter()
				{
					MixType = MixCaseFilter.MixCaseType.ToLowerCase,
					Weight = 55
				}
			};

			var json = SerializationHelper.SerializeToJson(input);
			var copy = SerializationHelper.DeserializeFromJson<Test>(json);

			Assert.IsTrue(input.Value.Equals(copy.Value));

			copy.Value.Weight++;

			Assert.IsFalse(input.Value.Equals(copy.Value));
		}
	}
}
