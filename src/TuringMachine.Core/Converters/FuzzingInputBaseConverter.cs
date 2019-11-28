using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TuringMachine.Core.Inputs;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Converters
{
	public class FuzzingInputBaseConverter : JsonConverter
	{
		private static readonly Type _ExpectedType = typeof(FuzzingInputBase);

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
					case "Execution":
						{
							existingValue = obj.ToObject<ExecutionFuzzingInput>(serializer);
							break;
						}
					case "File":
						{
							existingValue = obj.ToObject<FileFuzzingInput>(serializer);
							break;
						}
					case "Manual":
						{
							existingValue = obj.ToObject<ManualFuzzingInput>(serializer);
							break;
						}
					case "Random":
						{
							existingValue = obj.ToObject<RandomFuzzingInput>(serializer);
							break;
						}
					case "TcpRequest":
						{
							existingValue = obj.ToObject<TcpQueryFuzzingInput>(serializer);
							break;
						}
				}
			}

			return existingValue;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var item = (FuzzingInputBase)value;
			serializer.Serialize(writer, item);
		}
	}
}
