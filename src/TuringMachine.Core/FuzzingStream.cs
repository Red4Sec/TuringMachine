using System;
using System.Collections.Generic;
using System.IO;
using TuringMachine.Core.Fuzzers.Patch;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core
{
    public class FuzzingStream : Stream
    {
        private long _lastRix = -1L;
        private bool _readedAll;
        private long _realOffset;
        private readonly Stream _source;
        private readonly List<byte> _buffer;
        private readonly MemoryStream _original;
        private readonly List<PatchChange> _log;
        private readonly Stream _storeCurrentStream;

        /// <summary>
        /// Fuzzing conditions
        /// </summary>
        public FuzzingConfigBase Config { get; private set; }

        /// <summary>
        /// Current Stored Stream
        /// </summary>
        public Stream CurrentStream => _storeCurrentStream;

        /// <summary>
        /// Variables
        /// </summary>
        public IDictionary<int, object> Variables { get; }

        /// <summary>
        /// Readed
        /// </summary>
        public byte[] OriginalData => _original.ToArray();

        /// <summary>
        /// Log
        /// </summary>
        public PatchChange[] Log => _log.ToArray();

        /// <summary>
        /// Input name
        /// </summary>
        public Guid InputId { get; set; }

        /// <summary>
        /// Config name
        /// </summary>
        public Guid ConfigId { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Mutations</param>
        /// <param name="stream">Stream</param>
        /// <param name="storeCurrentStream">Store current stream</param>
        public FuzzingStream(FuzzingConfigBase config, FuzzingInputBase stream, Stream storeCurrentStream = null) :
            this(config, stream == null ? throw new ArgumentNullException(nameof(stream)) : stream.GetStream(), storeCurrentStream)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            InputId = stream.Id;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Mutations</param>
        /// <param name="stream">Stream</param>
        /// <param name="storeCurrentStream">Store current stream</param>
        public FuzzingStream(FuzzingConfigBase config, byte[] stream, Stream storeCurrentStream = null) :
            this(config, stream == null ? throw new ArgumentNullException(nameof(stream)) : new MemoryStream(stream), storeCurrentStream)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Mutations</param>
        /// <param name="stream">Stream</param>
        /// <param name="storeCurrentStream">Store current stream</param>
        public FuzzingStream(FuzzingConfigBase config, Stream stream, Stream storeCurrentStream = null)
        {
            _source = stream ?? throw new ArgumentNullException(nameof(stream));
            Config = config;

            _realOffset = 0;
            _storeCurrentStream = storeCurrentStream;
            _buffer = new List<byte>();
            _original = new MemoryStream();
            _log = new List<PatchChange>();
            Variables = new Dictionary<int, object>();

            if (Config != null)
            {
                ConfigId = config.Id;
                Config.InitFor(this);
            }
        }

        /// <summary>
        /// Generate Zip
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="error">Exception</param>
        /// <returns>Zip File</returns>
        public byte[] GenerateZip(byte[] zip, string error)
        {
            var name = Guid.NewGuid().ToString();

            ZipHelper.AppendOrCreateZip(ref zip,
                new ZipHelper.FileEntry(name + ".error", error),
                new ZipHelper.FileEntry(name + ".input", OriginalData),
                new ZipHelper.FileEntry(name + ".data", Apply(OriginalData, Log)),
                new ZipHelper.FileEntry(name + ".fpatch", SerializationHelper.SerializeToJson(new PatchConfig("Patch", Log), true))
            );

            return zip;
        }

        /// <summary>
        /// Apply patch
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="logs">Logs</param>
        /// <returns>Data result</returns>
        private static byte[] Apply(byte[] data, PatchChange[] logs)
        {
            var cfg = new PatchConfig("", logs);

            using (var mem = new MemoryStream())
            using (var stream = new FuzzingStream(cfg, data))
            {
                stream.CopyTo(mem, 1024);
                return mem.ToArray();
            }
        }

        #region Read

        public override bool CanRead { get => _source.CanRead; }

        public override bool CanSeek { get => _source.CanSeek; }

        public override long Length { get => _source.Length; }

        public override int ReadTimeout
        {
            get => _source.ReadTimeout;
            set { _source.ReadTimeout = value; }
        }

        public override bool CanTimeout { get => _source.CanTimeout; }

        public override long Position
        {
            get => _realOffset;
            set
            {
                _source.Position = value;
                _realOffset = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var ret = ReadInternal(buffer, offset, count);

            if (_storeCurrentStream != null && ret > 0)
            {
                // Store current stream

                _storeCurrentStream.Write(buffer, offset, ret);
                _storeCurrentStream.Flush();
            }

            return ret;
        }

        private int ReadInternal(byte[] buffer, int offset, int count)
        {
            if (count <= 0 || _readedAll)
            {
                return 0;
            }

            if (Config == null)
            {
                // Without fuzzing
                return ReadFromOriginalStream(buffer, offset, count);
            }

            // Read buffer first

            var readed = ReadFromBuffer(buffer, ref offset, ref count);

            // Perform mutations (byte peer byte)
            while (count > 0)
            {
                PatchChange log = null;

                // If no buffer are available (FUZZ!)
                if (_buffer.Count == 0 && _lastRix != _realOffset)
                {
                    _lastRix = _realOffset;
                    log = Config.Get(this);
                }

                // If change!
                if (log != null)
                {
                    // Add to log

                    _log.Add(log);

                    // Add to buffer

                    if (log.Append != null)
                    {
                        _buffer.AddRange(log.Append);
                    }

                    // Remove from source

                    if (log.Remove > 0)
                    {
                        var removeBuffer = new byte[log.Remove];
                        var ret = ReadFromOriginalStream(removeBuffer, 0, log.Remove);
                        if (ret <= 0)
                        {
                            break;
                        }
                    }
                }

                // Read buffer first (if available)
                var d = ReadFromBuffer(buffer, ref offset, ref count);
                if (d <= 0)
                {
                    // Peek one byte if not from buffer
                    d = ReadFromOriginalStream(buffer, offset, 1);
                    if (d <= 0)
                    {
                        break;
                    }
                    offset += d;
                    count -= d;
                }

                readed += d;
            }

            return readed;
        }

        /// <summary>
        /// Read from mutated buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        int ReadFromBuffer(byte[] buffer, ref int offset, ref int count)
        {
            var readed = Math.Min(count, _buffer.Count);

            // Buffer empty or count <= 0

            if (readed <= 0)
            {
                return 0;
            }

            for (var x = 0; x < readed; x++)
            {
                buffer[offset + x] = _buffer[x];
            }

            // Move pointers

            count -= readed;
            offset += readed;

            // Remove from buffer

            _buffer.RemoveRange(0, readed);
            return readed;
        }

        /// <summary>
        /// Read from original stream
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        /// <returns></returns>
        int ReadFromOriginalStream(byte[] buffer, int offset, int count)
        {
            // Try read from original

            var lee = StreamHelper.ReadFull(_source, buffer, offset, count);

            if (lee <= 0)
            {
                _readedAll = true;
                return 0;
            }
            else
            {
                _original.Write(buffer, offset, lee);
                _realOffset += lee;
                return lee;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _realOffset = _source.Seek(offset, origin);
        }

        #endregion

        #region Write

        public override int WriteTimeout
        {
            get => _source.WriteTimeout;
            set { _source.WriteTimeout = value; }
        }

        public override bool CanWrite { get { return _source.CanWrite; } }

        public override void Flush() { _source.Flush(); }

        public override void SetLength(long value)
        {
            _realOffset = value;
            _source.SetLength(value);
        }

        public void AppendToSource(byte[] buffer, int offset, int count, bool reSeek)
        {
            long ps = 0;
            if (reSeek) ps = _source.Position;
            _source.Write(buffer, offset, count);
            if (reSeek) _source.Position = ps;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count <= 0) return;

            _realOffset += count;
            //_Original.Write(buffer, offset , count);
            //if (!_FuzzWrite)
            //{
            //    _Source.Write(buffer, offset, count);
            //    return;
            //}
        }

        #endregion

        #region Privates

        public override void Close()
        {
            _source.Close();
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _source.Dispose();
            _original.Dispose();

            foreach (var a in Variables.Values)
            {
                if (a is IDisposable d)
                {
                    d.Dispose();
                }
            }

            Variables.Clear();
        }

        #endregion
    }
}
