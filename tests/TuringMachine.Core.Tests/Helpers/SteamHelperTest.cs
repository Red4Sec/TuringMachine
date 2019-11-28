using NUnit.Framework;
using System.IO;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Tests.Helpers
{
    [TestFixture]
    public class SteamHelperTest
    {
        [Test]
        public void ReadWriteString()
        {
            var a = new MemoryStream();

            StreamHelper.WriteString(a, "Test");

            a.Position = 0;

            Assert.AreEqual("Test", StreamHelper.ReadString(a));

            Assert.Catch<IOException>(() => StreamHelper.ReadString(a));

            a.Position = 4;

            Assert.Catch<IOException>(() => StreamHelper.ReadString(a));

            a.Position = 0;
            a.SetLength(7);

            Assert.Catch<IOException>(() => StreamHelper.ReadString(a));
        }

        [Test]
        public void ReadFull()
        {
            var a = new MemoryStream();

            StreamHelper.WriteString(a, "Test");

            a.Position = 0;

            var data = StreamHelper.WriteString("Test");
            var read = new byte[data.Length];

            Assert.AreEqual(read.Length, StreamHelper.ReadFull(a, read, 0, read.Length));
            CollectionAssert.AreEqual(read, data);
        }
    }
}
