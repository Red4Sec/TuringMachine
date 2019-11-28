using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Converters
{
    internal class IPEndPointConverter : JsonConverter
    {
        private static readonly Type _ExpectedType = typeof(IPEndPoint);

        public override bool CanConvert(Type objectType)
        {
            return objectType == _ExpectedType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var ep = (IPEndPoint)value;
            writer.WriteValue(ep.Address.ToString() + "," + ep.Port.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            return token.Value<string>().ToIpEndPoint();
        }
    }
}
