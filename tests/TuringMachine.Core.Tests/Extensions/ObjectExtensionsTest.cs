using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using TuringMachine.Core.Extensions;

namespace TuringMachine.Core.Tests.Extensions
{
    [TestFixture]
    public class ObjectExtensionsTest
    {
        [Test]
        public void ClearTest()
        {
            // IPEndPoint

            Assert.IsFalse(ObjectExtensions.EqualWithNullCheck(new IPEndPoint(0, 1), null));
            Assert.IsFalse(ObjectExtensions.EqualWithNullCheck(null, new IPEndPoint(0, 1)));
            Assert.IsTrue(ObjectExtensions.EqualWithNullCheck(new IPEndPoint(0, 1), new IPEndPoint(0, 1)));
            Assert.IsFalse(ObjectExtensions.EqualWithNullCheck(new IPEndPoint(0, 1), new IPEndPoint(0, 2)));

            // Byte[]

            Assert.IsFalse(ObjectExtensions.SequenceEqualWithNullCheck(new byte[] { 0x00, 0x01 }, null));
            Assert.IsFalse(ObjectExtensions.SequenceEqualWithNullCheck(null, new byte[] { 0x00, 0x01 }));
            Assert.IsTrue(ObjectExtensions.SequenceEqualWithNullCheck(new byte[] { 0x00, 0x01 }, new byte[] { 0x00, 0x01 }));
            Assert.IsFalse(ObjectExtensions.SequenceEqualWithNullCheck(new byte[] { 0x00, 0x01 }, new byte[] { 0x00, 0x02 }));

            // IList

            Assert.IsFalse(ObjectExtensions.SequenceEqualWithNullCheck(new List<byte>(new byte[] { 0x00, 0x01 }), null));
            Assert.IsFalse(ObjectExtensions.SequenceEqualWithNullCheck(null, new List<byte>(new byte[] { 0x00, 0x01 })));
            Assert.IsTrue(ObjectExtensions.SequenceEqualWithNullCheck(new List<byte>(new byte[] { 0x00, 0x01 }), new List<byte>(new byte[] { 0x00, 0x01 })));
            Assert.IsFalse(ObjectExtensions.SequenceEqualWithNullCheck(new List<byte>(new byte[] { 0x00, 0x01 }), new List<byte>(new byte[] { 0x00, 0x02 })));

            // IEquatable

            Assert.IsFalse(ObjectExtensions.EqualWithNullCheck((IEquatable<Decimal>)new Decimal(1), null));
            Assert.IsFalse(ObjectExtensions.EqualWithNullCheck(null, (IEquatable<Decimal>)new Decimal(1)));
            Assert.IsTrue(ObjectExtensions.EqualWithNullCheck((IEquatable<Decimal>)new Decimal(1), (IEquatable<Decimal>)new Decimal(1)));
            Assert.IsFalse(ObjectExtensions.EqualWithNullCheck((IEquatable<Decimal>)new Decimal(1), (IEquatable<Decimal>)new Decimal(2)));

            // IList<byte[]>

            Assert.IsFalse(ObjectExtensions.SequenceEqualWithNullCheck(new List<byte[]>(new byte[][] { new byte[] { 0x00, 0x01 } }), null));
            Assert.IsFalse(ObjectExtensions.SequenceEqualWithNullCheck(null, new List<byte[]>(new byte[][] { new byte[] { 0x00, 0x01 } })));
            Assert.IsTrue(ObjectExtensions.SequenceEqualWithNullCheck(new List<byte[]>(new byte[][] { new byte[] { 0x00, 0x01 } }), new List<byte[]>(new byte[][] { new byte[] { 0x00, 0x01 } })));
            Assert.IsFalse(ObjectExtensions.SequenceEqualWithNullCheck(new List<byte[]>(new byte[][] { new byte[] { 0x00, 0x01 } }), new List<byte[]>(new byte[][] { new byte[] { 0x00, 0x02 } })));
        }
    }
}
