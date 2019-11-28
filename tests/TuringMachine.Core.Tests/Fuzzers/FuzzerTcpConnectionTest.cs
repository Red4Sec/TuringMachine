using NUnit.Framework;
using System.Net;
using TuringMachine.Core.Fuzzers;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Tests.Fuzzers
{
	[TestFixture]
	public class FuzzerTcpConnectionTest
	{
		[Test]
		public void EqualTest()
		{
			var value = new FuzzerTcpConnection()
			{
				EndPoint = new IPEndPoint(IPAddress.Any, 123)
			};

			var json = SerializationHelper.SerializeToJson(value, true);
			var copy = SerializationHelper.DeserializeFromJson<FuzzerTcpConnection>(json);

			Assert.IsTrue(value.Equals(copy));
			Assert.IsTrue(value.Equals((object)copy));
			Assert.IsFalse(value.Equals(new object()));
			Assert.AreEqual(value.GetHashCode(), copy.GetHashCode());

			copy.EndPoint = new IPEndPoint(IPAddress.Any, 1234);
			Assert.AreNotEqual(value.GetHashCode(), copy.GetHashCode());
		}
	}
}
