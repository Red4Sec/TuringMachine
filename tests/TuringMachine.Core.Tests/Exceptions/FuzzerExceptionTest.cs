using NUnit.Framework;
using System;
using TuringMachine.Core.Exceptions;
using TuringMachine.Core.Logs;

namespace TuringMachine.Core.Tests.Exceptions
{
	[TestFixture]
	public class FuzzerExceptionTest
	{
		[Test]
		public void SerializeTest()
		{
			var id = Guid.NewGuid();
			var data = new FuzzerException(id, new byte[] { 0x01, 0x02 }, FuzzerError.EExplotationResult.Exploitable);

			Assert.AreEqual(id, data.ErrorId);
			Assert.AreEqual(FuzzerError.EExplotationResult.Exploitable, data.Result);
			CollectionAssert.AreEqual(new byte[] { 0x01, 0x02 }, data.Zip);
		}
	}
}
