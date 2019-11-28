using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Text;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Inputs;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Tests.Inputs
{
	[TestFixture]
	public class InputTests
	{
		[Test]
		public void RandomFuzzingInput()
		{
			var a = new RandomFuzzingInput(new FromToValue<long>(3))
			{
				Description = "Test",
				Id = Guid.NewGuid()
			};
			var b = new RandomFuzzingInput(new FromToValue<long>(3))
			{
				Description = "Test",
				Id = a.Id
			};

			var retA = a.GetStream();
			var retB = b.GetStream();

			CollectionAssert.AreNotEqual(retA, retB);

			// Regular constructor

			a = new RandomFuzzingInput() { Length = new FromToValue<long>(3) };
			b = new RandomFuzzingInput() { Length = new FromToValue<long>(3) };

			retA = a.GetStream();
			retB = b.GetStream();

			CollectionAssert.AreNotEqual(retA, retB);

			// Serialize

			var value = a;
			var json = SerializationHelper.SerializeToJson(value, true);
			var copy = SerializationHelper.DeserializeFromJson<RandomFuzzingInput>(json);
			var copy2 = SerializationHelper.DeserializeFromJson<FuzzingInputBase>(json);

			Assert.IsTrue(copy.Equals(copy2));

			// Equals

			Assert.IsTrue(value.Equals(copy));
			Assert.IsTrue(value.Equals((object)copy));
			Assert.IsFalse(value.Equals(new object()));
			Assert.IsFalse(value.Equals((FuzzingInputBase)new FileFuzzingInput()));
			Assert.AreEqual(value.GetHashCode(), copy.GetHashCode());

			value.Id = Guid.NewGuid();
			Assert.AreNotEqual(value.GetHashCode(), copy.GetHashCode());
		}

		[Test]
		public void ManualFuzzingInput()
		{
			var value = new ManualFuzzingInput(new byte[] { 0x00 })
			{
				Description = "Test",
				Id = Guid.NewGuid()
			};

			var ret = value.GetStream();
			CollectionAssert.AreEqual(new byte[] { 0x00 }, ret);

			// Regular constructor

			value = new ManualFuzzingInput(new byte[0]);
			ret = value.GetStream();

			Assert.AreEqual(0, ret.Length);

			// Serialize

			var json = SerializationHelper.SerializeToJson(value, true);
			var copy = SerializationHelper.DeserializeFromJson<ManualFuzzingInput>(json);
			var copy2 = SerializationHelper.DeserializeFromJson<FuzzingInputBase>(json);

			Assert.IsTrue(copy.Equals(copy2));

			// Equals

			Assert.IsTrue(value.Equals(copy));
			Assert.IsTrue(value.Equals((object)copy));
			Assert.IsFalse(value.Equals(new object()));
			Assert.IsFalse(value.Equals((FuzzingInputBase)new FileFuzzingInput()));
			Assert.AreEqual(value.GetHashCode(), copy.GetHashCode());

			value.Id = Guid.NewGuid();
			Assert.AreNotEqual(value.GetHashCode(), copy.GetHashCode());
		}

		[Test]
		public void ExecutionFuzzingInput()
		{
			ExecutionFuzzingInput value;

			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				value = new ExecutionFuzzingInput("bash", "-c \"echo Hello World\"")
				{
					Description = "Test",
					Id = Guid.NewGuid()
				};
			}
			else
			{
				value = new ExecutionFuzzingInput("cmd", "/c echo Hello World")
				{
					Description = "Test",
					Id = Guid.NewGuid()
				};
			}

			var ret = Encoding.ASCII.GetString(value.GetStream());
			Assert.AreEqual("Hello World", ret.Trim());

			// Regular constructor

			value = new ExecutionFuzzingInput()
			{
				FileName = value.FileName,
				Arguments = value.Arguments
			};

			ret = Encoding.ASCII.GetString(value.GetStream());
			Assert.AreEqual("Hello World", ret.Trim());

			// Serialize

			var json = SerializationHelper.SerializeToJson(value, true);
			var copy = SerializationHelper.DeserializeFromJson<ExecutionFuzzingInput>(json);
			var copy2 = SerializationHelper.DeserializeFromJson<FuzzingInputBase>(json);

			Assert.IsTrue(copy.Equals(copy2));

			// Equals

			Assert.IsTrue(value.Equals(copy));
			Assert.IsTrue(value.Equals((object)copy));
			Assert.IsFalse(value.Equals(new object()));
			Assert.IsFalse(value.Equals((FuzzingInputBase)new FileFuzzingInput()));
			Assert.AreEqual(value.GetHashCode(), copy.GetHashCode());

			value.Id = Guid.NewGuid();
			Assert.AreNotEqual(value.GetHashCode(), copy.GetHashCode());
		}

		[Test]
		public void FileFuzzingInput()
		{
			var value = new FileFuzzingInput("Samples//PatchSample.fpatch")
			{
				Description = "Test",
				Id = Guid.NewGuid()
			};

			CollectionAssert.AreEqual(File.ReadAllBytes("Samples//PatchSample.fpatch"), value.GetStream());

			// Regular constructor

			value = new FileFuzzingInput()
			{
				FileName = value.FileName
			};

			CollectionAssert.AreEqual(File.ReadAllBytes("Samples//PatchSample.fpatch"), value.GetStream());

			// Cache it

			value.UseCache = true;

			CollectionAssert.AreEqual(File.ReadAllBytes("Samples//PatchSample.fpatch"), value.GetStream());

			// Serialize

			var json = SerializationHelper.SerializeToJson(value, true);
			var copy = SerializationHelper.DeserializeFromJson<FileFuzzingInput>(json);
			var copy2 = SerializationHelper.DeserializeFromJson<FuzzingInputBase>(json);

			Assert.IsTrue(copy.Equals(copy2));

			// Equals

			Assert.IsTrue(value.Equals(copy));
			Assert.IsTrue(value.Equals((object)copy));
			Assert.IsFalse(value.Equals(new object()));
			Assert.IsFalse(value.Equals((FuzzingInputBase)new RandomFuzzingInput()));
			Assert.AreEqual(value.GetHashCode(), copy.GetHashCode());

			value.Id = Guid.NewGuid();
			Assert.AreNotEqual(value.GetHashCode(), copy.GetHashCode());
		}

		[Test]
		public void TcpQueryFuzzingInput()
		{
			var value = new TcpQueryFuzzingInput(new IPEndPoint(IPAddress.Parse("216.58.211.35"), 80),
				Encoding.ASCII.GetBytes(
@"GET / HTTP/1.1
Host: www.google.es
Connection: close
Upgrade-Insecure-Requests: 1
User-Agent: Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
Accept-Encoding: gzip, deflate, sdch
Accept-Language: en-US,en;q=0.8,es;q=0.6


"))
			{
				Description = "Test",
				Id = Guid.NewGuid()
			};

			Assert.IsTrue(Encoding.ASCII.GetString(value.GetStream()).StartsWith("HTTP/1.1 "));

			// Regular constructor

			value = new TcpQueryFuzzingInput()
			{
				EndPoint = value.EndPoint,
				Request = value.Request,
				Description = "Test",
				Id = Guid.NewGuid()
			};

			Assert.IsTrue(Encoding.ASCII.GetString(value.GetStream()).StartsWith("HTTP/1.1 "));

			// Serialize

			var json = SerializationHelper.SerializeToJson(value, true);
			var copy = SerializationHelper.DeserializeFromJson<TcpQueryFuzzingInput>(json);
			var copy2 = SerializationHelper.DeserializeFromJson<FuzzingInputBase>(json);

			Assert.IsTrue(copy.Equals(copy2));

			// Equals

			Assert.IsTrue(value.Equals(copy));
			Assert.IsTrue(value.Equals((object)copy));
			Assert.IsFalse(value.Equals(new object()));
			Assert.IsFalse(value.Equals((FuzzingInputBase)new RandomFuzzingInput()));
			Assert.AreEqual(value.GetHashCode(), copy.GetHashCode());

			value.Id = Guid.NewGuid();
			Assert.AreNotEqual(value.GetHashCode(), copy.GetHashCode());
		}
	}
}
