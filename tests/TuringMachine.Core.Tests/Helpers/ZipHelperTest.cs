using NUnit.Framework;
using System;
using System.IO;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Tests.Helpers
{
    [TestFixture]
    public class ZipHelperTest
    {
        [Test]
        public void FileEntryTest()
        {
            var entryA = new ZipHelper.FileEntry("a", new byte[1] { (byte)'A' });
            var entryB = new ZipHelper.FileEntry("a", "A");

            Assert.IsTrue(entryA.Equals(entryB));

            // Serialize

            var value = entryA;
            var json = SerializationHelper.SerializeToJson(value, true);
            var copy = SerializationHelper.DeserializeFromJson<ZipHelper.FileEntry>(json);

            // Equals

            Assert.IsTrue(value.Equals(copy));
            Assert.IsTrue(value.Equals((object)copy));
            Assert.IsFalse(value.Equals(new object()));
            Assert.AreEqual(value.GetHashCode(), copy.GetHashCode());

            value.FileName += "A";
            Assert.AreNotEqual(value.GetHashCode(), copy.GetHashCode());
        }

        [Test]
        public void TryReadFile()
        {
            var file = Path.GetTempFileName();
            if (File.Exists(file)) File.Delete(file);

            var ret = ZipHelper.TryReadFile(file, TimeSpan.FromMilliseconds(100), out var entry);

            Assert.IsFalse(ret);
            Assert.IsNull(entry);

            File.WriteAllBytes(file, new byte[1] { 0xAA });

            ret = ZipHelper.TryReadFile(file, TimeSpan.FromMilliseconds(100), out entry);

            Assert.IsTrue(ret);

            Assert.AreEqual(Path.GetFileName(file), entry.FileName);
            CollectionAssert.AreEqual(File.ReadAllBytes(file), entry.Data);

            File.Delete(file);
        }

        [Test]
        public void AppendOrCreateTest()
        {
            var entryA = new ZipHelper.FileEntry("a", new byte[1] { (byte)'A' });
            var entryB = new ZipHelper.FileEntry("a", "A");
            var entryC = new ZipHelper.FileEntry("a", (byte[])null);

            var data = new byte[0];
            var ret = ZipHelper.AppendOrCreateZip(ref data, entryA, entryB, entryC);

            Assert.AreEqual(2, ret);
            Assert.IsTrue(data.Length > 0);

            var entryD = new ZipHelper.FileEntry("b", "b");
            ret = ZipHelper.AppendOrCreateZip(ref data, entryD);
            Assert.AreEqual(1, ret);
        }
    }
}
