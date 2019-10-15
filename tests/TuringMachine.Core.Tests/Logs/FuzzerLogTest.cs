using NUnit.Framework;
using System;
using System.IO;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Logs;

namespace TuringMachine.Core.Tests.Logs
{
    [TestFixture]
    public class FuzzerLogTest
    {
        [Test]
        public void ErrorLogSave()
        {
            var log = new FuzzerError()
            {
                ErrorId = Guid.NewGuid(),
                Error = FuzzerError.EFuzzingErrorType.Crash,
                ExplotationResult = FuzzerError.EExplotationResult.Exploitable,
                ReplicationData = null,
            };

            var step = log.Error == FuzzerError.EFuzzingErrorType.Crash ? "crashes" : "failures";
            var nameUnique = $"{step}/{DateTime.UtcNow.ToString("yyyy-MM-dd")}/uniques/";
            var nameRegular = $"{step}/{DateTime.UtcNow.ToString("yyyy-MM-dd")}/";

            // Clean

            if (Directory.Exists(step))
            {
                Directory.Delete(step, true);
            }

            // Without data

            log.Save(true);

            Assert.IsFalse(Directory.Exists(nameUnique));
            Assert.IsFalse(Directory.Exists(nameRegular));

            // With data

            log.ReplicationData = new byte[] { 0x00, 0x01 };

            log.Save(true);

            Assert.AreEqual(1, Directory.GetFiles(nameUnique, "*.zip", SearchOption.AllDirectories).Length);
            Assert.AreEqual(0, Directory.GetFiles(nameRegular, "*.zip", SearchOption.TopDirectoryOnly).Length);

            log.Save(false);

            Assert.AreEqual(1, Directory.GetFiles(nameRegular, "*.zip", SearchOption.TopDirectoryOnly).Length);

            // Clean

            if (Directory.Exists(step))
            {
                Directory.Delete(step, true);
            }
        }

        [Test]
        public void ErrorLog()
        {
            var log = new FuzzerError()
            {
                ErrorId = Guid.NewGuid(),
                Error = FuzzerError.EFuzzingErrorType.Crash,
                ExplotationResult = FuzzerError.EExplotationResult.Exploitable,
                ReplicationData = new byte[1] { 0x00 },
            };

            var json = SerializationHelper.SerializeToJson(log, true);
            var copy = SerializationHelper.DeserializeFromJson<FuzzerError>(json);

            Assert.IsTrue(log.Equals(copy));
            Assert.IsTrue(log.Equals((object)copy));
            Assert.IsFalse(log.Equals(new object()));
            Assert.AreEqual(log.GetHashCode(), copy.GetHashCode());

            copy.ErrorId = Guid.NewGuid();
            Assert.AreNotEqual(log.GetHashCode(), copy.GetHashCode());
        }

        [Test]
        public void Log()
        {
            var log = new FuzzerLog()
            {
                ConfigId = Guid.NewGuid(),
                InputId = Guid.NewGuid(),
                Coverage = 100.12
            };

            var json = SerializationHelper.SerializeToJson(log, true);
            var copy = SerializationHelper.DeserializeFromJson<FuzzerLog>(json);

            Assert.IsTrue(log.Equals(copy));
            Assert.IsTrue(log.Equals((object)copy));
            Assert.IsFalse(log.Equals(new object()));
            Assert.AreEqual(log.GetHashCode(), copy.GetHashCode());

            copy.ConfigId = Guid.NewGuid();
            Assert.AreNotEqual(log.GetHashCode(), copy.GetHashCode());
        }
    }
}
