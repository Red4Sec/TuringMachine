using System.Diagnostics;
using System.Text;

namespace TuringMachine.Core.Detectors
{
    public class ProcessStartInfoEx
    {
        #region Public fields

        /// <summary>
        /// When exit process, wait for memory dump
        /// </summary>
        public bool WaitMemoryDump { get; set; } = false;

        #endregion

        #region Base

        public string FileName { get; set; } = "";
        public string Arguments { get; set; } = "";
        public string WorkingDirectory { get; set; } = "";
        public bool CreateNoWindow { get; set; } = true;
        public bool UseShellExecute { get; set; } = false;
        public bool RedirectStandardError { get; set; } = false;
        public bool RedirectStandardInput { get; set; } = false;
        public bool RedirectStandardOutput { get; set; } = false;
        public Encoding StandardErrorEncoding { get; set; } = null;
        public Encoding StandardOutputEncoding { get; set; } = null;
        public ProcessWindowStyle WindowStyle { get; set; } = ProcessWindowStyle.Hidden;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessStartInfoEx() : this("", "") { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="arguments">Arguments</param>
        public ProcessStartInfoEx(string fileName, string arguments = "")
        {
            FileName = fileName;
            Arguments = arguments;

            CreateNoWindow = true;
            WindowStyle = ProcessWindowStyle.Hidden;
            UseShellExecute = false;
            WaitMemoryDump = true;
        }

        /// <summary>
        /// Get process start info
        /// </summary>
        /// <returns>ProcessStartInfo</returns>
        public ProcessStartInfo GetProcessStartInfo()
        {
            return new ProcessStartInfo()
            {
                FileName = FileName,
                Arguments = Arguments,
                WindowStyle = WindowStyle,
                CreateNoWindow = CreateNoWindow,
                UseShellExecute = UseShellExecute,
                WorkingDirectory = WorkingDirectory,
                RedirectStandardError = RedirectStandardError,
                RedirectStandardInput = RedirectStandardInput,
                RedirectStandardOutput = RedirectStandardOutput,
                StandardErrorEncoding = StandardErrorEncoding,
                StandardOutputEncoding = StandardOutputEncoding,
            };
        }
    }
}
