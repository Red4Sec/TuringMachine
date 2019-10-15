using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TuringMachine.Core.Fuzzers.Mutational;

namespace TuringMachine.Core.Converters
{
    internal class MutationalChunkConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            if (jObject.Property("Type") != null)
            {
                switch (jObject.GetValue("Type").ToString())
                {
                    case "From-To":
                        {
                            byte from = byte.MinValue;
                            byte to = byte.MaxValue;

                            if (jObject.Property("From") != null)
                            {
                                from = byte.Parse(jObject.GetValue("From").ToString());
                            }
                            if (jObject.Property("To") != null)
                            {
                                to = byte.Parse(jObject.GetValue("To").ToString());
                            }

                            var ret = new MutationalFromTo(from, to);

                            if (jObject.Property("Excludes") != null)
                            {
                                var excludes = ret.Excludes;
                                excludes.Clear();
                                foreach (object o in ((JArray)jObject.GetValue("Excludes")))
                                    excludes.Add(byte.Parse(o.ToString()));
                            }

                            existingValue = ret;

                            break;
                        }
                    case "Fixed":
                        {
                            var chunks = new List<byte[]>();

                            if (jObject.Property("Allowed") != null)
                            {
                                foreach (object o in ((JArray)jObject.GetValue("Allowed")))
                                {
                                    try
                                    {
                                        byte[] value = Convert.FromBase64String(o.ToString());
                                        chunks.Add(value);
                                    }
                                    catch
                                    {
                                        byte[] value = new byte[] { byte.Parse(o.ToString()) };
                                        chunks.Add(value);
                                    }
                                }
                            }

                            existingValue = new MutationalChunk(chunks);
                            break;
                        }
                }
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var item = (IMutation)value;

            writer.WriteStartObject();
            writer.WritePropertyName("Type");
            writer.WriteValue(item.Type);

            switch (item.Type)
            {
                case "From-To":
                    {
                        var fromTo = (MutationalFromTo)item;

                        writer.WritePropertyName("From");
                        writer.WriteValue(fromTo.From);
                        writer.WritePropertyName("To");
                        writer.WriteValue(fromTo.To);

                        if (fromTo.Excludes != null && fromTo.Excludes.Count > 0)
                        {
                            writer.WritePropertyName("Excludes");
                            writer.WriteStartArray();
                            foreach (var o in fromTo.Excludes)
                                writer.WriteValue(o);
                            writer.WriteEndArray();
                        }
                        break;
                    }
                case "Fixed":
                    {
                        var chunk = (MutationalChunk)item;
                        var chunks = chunk.Chunks?.ToArray();

                        if (chunks != null && chunks.Count() > 0)
                        {
                            writer.WritePropertyName("Allowed");
                            writer.WriteStartArray();
                            foreach (var o in chunks)
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
