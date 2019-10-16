using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Fuzzers.Mutational
{
    [DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
    public class MutationalChunk : IMutation, IEquatable<MutationalChunk>
    {
        /// <summary>
        /// Chunks
        /// </summary>
        public IList<byte[]> Allowed { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [JsonProperty(Order = 0)]
        public string Type => "Fixed";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Data</param>
        public MutationalChunk()
        {
            Allowed = new List<byte[]>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Data</param>
        public MutationalChunk(IEnumerable<byte[]> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Allowed = new List<byte[]>(data);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Data</param>
        public MutationalChunk(params string[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Allowed = new List<byte[]>(data.Select(u => Encoding.UTF8.GetBytes(u)));
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
                var chunk = RandomHelper.GetRandom(Allowed);
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

        public bool Equals(IMutation obj)
        {
            if (!(obj is MutationalChunk o)) return false;
            return Equals(o);
        }

        public bool Equals(MutationalChunk obj)
        {
            if (obj == null) return false;

            return obj.Type == Type
                && obj.Allowed.ChunkSequenceEqualWithNullCheck(Allowed);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = 1043142361;
            hashCode = hashCode * -1521134295 + Type.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + Allowed.GetHashCodeWithNullCheck();
            return hashCode;
        }
    }
}
