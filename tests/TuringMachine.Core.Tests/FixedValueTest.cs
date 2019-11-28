using NUnit.Framework;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Tests
{
	[TestFixture]
	public class FixedValueTest
	{
		[Test]
		public void GetAndValidTest()
		{
			var entry = new FixedValue<byte>();

			Assert.AreEqual(0, entry.Get());
			Assert.IsFalse(entry.ItsValid(0));

			entry.Allowed.Add(1);

			Assert.AreEqual(1, entry.Get());
			Assert.IsFalse(entry.ItsValid(0));
			Assert.IsTrue(entry.ItsValid(1));
		}

		[Test]
		public void SerializationTest()
		{
			var entry = new FixedValue<byte>();

			entry.Allowed.Add(1);
			entry.Allowed.Add(2);

			var copy = new FixedValue<byte>(1, 2);

			// Test PatchChange Equals

			Assert.IsTrue(entry.Equals(copy));
			Assert.IsTrue(entry.Equals((object)copy));
			Assert.IsTrue(entry.Equals((IGetValue<byte>)copy));
			Assert.IsFalse(entry.Equals(new object()));
			Assert.IsFalse(entry.Equals((IGetValue<byte>)new FromToValue<byte>()));
			Assert.AreEqual(entry.GetHashCode(), copy.GetHashCode());

			copy.Allowed.Add(3);

			Assert.AreNotEqual(entry.GetHashCode(), copy.GetHashCode());

			// Test PatchConfig Equals

			var json = SerializationHelper.SerializeToJson(entry);
			copy = SerializationHelper.DeserializeFromJson<FixedValue<byte>>(json);

			Assert.IsTrue(copy.Equals(copy));
			Assert.IsTrue(copy.Equals((object)copy));
			Assert.IsTrue(copy.Equals((IGetValue<byte>)copy));
			Assert.IsFalse(copy.Equals(new object()));
			Assert.IsFalse(copy.Equals((IGetValue<byte>)new FromToValue<byte>()));
			Assert.AreEqual(copy.GetHashCode(), copy.GetHashCode());

			copy.Allowed.Clear();

			Assert.AreEqual(copy.GetHashCode(), copy.GetHashCode());
		}
	}
}
