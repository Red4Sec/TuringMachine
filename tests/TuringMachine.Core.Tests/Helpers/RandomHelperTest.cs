using NUnit.Framework;
using System.Linq;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Tests.Helpers
{
    [TestFixture]
    public class RandomHelperTest
    {
        [Test]
        public void FillWithRandomBytesTest()
        {
            var data = new byte[32];
            RandomHelper.FillWithRandomBytes(data);
            Assert.IsTrue(data.Distinct().Count() > 20);
        }

        [Test]
        public void Int32Test()
        {
            var a = RandomHelper.GetRandom((int)1, (int)1);

            Assert.AreEqual((int)1, a);

            bool isA = false, isB = false;

            for (int x = 0; x < 200 && !(isA && isB); x++)
            {
                a = RandomHelper.GetRandom((int)1, (int)2);

                if (a == 1) isA = true;
                else if (a == 2) isB = true;
            }

            Assert.IsTrue(isA && isB);
        }

        [Test]
        public void Int64Test()
        {
            var a = RandomHelper.GetRandom((long)1, (long)1);

            Assert.AreEqual((long)1, a);

            bool isA = false, isB = false;

            for (int x = 0; x < 200 && !(isA && isB); x++)
            {
                a = RandomHelper.GetRandom((long)1, (long)2);

                if (a == 1) isA = true;
                else if (a == 2) isB = true;
            }

            Assert.IsTrue(isA && isB);
        }

        [Test]
        public void DoubleTest()
        {
            var a = RandomHelper.GetRandom((double)1, (double)1);

            Assert.AreEqual((double)1, a);

            for (int x = 0; x < 200; x++)
            {
                a = RandomHelper.GetRandom((double)1, (double)2);
                Assert.IsTrue(a >= 1 && a <= 2);
            }
        }

        [Test]
        public void CollectionTest()
        {
            var list = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 }.ToHashSet();
            var asserts = new bool[list.Count];

            for (int x = 0; x < 1000 && !asserts.All(u => u); x++)
            {
                asserts[RandomHelper.GetRandom(list)] = true;
            }

            Assert.IsTrue(asserts.All(u => u));
            Assert.AreEqual((byte)0, RandomHelper.GetRandom(new byte[0].ToHashSet()));
        }

        [Test]
        public void IsRandomPercentOkTest()
        {
            Assert.IsTrue(RandomHelper.IsRandomPercentOk(101));
            Assert.IsTrue(RandomHelper.IsRandomPercentOk(100));
            Assert.IsFalse(RandomHelper.IsRandomPercentOk(0));
            Assert.IsFalse(RandomHelper.IsRandomPercentOk(-1));

            bool isA = false, isB = false;

            for (int x = 0; x < 200 && !(isA && isB); x++)
            {
                var a = RandomHelper.IsRandomPercentOk(50);

                if (a) isA = true;
                else isB = true;
            }

            Assert.IsTrue(isA && isB);
        }

        [Test]
        public void GetRandomTest()
        {
            var list = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            var asserts = new bool[list.Length];

            for (int x = 0; x < 1000 && !asserts.All(u => u); x++)
            {
                asserts[RandomHelper.GetRandom(list)] = true;
            }

            Assert.IsTrue(asserts.All(u => u));
            Assert.AreEqual((byte)0, RandomHelper.GetRandom(new byte[0]));
        }

        [Test]
        public void RandomizeTest()
        {
            var fromTo = new FromToValue<byte>(1, 2);
            var data = new byte[200];

            RandomHelper.Randomize(data, 0, data.Length, fromTo);

            Assert.IsTrue(data.Any(u => u == 1));
            Assert.IsTrue(data.Any(u => u == 2));
        }
    }
}
