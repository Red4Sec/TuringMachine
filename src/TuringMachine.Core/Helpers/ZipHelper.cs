using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using TuringMachine.Core.Extensions;

namespace TuringMachine.Core.Helpers
{
    public class ZipHelper
    {
        [DebuggerDisplay("FileName={FileName}")]
        public class FileEntry : IEquatable<FileEntry>
        {
            /// <summary>
            /// FileName
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// Data
            /// </summary>
            public byte[] Data { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public FileEntry() { }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="fileName">FileName</param>
            /// <param name="data">Data</param>
            public FileEntry(string fileName, string data)
            {
                FileName = fileName;
                Data = Encoding.UTF8.GetBytes(data);
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="fileName">FileName</param>
            /// <param name="data">Data</param>
            public FileEntry(string fileName, byte[] data)
            {
                FileName = fileName;
                Data = data;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is FileEntry o))
                {
                    return false;
                }

                return Equals(o);
            }

            public bool Equals(FileEntry obj)
            {
                if (obj == null) return false;

                return obj.FileName == FileName
                    && obj.Data.SequenceEqual(Data);
            }

            /// <summary>
            /// GetHashCode
            /// </summary>
            /// <returns>Hash code</returns>
            public override int GetHashCode()
            {
                var hashCode = -972130872;
                hashCode = hashCode * -1521134295 + FileName.GetHashCodeWithNullCheck();
                hashCode = hashCode * -1521134295 + Data.GetHashCodeWithNullCheck();
                return hashCode;
            }
        }

        /// <summary>
        /// Try read the file
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="timeOut">Timeout</param>
        /// <param name="entry">Entry</param>
        public static bool TryReadFile(string path, TimeSpan timeOut, out FileEntry entry)
        {
            var data = ReadFileWaiting(path, timeOut);

            if (data != null)
            {
                entry = new FileEntry(Path.GetFileName(path), data);
                return true;
            }

            entry = null;
            return false;
        }

        /// <summary>
        /// Blocks until the file is not locked any more
        /// </summary>
        /// <param name="fullPath">Full path</param>
        /// <param name="ensureFileTimeout">Wait time if the file not exists</param>
        public static byte[] ReadFileWaiting(string fullPath, TimeSpan ensureFileTimeout)
        {
            double sec = ensureFileTimeout.TotalMilliseconds;

            while (true)
            {
                try
                {
                    // Attempt to open the file exclusively.
                    using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 100))
                    {
                        byte[] data = new byte[fs.Length];
                        StreamHelper.ReadFull(fs, data, 0, data.Length);
                        return data;
                    }
                }
                catch //(Exception ex)
                {
                    if (sec <= 0) return null;

                    // Wait for the lock to be released
                    Thread.Sleep(250);

                    if (!File.Exists(fullPath))
                        sec -= 250;
                }
            }
        }

        /// <summary>
        /// Append or create a Zip
        /// </summary>
        /// <param name="zip">Current zip</param>
        /// <param name="append">Append</param>
        public static int AppendOrCreateZip(ref byte[] zip, params FileEntry[] append)
        {
            bool currentIsZip = zip != null && zip.Length > 0;

            int dv = 0;
            using (var memoryStream = new MemoryStream())
            {
                // Append zip
                if (currentIsZip)
                {
                    memoryStream.Write(zip, 0, zip.Length);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                }

                // Append entries

                using (var archive = new ZipArchive(memoryStream, currentIsZip ? ZipArchiveMode.Update : ZipArchiveMode.Create, true))
                {
                    foreach (var f in append)
                    {
                        if (f.Data == null)
                        {
                            continue;
                        }

                        using (var entryStream = archive.CreateEntry(f.FileName).Open())
                        {
                            entryStream.Write(f.Data, 0, f.Data.Length);
                        }

                        dv++;
                    }
                }

                // Recover zip

                memoryStream.Seek(0, SeekOrigin.Begin);
                zip = memoryStream.ToArray();
            }

            return dv;
        }
    }
}
