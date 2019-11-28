using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TuringMachine.Core.Fuzzers.Mutational;
using TuringMachine.Core.Fuzzers.Mutational.Filters;

namespace TuringMachine.Core.Converters
{
    internal class IChunkFilterConverter : JsonConverter
    {
        private static readonly Type _ExpectedType = typeof(IChunkFilter);

        public override bool CanConvert(Type objectType)
        {
            return objectType == _ExpectedType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            if (jObject.Property("Type") != null)
            {
                switch (jObject.GetValue("Type").ToString())
                {
                    case "MixCase":
                        {
                            existingValue = jObject.ToObject<MixCaseFilter>(serializer);
                            break;
                        }
                }
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = (IChunkFilter)value;

            switch (item.Type)
            {
                case "MixCase":
                    {
                        serializer.Serialize(writer, (MixCaseFilter)item);
                        break;
                    }
            }
        }
    }
}
