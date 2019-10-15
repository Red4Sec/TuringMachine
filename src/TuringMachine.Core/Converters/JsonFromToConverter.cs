using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.ComponentModel;

namespace TuringMachine.Core.Converters
{
    internal class JsonFromToConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var tar = objectType.GenericTypeArguments[0];
            var conv = TypeDescriptor.GetConverter(tar);

            if (jObject.Property("Type") != null)
            {
                switch (jObject.GetValue("Type").ToString())
                {
                    case "From-To":
                        {
                            existingValue = Activator.CreateInstance(typeof(FromToValue<>).MakeGenericType(tar));
                            var type = existingValue.GetType();

                            if (jObject.Property("From") != null)
                            {
                                var value = conv.ConvertFromInvariantString(jObject.GetValue("From").ToString());
                                type.GetProperty("From").SetValue(existingValue, value);
                            }
                            if (jObject.Property("To") != null)
                            {
                                var value = conv.ConvertFromInvariantString(jObject.GetValue("To").ToString());
                                type.GetProperty("To").SetValue(existingValue, value);
                            }
                            if (jObject.Property("Excludes") != null)
                            {
                                var excludes = (IList)type.GetProperty("Excludes").GetValue(existingValue);
                                excludes.Clear();
                                foreach (object o in ((JArray)jObject.GetValue("Excludes")))
                                    excludes.Add(conv.ConvertFromInvariantString(o.ToString()));
                            }
                            break;
                        }
                    case "Fixed":
                        {
                            existingValue = Activator.CreateInstance(typeof(FixedValue<>).MakeGenericType(tar));
                            var type = existingValue.GetType();

                            if (jObject.Property("Allowed") != null)
                            {
                                var allowed = (IList)type.GetProperty("Allowed").GetValue(existingValue);
                                allowed.Clear();
                                foreach (object o in ((JArray)jObject.GetValue("Allowed")))
                                    allowed.Add(conv.ConvertFromInvariantString(o.ToString()));
                            }
                            break;
                        }
                }
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var type = value.GetType();
            var name = type.GetProperty("Type").GetValue(value).ToString();

            writer.WriteStartObject();
            writer.WritePropertyName("Type");
            writer.WriteValue(name);

            switch (name)
            {
                case "From-To":
                    {
                        writer.WritePropertyName("From");
                        writer.WriteValue(type.GetProperty("From").GetValue(value));
                        writer.WritePropertyName("To");
                        writer.WriteValue(type.GetProperty("To").GetValue(value));

                        var excludes = (IList)type.GetProperty("Excludes").GetValue(value);
                        if (excludes != null && excludes.Count > 0)
                        {
                            writer.WritePropertyName("Excludes");
                            writer.WriteStartArray();
                            foreach (var o in excludes)
                                writer.WriteValue(o);
                            writer.WriteEndArray();
                        }
                        break;
                    }
                case "Fixed":
                    {
                        var allowed = (IList)type.GetProperty("Allowed").GetValue(value);
                        if (allowed != null && allowed.Count > 0)
                        {
                            writer.WritePropertyName("Allowed");
                            writer.WriteStartArray();
                            foreach (var o in allowed)
                                writer.WriteValue(o);
                            writer.WriteEndArray();
                        }
                        break;
                    }
            }

            writer.WriteEndObject();
        }
    }
}
