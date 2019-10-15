using NUnit.Framework;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Tests.Helpers
{
    [TestFixture]
    public class BitHelperTest
    {
        [Test]
        public void Int32Test()
        {
            var a = BitHelper.GetBytes(123);

            CollectionAssert.AreEqual(new byte[] { 123, 0, 0, 0 }, a);

            var b = BitHelper.ToInt32(a, 0);

            Assert.AreEqual(123, b);
        }
    }
}
