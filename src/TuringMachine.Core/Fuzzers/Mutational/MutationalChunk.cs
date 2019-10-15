using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TuringMachine.Core.Converters;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Fuzzers.Mutational
{
    [JsonConverter(typeof(MutationalChunkConverter))]
    [DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
    public class MutationalChunk : IMutation, IEquatable<MutationalChunk>
    {
        private readonly IList<byte[]> _chunks;

        [JsonIgnore]
        [ReadOnly(true)]
        public IEnumerable<byte[]> Chunks => _chunks.ToArray();

        /// <summary>
        /// Name
        /// </summary>
        [JsonIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        public string Type => "Fixed";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Data</param>
        public MutationalChunk(IEnumerable<byte[]> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            _chunks = new List<byte[]>(data);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Data</param>
        public MutationalChunk(params string[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            _chunks = new List<byte[]>(data.Select(u => Encoding.UTF8.GetBytes(u)));
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <returns>Get random chunk</returns>
        public byte[] GetChunk(int size)
        {
            // Iterate the chunks

            var data = new List<byte>();
            for (int x = 0; x < size; x++)
            {
                var chunk = RandomHelper.GetRandom(_chunks);
                if (chunk == null || chunk.Length == 0) continue;

                data.InsertRange(RandomHelper.GetRandom(0, data.Count), chunk);
            }

            return data.ToArray();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MutationalChunk o)) return false;
            return Equals(o);
        }

        public bool Equals(MutationalChunk obj)
        {
            if (obj == null) return false;

            return obj.Type == Type
                && SequenceEqual(obj._chunks, _chunks);
        }

        private bool SequenceEqual(IList<byte[]> chunks1, IList<byte[]> chunks2)
        {
            if ((chunks1 == null) != (chunks2 == null)) return false;
            if (chunks1 == null) return true;

            if (chunks1.Count != chunks2.Count)
            {
                return false;
            }

            for (int x = chunks1.Count - 1; x >= 0; x--)
            {
                if (!chunks1[x].SequenceEqual(chunks2[x]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = 1043142361;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + _chunks
                .Sum(u => (long)(u.Length < 4 ? u.Length : BitConverter.ToInt32(u, 0)))
                .GetHashCode();
            return hashCode;
        }
    }
}
