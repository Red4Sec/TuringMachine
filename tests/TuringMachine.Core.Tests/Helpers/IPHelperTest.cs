using NUnit.Framework;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Tests.Helpers
{
    [TestFixture]
    public class IPHelperTest
    {
        [Test]
        public void ToIpEndPointTest()
        {
            var a = IPHelper.ToIpEndPoint("127.0.0.1,123");

            Assert.AreEqual("127.0.0.1", a.Address.ToString());
            Assert.AreEqual(123, a.Port);
        }
    }
}
