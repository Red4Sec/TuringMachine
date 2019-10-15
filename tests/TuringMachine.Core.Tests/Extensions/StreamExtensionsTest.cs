using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using TuringMachine.Core.Extensions;

namespace TuringMachine.Core.Tests.Extensions
{
    [TestFixture]
    public class StreamExtensionsTest
    {
        [Test]
        public void ToArrayTest()
        {
            var ms = new MemoryStream(new byte[] { 0x01 });
            var arr = StreamExtensions.ToArray(ms);

            CollectionAssert.AreEqual(new byte[] { 0x01 }, arr);
        }

        [Test]
        public void ConnectoAndCopyToTest()
        {
            var ms = new MemoryStream(new byte[] { 0x01 });

            Assert.IsTrue(ms.ConnectoAndCopyTo(new IPEndPoint(IPAddress.Parse("216.58.211.35"), 80), TimeSpan.FromSeconds(10)));

            ms.Position = 0;

            Assert.IsFalse(ms.ConnectoAndCopyTo(new IPEndPoint(IPAddress.Parse("216.58.211.35"), 1), TimeSpan.FromMilliseconds(1)));
        }
    }
}
