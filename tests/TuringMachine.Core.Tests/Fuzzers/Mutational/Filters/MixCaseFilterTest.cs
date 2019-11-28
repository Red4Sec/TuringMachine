using NUnit.Framework;
using System.Text;
using TuringMachine.Core.Fuzzers.Mutational.Filters;

namespace TuringMachine.Core.Tests.Fuzzers.Mutational.Filters
{
	[TestFixture]
	public class MixCaseFilterTest
	{
		[Test]
		public void ToUpperNullTest()
		{
			var entry = new MixCaseFilter()
			{
				FilterPercent = null,
				MixType = MixCaseFilter.MixCaseType.ToUpperCase,
				Weight = 1
			};

			var input = "qwertyuiopasdfghjklzxcvbnm";
			var output = Encoding.UTF8.GetString(entry.ApplyFilter(Encoding.UTF8.GetBytes(input)));

			Assert.AreEqual(input.ToUpperInvariant(), output);
		}

		[Test]
		public void ToUpperPercentTest()
		{
			var entry = new MixCaseFilter()
			{
				FilterPercent = new FixedValue<double>(10),
				MixType = MixCaseFilter.MixCaseType.ToUpperCase,
				Weight = 1
			};

			var input = "".PadLeft(100, 'a');
			var output = Encoding.UTF8.GetString(entry.ApplyFilter(Encoding.UTF8.GetBytes(input)));

			output = output.Replace("a", "");

			Assert.IsTrue(output.Length > 1 && output.Length < 100);
		}

		[Test]
		public void ToLowerNullTest()
		{
			var entry = new MixCaseFilter()
			{
				FilterPercent = null,
				MixType = MixCaseFilter.MixCaseType.ToLowerCase,
				Weight = 1
			};

			var input = "QWERTYUIOPASDFGHJLZXCVBNM";
			var output = Encoding.UTF8.GetString(entry.ApplyFilter(Encoding.UTF8.GetBytes(input)));

			Assert.AreEqual(input.ToLowerInvariant(), output);
		}

		[Test]
		public void ToLowerPercentTest()
		{
			var entry = new MixCaseFilter()
			{
				FilterPercent = new FixedValue<double>(10),
				MixType = MixCaseFilter.MixCaseType.ToLowerCase,
				Weight = 1
			};

			var input = "".PadLeft(100, 'A');
			var output = Encoding.UTF8.GetString(entry.ApplyFilter(Encoding.UTF8.GetBytes(input)));

			output = output.Replace("A", "");

			Assert.IsTrue(output.Length > 1 && output.Length < 100);
		}

		[Test]
		public void ChangeCaseNullTest()
		{
			var entry = new MixCaseFilter()
			{
				FilterPercent = null,
				MixType = MixCaseFilter.MixCaseType.ChangeCase,
				Weight = 1
			};

			var input = "".PadLeft(100, 'A') + "".PadLeft(100, 'b');
			var output = Encoding.UTF8.GetString(entry.ApplyFilter(Encoding.UTF8.GetBytes(input)));

			output = output.Replace("A", "");
			output = output.Replace("b", "");

			Assert.IsTrue(output.Length == 200);
		}

		[Test]
		public void ChangeCasePercentTest()
		{
			var entry = new MixCaseFilter()
			{
				FilterPercent = new FixedValue<double>(10),
				MixType = MixCaseFilter.MixCaseType.ToLowerCase,
				Weight = 1
			};

			var input = "".PadLeft(100, 'A') + "".PadLeft(100, 'b');
			var output = Encoding.UTF8.GetString(entry.ApplyFilter(Encoding.UTF8.GetBytes(input)));

			output = output.Replace("A", "");
			output = output.Replace("b", "");

			Assert.IsTrue(output.Length > 1 && output.Length < 200);
		}
	}
}
