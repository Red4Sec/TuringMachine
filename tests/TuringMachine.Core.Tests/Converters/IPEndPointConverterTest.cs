using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;
using TuringMachine.Core.Converters;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Tests.Converters
{
	[TestFixture]
	public class IPEndPointConverterTest
	{
		class Test
		{
			[JsonConverter(typeof(IPEndPointConverter))]
			public IPEndPoint Value;
		}

		[Test]
		public void TestSerialization()
		{
			var input = new Test()
			{
				Value = new IPEndPoint(IPAddress.Loopback, 1)
			};

			var json = SerializationHelper.SerializeToJson(input);
			var copy = SerializationHelper.DeserializeFromJson<Test>(json);

			Assert.IsTrue(input.Value.Equals(copy.Value));

			copy.Value = new IPEndPoint(IPAddress.Loopback, 12);

			Assert.IsFalse(input.Value.Equals(copy.Value));
		}
	}
}
