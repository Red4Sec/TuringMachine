using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TuringMachine.Core.Fuzzers.Mutational;

namespace TuringMachine.Core.Converters
{
    internal class IMutationConverter : JsonConverter
    {
        private static readonly Type _ExpectedType = typeof(IMutation);

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
                    case "From-To":
                        {
                            existingValue = jObject.ToObject<MutationalFromTo>(serializer);
                            break;
                        }
                    case "Fixed":
                        {
                            existingValue = jObject.ToObject<MutationalChunk>(serializer);
                            break;
                        }
                }
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = (IMutation)value;

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
