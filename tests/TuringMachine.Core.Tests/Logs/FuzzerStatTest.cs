using NUnit.Framework;
using TuringMachine.Core.Fuzzers.Patch;
using TuringMachine.Core.Logs;

namespace TuringMachine.Core.Tests.Logs
{
    [TestFixture]
    public class FuzzerStatTest
    {
        [Test]
        public void Equals()
        {
            var cfg = new PatchConfig();
            var stat = new FuzzerStat<PatchConfig>(cfg);
            var copy = new FuzzerStat<PatchConfig>(stat.Source);

            Assert.IsTrue(stat.Equals(copy));
            Assert.IsTrue(stat.Equals((object)copy));
            Assert.IsFalse(stat.Equals(new object()));
            Assert.AreEqual(stat.GetHashCode(), copy.GetHashCode());

            stat.Crashes++;

            Assert.AreNotEqual(stat.GetHashCode(), copy.GetHashCode());
        }

        [Test]
        public void IncrementTest()
        {
            var cfg = new PatchConfig();
            var stat = new FuzzerStat<PatchConfig>(cfg);

            Assert.AreEqual(cfg, stat.Source);
            Assert.AreEqual(cfg.Description, stat.Description);
            Assert.AreEqual(cfg.Id, stat.Id);

            Assert.AreEqual(0, stat.Crashes);
            Assert.AreEqual(0, stat.Tests);
            Assert.AreEqual(0, stat.Errors);

            stat.Increment();

            Assert.AreEqual(0, stat.Crashes);
            Assert.AreEqual(1, stat.Tests);
            Assert.AreEqual(0, stat.Errors);

            stat.Increment(FuzzerError.EFuzzingErrorType.Crash);

            Assert.AreEqual(1, stat.Crashes);
            Assert.AreEqual(2, stat.Tests);
            Assert.AreEqual(0, stat.Errors);

            stat.Increment(FuzzerError.EFuzzingErrorType.Fail);

            Assert.AreEqual(1, stat.Crashes);
            Assert.AreEqual(3, stat.Tests);
            Assert.AreEqual(1, stat.Errors);

            stat.Reset();

            Assert.AreEqual(0, stat.Crashes);
            Assert.AreEqual(0, stat.Tests);
            Assert.AreEqual(0, stat.Errors);
        }
    }
}
