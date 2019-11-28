using NUnit.Framework;
using TuringMachine.Core.Collections;
using TuringMachine.Core.Fuzzers.Mutational;

namespace TuringMachine.Core.Tests.Collections
{
	[TestFixture]
	public class WeightCollectionTest
	{
		[Test]
		public void TestEqual()
		{
			var collection = new WeightCollection<MutationalChange>
			(
				new MutationalChange() { Weight = 1, Description = "1" },
				new MutationalChange() { Weight = 2, Description = "2" }
			);

			var collectionCopy = new WeightCollection<MutationalChange>
			(
				new MutationalChange() { Weight = 1, Description = "1" },
				new MutationalChange() { Weight = 2, Description = "2" }
			);

			Assert.IsTrue(collection.Equals(collectionCopy));
			Assert.IsTrue(collection.Equals((object)collectionCopy));
			Assert.IsFalse(collection.Equals(null));
			Assert.AreEqual(collection.GetHashCode(), collectionCopy.GetHashCode());

			var collection2 = new WeightCollection<MutationalChange>
			(
				new MutationalChange() { Weight = 1, Description = "1" }
			);

			Assert.IsFalse(collection.Equals(collection2));
			Assert.IsFalse(collection.Equals((object)collection2));
			Assert.AreNotEqual(collection.GetHashCode(), collection2.GetHashCode());
		}
	}
}
