using NUnit.Framework;
using TuringMachine.Core.Fuzzers;

namespace TuringMachine.Core.Tests
{
	[TestFixture]
	public class CommandLineOptionsTest
	{
		[Test]
		public void TestServer()
		{
			var cn = CommandLineOptions.Parse(new string[] { "--server", "127.0.0.1,1234" });

			Assert.AreEqual("127.0.0.1,1234", cn.Server);

			var cx = cn.GetConnection();

			Assert.IsInstanceOf<FuzzerTcpConnection>(cx);

			var parsed = (FuzzerTcpConnection)cx;

			Assert.AreEqual(1234, parsed.EndPoint.Port);
			Assert.AreEqual("127.0.0.1", parsed.EndPoint.Address.ToString());
		}

		[Test]
		public void TestNamedPipe()
		{
			var cn = CommandLineOptions.Parse(new string[] { "--pipe", "pipeTest" });

			Assert.AreEqual("pipeTest", cn.PipeName);

			var cx = cn.GetConnection();

			Assert.IsInstanceOf<FuzzerNamedPipeConnection>(cx);

			var parsed = (FuzzerNamedPipeConnection)cx;

			Assert.AreEqual("pipeTest", parsed.PipeName);
		}
	}
}
