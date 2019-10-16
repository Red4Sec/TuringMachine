using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using TuringMachine.Core.Extensions;
using TuringMachine.Core.Helpers;

namespace TuringMachine.Core.Logs
{
    [DebuggerDisplay(SerializationHelper.DebuggerDisplay)]
    public class FuzzerLog : IEquatable<FuzzerLog>
    {
        /// <summary>
        /// Input
        /// </summary>
        [JsonProperty("i")]
        public Guid InputId { get; set; }

        /// <summary>
        /// Config
        /// </summary>
        [JsonProperty("c")]
        public Guid ConfigId { get; set; }

        /// <summary>
        /// Coverage
        /// </summary>
        [JsonProperty("t")]
        public double Coverage { get; set; } = 0.0;

        /// <summary>
        /// Config
        /// </summary>
        [JsonProperty("e", NullValueHandling = NullValueHandling.Ignore)]
        public FuzzerError Error { get; set; }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public bool Equals(FuzzerLog obj)
        {
            if (obj == null) return false;

            return InputId == obj.InputId
                && ConfigId == obj.ConfigId
                && Coverage == obj.Coverage
                && ((Error == null && obj.Error == null) || Error.Equals(obj.Error));
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public override bool Equals(object obj)
        {
            if (obj is FuzzerLog o)
            {
                return Equals(o);
            }

            return false;
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = -1268746195;
            hashCode = hashCode * -1521134295 + InputId.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + ConfigId.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + Error.GetHashCodeWithNullCheck();
            hashCode = hashCode * -1521134295 + Coverage.GetHashCodeWithNullCheck();
            return hashCode;
        }

        /// <summary>
        /// Create from current file
        /// </summary>
        /// <param name="file">File</param>
        /// <param name="exception">Exception</param>
        /// <param name="output">Output</param>
        /// <returns>Log</returns>
        public static FuzzerLog FromCurrentFile(string file, Exception exception, string output)
        {
            var zip = new byte[0];
            ZipHelper.AppendOrCreateZip(ref zip, new ZipHelper.FileEntry()
            {
                FileName = Path.GetFileName(file),
                Data = File.ReadAllBytes(file)
            });

            if (!string.IsNullOrEmpty(output))
            {
                ZipHelper.AppendOrCreateZip(ref zip, new ZipHelper.FileEntry()
                {
                    FileName = "output.log",
                    Data = Encoding.UTF8.GetBytes(output)
                });
            }

            var inputId = Guid.Empty;
            var configId = Guid.Empty;

            try
            {
                var split = Path.GetFileNameWithoutExtension(file).Split('.');
                inputId = Guid.Parse(split[0]);
                configId = Guid.Parse(split[1]);
            }
            catch { }

            return new FuzzerLog()
            {
                InputId = inputId,
                ConfigId = configId,
                Coverage = CoverageHelper.CurrentCoverage,
                Error = new FuzzerError()
                {
                    Error = FuzzerError.EFuzzingErrorType.Crash,
                    ExplotationResult = FuzzerError.EExplotationResult.Unknown,
                    ErrorId = new Guid(HashHelper.Md5(Encoding.UTF8.GetBytes(exception.ToString()))),
                    ReplicationData = zip
                }
            };
        }
    }
}
