using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TuringMachine.Core.Detectors;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Logs;

namespace TuringMachine.Core.Analyzers
{
    public class DumpAnalyzer
    {
        /// <summary>
        /// Only for Windows!
        /// 
        /// Config at: 
        ///     http://insidetrust.blogspot.com.es/2011/02/assessing-buffer-overflows-with-windbg.html
        /// WindDbg:
        ///     https://developer.microsoft.com/es-es/windows/downloads/windows-10-sdk
        ///     copy to C:\Program Files (x86)\Windows Kits\10\Debuggers\x64\winext
        /// !exploitable
        ///     http://msecdbg.codeplex.com/
        /// </summary>
        public static readonly DumpAnalyzer WinDbgAnalyzer = new DumpAnalyzer
            (
            @"C:\Program Files (x86)\Windows Kits\10\Debuggers\x64\windbg.exe",
            "-z \"%0\" -c \"!load winext\\msec.dll;!exploitable;!analyze -v;q\" -logo \"%1\"",
            new Regex(@"PROBLEM_CLASSES:(?<value>.*)LAST_CONTROL_TRANSFER", RegexOptions.Multiline | RegexOptions.Singleline),
            new Regex(@"Exploitability Classification\:\s+(?<value>.*)", RegexOptions.Multiline),
            (i) =>
                {
                    if (!Enum.TryParse<FuzzerError.EExplotationResult>(i, true, out var ret))
                    {
                        return FuzzerError.EExplotationResult.Unknown;
                    }

                    return ret;
                }
            );

        /// <summary>
        /// FileName
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Arguments
        /// </summary>
        public string Arguments { get; }

        /// <summary>
        /// Search this string in the output
        /// </summary>
        private readonly Regex _exploitability;

        /// <summary>
        /// Error id content
        /// </summary>
        private readonly Regex _errorId;

        /// <summary>
        /// Parser
        /// </summary>
        private readonly Func<string, FuzzerError.EExplotationResult> _parser;

        /// <summary>
        /// Exit Timeout
        /// </summary>
        public TimeSpan ExitTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="arguments"></param>
        /// <param name="errorId"></param>
        /// <param name="exploitability"></param>
        /// <param name="parser">Parser</param>
        public DumpAnalyzer(string fileName, string arguments, Regex errorId, Regex exploitability, Func<string, FuzzerError.EExplotationResult> parser)
        {
            FileName = fileName;
            Arguments = arguments;
            _errorId = errorId;
            _exploitability = exploitability;
            _parser = parser;
        }

        /// <summary>
        /// Check dump Path
        /// </summary>
        /// <param name="dump">Dump</param>
        /// <param name="errorId">Error id</param>
        /// <param name="explotationResult">Explotation Result</param>
        public string CheckMemoryDump(string dump, out Guid errorId, out FuzzerError.EExplotationResult explotationResult)
        {
            errorId = Guid.Empty;
            explotationResult = FuzzerError.EExplotationResult.Unknown;

            if (string.IsNullOrEmpty(FileName))
            {
                return "";
            }

            var logFile = "";
            var file = dump + ".~explog";

            try
            {
                using (var p = new ProcessEx(new ProcessStartInfoEx()
                {
                    FileName = FileName,
                    Arguments = Arguments.Replace("%0", dump).Replace("%1", file),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }))
                {
                    if (!p.WaitForExit(ExitTimeout))
                    {
                        return "";
                    }
                }

                if (File.Exists(file))
                {
                    logFile = File.ReadAllText(file);
                    File.Delete(file);
                }
            }
            catch
            {
                return "";
            }

            if (string.IsNullOrEmpty(logFile))
            {
                return "";
            }

            // Exploitability

            var match = _parser != null && _exploitability != null ? _exploitability.Match(logFile) : null;

            if (match != null && match.Success)
            {
                explotationResult = _parser(match.Groups["value"].Value);
            }

            // ErrorId

            match = _errorId?.Match(logFile);

            if (match != null && match.Success)
            {
                errorId = new Guid(HashHelper.Md5(Encoding.UTF8.GetBytes(match.Groups["value"].Value)));
            }
            else
            {
                errorId = Guid.NewGuid();
            }

            return logFile;
        }
    }
}
