using NUnit.Framework;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Tests.Helpers
{
    [TestFixture]
    public class StringHelperTest
    {
        [Test]
        public void ByteArrayToHexTest()
        {
            var a = new byte[0];

            Assert.AreEqual("", StringHelper.ByteArrayToHex(a));

            a = new byte[2] { 0x00, 0xFF };

            Assert.AreEqual("00FF", StringHelper.ByteArrayToHex(a));
        }
    }
}
