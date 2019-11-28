using NUnit.Framework;
using System;
using System.Text.RegularExpressions;
using TuringMachine.Core.Analyzers;
using TuringMachine.Core.Logs;

namespace TuringMachine.Core.Tests.Analyzers
{
	[TestFixture]
	public class DumpAnalyzerTest
	{
		[Test]
		public void StaticVarsTest()
		{
			Assert.IsNotNull(DumpAnalyzer.WinDbgAnalyzer);
		}

		[Test]
		public void CheckMemoryDumpEmptyTest()
		{
			string file, args;

			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				file = "touch";
				args = "%1";
			}
			else
			{
				file = "cmd";
				args = "/c \"\" > %1";
			}

			var dummy = new DumpAnalyzer
				(
				"", "",
				new Regex(@""),
				new Regex(@""),
				(i) => FuzzerError.EExplotationResult.Unknown
				);

			var res = dummy.CheckMemoryDump("", out var id, out var result);

			Assert.AreEqual("", res, $"Executed: '' ''");
			Assert.AreEqual(Guid.Empty, id);
			Assert.AreEqual(FuzzerError.EExplotationResult.Unknown, result);

			dummy = new DumpAnalyzer
			   (
			   file, args,
			   new Regex(@""),
			   new Regex(@""),
			   (i) => FuzzerError.EExplotationResult.Unknown
			   );

			res = dummy.CheckMemoryDump("", out id, out result);

			Assert.AreEqual("", res.Trim(), $"Executed: '{file}' '{args}'");
			Assert.AreEqual(Guid.Empty, id);
			Assert.AreEqual(FuzzerError.EExplotationResult.Unknown, result);
		}

		[Test]
		public void CheckMemoryDumpErrorTest()
		{
			var dummy = new DumpAnalyzer
				(
				"(XPPPXPPPXXPPXXPXP)", "",
				null,
				null,
				null
				);

			var res = dummy.CheckMemoryDump("", out var id, out var result);

			Assert.AreEqual("", res);
			Assert.AreEqual(Guid.Empty, id);
			Assert.AreEqual(FuzzerError.EExplotationResult.Unknown, result);
		}

		[Test]
		public void CheckMemoryDumpExitTimeoutTest()
		{
			string file, args;

			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				file = "bash";
				args = "-c \"sleep 2\"";
			}
			else
			{
				file = "cmd";
				args = "/c sleep 2";
			}

			var dummy = new DumpAnalyzer
				(
				file, args,
				null,
				null,
				null
				)
			{
				ExitTimeout = TimeSpan.FromMilliseconds(0),
			};

			var res = dummy.CheckMemoryDump("TESTDUMP", out var id, out var result);

			Assert.AreEqual("", res);
			Assert.AreEqual(Guid.Empty, id);
			Assert.AreEqual(FuzzerError.EExplotationResult.Unknown, result);
		}

		[Test]
		public void CheckMemoryDumpTest()
		{
			string file, args;

			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				file = "bash";
				args = "-c \"echo 123x%0x456 > %1\"";
			}
			else
			{
				file = "cmd";
				args = "/c echo 123x%0x456 > %1";
			}

			var dummy = new DumpAnalyzer
			   (
			   file, args,
			   new Regex("123x(?<value>.*)x456", RegexOptions.Singleline),
			   new Regex("123xTEST(?<value>.*)x456", RegexOptions.Singleline),
				(i) => i == "DUMP" ? FuzzerError.EExplotationResult.Exploitable : FuzzerError.EExplotationResult.Unknown
			   );

			var res1 = dummy.CheckMemoryDump("TESTDUMP", out var id1, out var result1);

			Assert.AreEqual("123xTESTDUMPx456", res1.Trim(), $"Executed: '{file}' '{args}'");
			Assert.AreNotEqual(Guid.Empty, id1);
			Assert.AreEqual(FuzzerError.EExplotationResult.Exploitable, result1);

			var res2 = dummy.CheckMemoryDump("TESTDUMP", out var id2, out var result2);

			Assert.AreEqual("123xTESTDUMPx456", res2.Trim(), $"Executed: '{file}' '{args}'");
			Assert.AreEqual(id1, id2);
			Assert.AreEqual(FuzzerError.EExplotationResult.Exploitable, result2);

			// WrongId

			dummy = new DumpAnalyzer
			   (
			   file, args,
			   new Regex("456x(?<value>.*)x456", RegexOptions.Singleline),
			   new Regex("456xTEST(?<value>.*)x456", RegexOptions.Singleline),
				(i) => i == "DUMP" ? FuzzerError.EExplotationResult.Exploitable : FuzzerError.EExplotationResult.Unknown
			   );

			var res3 = dummy.CheckMemoryDump("123", out var id3, out var result3);

			Assert.AreEqual("123x123x456", res3.Trim(), $"Executed: '{file}' '{args}'");
			Assert.AreNotEqual(Guid.Empty, id3);
			Assert.AreEqual(FuzzerError.EExplotationResult.Unknown, result3);
		}
	}
}
