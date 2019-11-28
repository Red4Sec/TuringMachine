using NUnit.Framework;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Tests.Helpers
{
    [TestFixture]
    public class HashHelperTest
    {
        [Test]
        public void Sha256Test()
        {
            var a = new byte[0];

            Assert.AreEqual("E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855", StringHelper.ByteArrayToHex(HashHelper.Sha256(a)));

            a = new byte[1] { 0x00 };

            Assert.AreEqual("6E340B9CFFB37A989CA544E6BB780A2C78901D3FB33738768511A30617AFA01D", StringHelper.ByteArrayToHex(HashHelper.Sha256(a)));
        }

        [Test]
        public void Md5Test()
        {
            var a = new byte[0];

            Assert.AreEqual("D41D8CD98F00B204E9800998ECF8427E", StringHelper.ByteArrayToHex(HashHelper.Md5(a)));

            a = new byte[1] { 0x00 };

            Assert.AreEqual("93B885ADFE0DA089CDF634904FD59F71", StringHelper.ByteArrayToHex(HashHelper.Md5(a)));
        }
    }
}
