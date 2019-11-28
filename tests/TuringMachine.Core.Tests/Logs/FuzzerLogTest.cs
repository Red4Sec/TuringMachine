using NUnit.Framework;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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

		[Test]
		public void FromCurrentFileError()
		{
			var fileName = "X.X.current";
			CoverageHelper.CurrentCoverage = 12.34;

			using (var file = File.Create(fileName))
			{
				file.Write(new byte[] { 0x01, 0x02, 0x03 });
				file.Close();
			}

			var log = FuzzerLog.FromCurrentFile(fileName, new Exception("Test"), "log123");
			File.Delete(fileName);

			Assert.AreEqual(Guid.Empty, log.InputId);
			Assert.AreEqual(Guid.Empty, log.ConfigId);
			Assert.AreEqual(12.34D, log.Coverage);

			CheckLogError(fileName, log.Error);
		}

		[Test]
		public void FromCurrentFile()
		{
			var inputId = Guid.NewGuid();
			var configId = Guid.NewGuid();
			var fileName = inputId.ToString() + "." + configId.ToString() + ".current";
			CoverageHelper.CurrentCoverage = 12.34;

			using (var file = File.Create(fileName))
			{
				file.Write(new byte[] { 0x01, 0x02, 0x03 });
				file.Close();
			}

			var log = FuzzerLog.FromCurrentFile(fileName, new Exception("Test"), "log123");
			File.Delete(fileName);

			Assert.AreEqual(inputId, log.InputId);
			Assert.AreEqual(configId, log.ConfigId);
			Assert.AreEqual(12.34D, log.Coverage);

			CheckLogError(fileName, log.Error);
		}

		private void CheckLogError(string fileName, FuzzerError error)
		{
			Assert.NotNull(error);

			using (var stream = new MemoryStream(error.ReplicationData))
			using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
			{
				// Log

				var entry = archive.Entries.Where(u => u.Name == "output.log").FirstOrDefault();
				Assert.NotNull(entry);

				using (var streamEntry = entry.Open())
				{
					var data = new byte[10];
					Array.Resize(ref data, StreamHelper.ReadFull(streamEntry, data, 0, data.Length));

					CollectionAssert.AreEqual(data, Encoding.UTF8.GetBytes("log123"));
				}

				// current

				entry = archive.Entries.Where(u => u.Name == Path.GetFileName(fileName)).FirstOrDefault();
				Assert.NotNull(entry);

				using (var streamEntry = entry.Open())
				{
					var data = new byte[10];
					Array.Resize(ref data, StreamHelper.ReadFull(streamEntry, data, 0, data.Length));

					CollectionAssert.AreEqual(data, new byte[] { 0x01, 0x02, 0x03 });
				}
			}
		}
	}
}
