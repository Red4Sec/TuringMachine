using NUnit.Framework;
using TuringMachine.Core.Fuzzers;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Tests.Fuzzers
{
    [TestFixture]
    public class FuzzerPipeConnectionTest
    {
        [Test]
        public void EqualTest()
        {
            var value = new FuzzerNamedPipeConnection()
            {
                PipeName = "read",
            };

            var json = SerializationHelper.SerializeToJson(value, true);
            var copy = SerializationHelper.DeserializeFromJson<FuzzerNamedPipeConnection>(json);

            Assert.IsTrue(value.Equals(copy));
            Assert.IsTrue(value.Equals((object)copy));
            Assert.IsFalse(value.Equals(new object()));
            Assert.AreEqual(value.GetHashCode(), copy.GetHashCode());

            copy.PipeName += copy.PipeName;
            Assert.AreNotEqual(value.GetHashCode(), copy.GetHashCode());
        }
    }
}
