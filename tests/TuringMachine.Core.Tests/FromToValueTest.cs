using NUnit.Framework;
using System;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Tests
{
	[TestFixture]
	public class FromToValueTest
	{
		[Test]
		public void TestExcludes()
		{
			var entry = new FromToValue<byte>(1, 3);
			entry.Excludes.Add(2);
			entry.Excludes.Add(3);

			Assert.AreEqual(1, entry.Get());
			Assert.AreEqual(1, entry.Get());
			Assert.AreEqual(1, entry.Get());
			Assert.AreEqual(1, entry.Get());

			Assert.IsFalse(entry.ItsValid(0));
			Assert.IsTrue(entry.ItsValid(1));
			Assert.IsFalse(entry.ItsValid(2));
			Assert.IsFalse(entry.ItsValid(3));
		}

		[Test]
		public void TestTypes()
		{
			GetAndValidTest<byte>(10, 20, 25);
			GetAndValidTest<int>(10, 20, 25);
			//GetAndValidTest<uint>(10, 20, 25);
			//GetAndValidTest<short>(10, 20, 25);
			GetAndValidTest<ushort>(10, 20, 25);
			GetAndValidTest<double>(10, 20, 25);
			GetAndValidTest<long>(10, 20, 25);
		}

		public void GetAndValidTest<T>(T from, T to, T invalid)
			where T : IComparable, IEquatable<T>, IComparable<T>
		{
			var entry = new FromToValue<T>
			{
				From = from,
				To = to
			};

			Assert.IsTrue(from.CompareTo(entry.Get()) <= 0);
			Assert.IsTrue(to.CompareTo(entry.Get()) >= 0);
			Assert.IsFalse(entry.ItsValid(invalid));
			Assert.IsTrue(entry.ItsValid(from));

			entry = new FromToValue<T>(from);

			Assert.AreEqual(from, entry.Get());
			Assert.IsFalse(entry.ItsValid(invalid));
			Assert.IsTrue(entry.ItsValid(from));
		}

		[Test]
		public void SerializationTest()
		{
			var entry = new FromToValue<byte>
			{
				From = 1,
				To = 2
			};

			entry.Excludes.AddRange(new byte[] { 1, 2 });

			var copy = new FromToValue<byte>(1, 2);

			copy.Excludes.AddRange(new byte[] { 1, 2 });

			// Test PatchChange Equals

			Assert.IsTrue(entry.Equals(copy));
			Assert.IsTrue(entry.Equals((object)copy));
			Assert.IsTrue(entry.Equals((IGetValue<byte>)copy));
			Assert.IsFalse(entry.Equals(new object()));
			Assert.IsFalse(entry.Equals((IGetValue<byte>)new FixedValue<byte>()));
			Assert.AreEqual(entry.GetHashCode(), copy.GetHashCode());

			copy.Excludes.Add(3);

			Assert.AreNotEqual(entry.GetHashCode(), copy.GetHashCode());

			// Test PatchConfig Equals

			var json = SerializationHelper.SerializeToJson(entry);
			copy = SerializationHelper.DeserializeFromJson<FromToValue<byte>>(json);

			Assert.IsTrue(copy.Equals(copy));
			Assert.IsTrue(copy.Equals((object)copy));
			Assert.IsTrue(copy.Equals((IGetValue<byte>)copy));
			Assert.IsFalse(copy.Equals(new object()));
			Assert.IsFalse(copy.Equals((IGetValue<byte>)new FixedValue<byte>()));
			Assert.AreEqual(copy.GetHashCode(), copy.GetHashCode());

			copy.Excludes.Clear();

			Assert.AreEqual(copy.GetHashCode(), copy.GetHashCode());
		}
	}
}
