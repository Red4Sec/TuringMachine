using NUnit.Framework;
using System.Collections.Concurrent;
using TuringMachine.Core.Extensions;

namespace TuringMachine.Core.Tests.Extensions
{
    [TestFixture]
    public class ConcurrentExtensionsTest
    {
        [Test]
        public void ClearTest()
        {
            var concurrent = new ConcurrentBag<int>
            {
                1,
                2,
                3
            };

            Assert.AreEqual(3, concurrent.Count);
            ConcurrentExtensions.Clear(concurrent);
            Assert.AreEqual(0, concurrent.Count);
        }
    }
}
