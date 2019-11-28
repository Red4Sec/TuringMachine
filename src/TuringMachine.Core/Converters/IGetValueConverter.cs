using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Converters
{
	internal class IGetValueConverter : JsonConverter
	{
		private static readonly Type[] _ExpectedTypes = new Type[]
		{
			typeof(IGetValue<double>),
			typeof(IGetValue<ushort>),
			typeof(IGetValue<long>)
		};

		public override bool CanConvert(Type objectType)
		{
			return Array.IndexOf(_ExpectedTypes, objectType) >= 0;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var jObject = JObject.Load(reader);
			var tar = objectType.GenericTypeArguments[0];

			if (jObject.Property("Type", StringComparison.InvariantCultureIgnoreCase) != null)
			{
				switch (jObject.GetValue("Type", StringComparison.InvariantCultureIgnoreCase).ToString())
				{
					case "From-To":
						{
							var type = typeof(FromToValue<>).MakeGenericType(tar);
							existingValue = jObject.ToObject(type, serializer);
							break;
						}
					case "Fixed":
						{
							var type = typeof(FixedValue<>).MakeGenericType(tar);
							existingValue = jObject.ToObject(type, serializer);
							break;
						}
				}
			}

			return existingValue;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var item = (IType)value;

			switch (item.Type)
			{
				case "Fixed":
				case "From-To":
					{
						serializer.Serialize(writer, item);
						break;
					}
			}
		}
	}
}
