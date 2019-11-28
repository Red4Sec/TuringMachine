using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TuringMachine.Core.Fuzzers.Mutational;
using TuringMachine.Core.Fuzzers.Patch;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Converters
{
	internal class FuzzingConfigBaseConverter : JsonConverter
	{
		private static readonly Type _ExpectedType = typeof(FuzzingConfigBase);

		public override bool CanConvert(Type objectType)
		{
			return objectType == _ExpectedType;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var obj = JObject.Load(reader);

			if (obj.Property("Type", StringComparison.InvariantCultureIgnoreCase) != null)
			{
				switch (obj.GetValue("Type", StringComparison.InvariantCultureIgnoreCase).ToString())
				{
					case "Patch":
						{
							existingValue = obj.ToObject<PatchConfig>(serializer);
							break;
						}
					case "Mutational":
						{
							existingValue = obj.ToObject<MutationConfig>(serializer);
							break;
						}
				}
			}

			return existingValue;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var item = (FuzzingConfigBase)value;
			serializer.Serialize(writer, item);
		}
	}
}
