using Newtonsoft.Json;
using NUnit.Framework;
using TuringMachine.Core.Converters;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Tests.Converters
{
	[TestFixture]
	public class IGetValueConverterTest
	{
		class Test
		{
			[JsonConverter(typeof(IGetValueConverter))]
			public IGetValue<byte> ValueA;

			[JsonConverter(typeof(IGetValueConverter))]
			public IGetValue<byte> ValueB;
		}

		[Test]
		public void TestSerialization()
		{
			var input = new Test()
			{
				ValueA = new FixedValue<byte>(5, 6),
				ValueB = new FromToValue<byte>(5, 6)
			};

			var json = SerializationHelper.SerializeToJson(input);
			var copy = SerializationHelper.DeserializeFromJson<Test>(json);

			Assert.IsTrue(input.ValueA.Equals(copy.ValueA));
			Assert.IsTrue(input.ValueB.Equals(copy.ValueB));

			copy.ValueA = new FixedValue<byte>(5, 7);
			copy.ValueB = new FromToValue<byte>(5, 7);

			Assert.IsFalse(input.ValueA.Equals(copy.ValueA));
			Assert.IsFalse(input.ValueB.Equals(copy.ValueB));
		}
	}
}
