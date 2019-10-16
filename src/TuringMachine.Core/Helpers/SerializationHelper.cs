using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TuringMachine.Core.Converters;
using TuringMachine.Core.Inputs;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Helpers
{
    public static class SerializationHelper
    {
        internal const string DebuggerDisplay = "{TuringMachine.Core.Helpers.SerializationHelper.SerializeToJson(this,false)}";

        private readonly static JsonSerializerSettings _Settings;

        static SerializationHelper()
        {
            _Settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            };
            _Settings.Converters.Add(new IPEndPointConverter());
            _Settings.Converters.Add(new FuzzingConfigBaseConverter());
            _Settings.Converters.Add(new FuzzingInputBaseConverter());
            _Settings.Converters.Add(new GetValueConverter());
            _Settings.Converters.Add(new ChunkFilterConverter());
            _Settings.Converters.Add(new IMutationalConverter());
        }

        /// <summary>
        /// Serializa un objeto a un json
        /// </summary>
        /// <param name="data">Datos</param>
        /// <param name="indented">Indented</param>
        public static string SerializeToJson(object data, bool indented = false)
        {
            if (data == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(data, indented ? Formatting.Indented : Formatting.None, _Settings);
        }

        /// <summary>
        /// Deserializa un json
        /// </summary>
        /// <typeparam name="T">Tipo</typeparam>
        /// <param name="json">Json</param>
        public static T DeserializeFromJson<T>(string json)
        {
            if (json == null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(json, _Settings);
        }

        /// <summary>
        /// Deserializa un json
        /// </summary>
        /// <typeparam name="T">Tipo</typeparam>
        /// <param name="json">Json</param>
        public static object DeserializeFromJson(string json, Type type)
        {
            if (json == null || type == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject(json, type, _Settings);
        }

        /// <summary>
        /// Get File content
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="file">File</param>
        /// <returns>T</returns>
        public static IEnumerable<T> DeserializeFromFile<T>(string file)
        {
            var content = File.ReadAllText(file);

            try
            {
                // Try collections

                var array = DeserializeFromJson<T[]>(content);
                array = array.Where(t => t != null).ToArray();

                if (array.Length > 0)
                {
                    return array;
                }
            }
            catch
            {
                try
                {
                    // Try single

                    var item = DeserializeFromJson<T>(content);
                    if (item != null)
                    {
                        return new T[] { item };
                    }
                }
                catch { }

                if (typeof(T) == typeof(FuzzingInputBase))
                {
                    // Use file as valid input

                    return new T[]
                    {
                        (T)(IIdentificable) new ManualFuzzingInput(File.ReadAllBytes(file))
                        {
                            Description = Path.GetFileName(file)
                        }
                    };
                }

                // No more tries
            }

            return new T[] { };
        }
    }
}
