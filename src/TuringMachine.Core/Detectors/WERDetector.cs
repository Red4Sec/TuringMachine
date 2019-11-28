using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TuringMachine.Core.Analyzers;
using TuringMachine.Core.Exceptions;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Logs;

namespace TuringMachine.Core.Detectors
{
    /// <summary>
    /// Only for Windows!
    /// 
    ///         LocalMode: https://msdn.microsoft.com/es-es/library/windows/desktop/bb787181(v=vs.85).aspx
    /// RegEdit (x32 x64): https://support.microsoft.com/en-us/kb/305097
    /// 
    ///             Windows Registry Editor Version 5.00
    ///             [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps]
    ///             "DumpType"=dword:00000002
    ///             "LocalDumps"=dword:00001000
    ///             "DumpFolder"="c:\\CrashDumps"
    ///             
    /// </summary>
    public class WERDetector : IDisposable
    {
        class ZipEntryEx
        {
            public string OriginalPath;
            public ZipHelper.FileEntry Entry;
        }

        private readonly string _storeLocation32;
        private readonly string _storeLocation64;

        private readonly string[] _FileNames;
        private readonly static string _CrashPath;

        public ProcessEx[] CreatedProcess { get; }

        /// <summary>
        /// Load CrashPath
        /// </summary>
        static WERDetector()
        {
            _CrashPath = @"%LOCALAPPDATA%\CrashDumps";

            try
            {
                // Read path
                using (var rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default))
                using (var r = rk.OpenSubKey(@"SOFTWARE\Microsoft\Windows\Windows Error Reporting\LocalDumps", false))
                {
                    if (r != null)
                    {
                        string v = r.GetValue("DumpFolder", _CrashPath).ToString();
                        if (!string.IsNullOrEmpty(v))
                            _CrashPath = v;
                    }
                }
            }
            catch { }

            _CrashPath = Environment.ExpandEnvironmentVariables(_CrashPath);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="startInfo">Process startInfo</param>
        public WERDetector(params ProcessStartInfoEx[] startInfo)
        {
            _storeLocation64 = GetStoreLocation(RegistryView.Registry64);
            _storeLocation32 = GetStoreLocation(RegistryView.Registry32);

            if (startInfo != null)
            {
                var files = new List<string>();
                var process = new List<ProcessEx>();

                foreach (var pi in startInfo)
                {
                    if (pi == null) continue;

                    var pid = 0;
                    var file = "";

                    if (!string.IsNullOrEmpty(pi.FileName))
                    {
                        file = Path.GetFileName(pi.FileName);
                        var p = new ProcessEx(pi);
                        pid = p.ProcessId;
                        process.Add(p);
                    }
                    else
                    {
                        throw new ArgumentException(nameof(pi.FileName));
                    }

                    if (!string.IsNullOrEmpty(file) && pid != 0 && pi.WaitMemoryDump)
                    {
                        files.Add(Path.Combine(_CrashPath, file + "." + pid.ToString() + ".dmp"));
                    }
                }

                _FileNames = files.ToArray();
                CreatedProcess = process.ToArray();
            }
        }

        /// <summary>
        /// Check if was changed the last WER store location
        /// </summary>
        /// <returns></returns>
        internal protected bool ItsChangedStoreLocation()
        {
            if (GetStoreLocation(RegistryView.Registry64) != _storeLocation64) return true;
            if (GetStoreLocation(RegistryView.Registry32) != _storeLocation32) return true;
            return false;
        }

        /// <summary>
        /// Check in Store location (its called first)
        /// </summary>
        static string GetStoreLocation(RegistryView v)
        {
            try
            {
                // Read path
                using (var rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, v))
                using (var r = rk.OpenSubKey(@"SOFTWARE\Microsoft\Windows\Windows Error Reporting\Debug", false))
                {
                    return r.GetValue("StoreLocation", "").ToString();
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Throw exception if is crashed
        /// </summary>
        /// <param name="isAlive">IsAlive</param>
        public void ThrowIfCrashed(Func<bool> isAlive)
        {
            if (IsCrashed(out var zip, out var result, isAlive, out var errorId))
            {
                throw new FuzzerException(errorId, zip, result);
            }
        }

        /// <summary>
        /// Return if are WER file
        /// </summary>
        /// <param name="zipCrashData">Crash data</param>
        /// <param name="exploitResult">Exploitation result</param>
        /// <param name="isAlive">IsAlive</param>
        /// <param name="errorId">Error Id</param>
        public bool IsCrashed(out byte[] zipCrashData, out FuzzerError.EExplotationResult exploitResult, Func<bool> isAlive, out Guid errorId)
        {
            zipCrashData = null;
            errorId = Guid.Empty;
            exploitResult = FuzzerError.EExplotationResult.Unknown;

            if (CreatedProcess == null || CreatedProcess.Length == 0)
            {
                return false;
            }

            // Wait for exit

            var isBreak = false;

            foreach (var p in CreatedProcess)
            {
                try
                {
                    p.WaitForExit(p.ExitTimeOut);
                }
                catch { }

                // Check store location for changes

                if (!isBreak && ItsChangedStoreLocation())
                {
                    isBreak = true;
                }
            }

            // Courtesy wait

            Thread.Sleep(250);

            // Search logs 

            var fileAppend = new List<ZipEntryEx>();

            if (_FileNames != null)
            {
                foreach (var f in _FileNames)
                {
                    if (ZipHelper.TryReadFile(f, TimeSpan.FromSeconds(isBreak ? 5 : 2), out var entry))
                    {
                        fileAppend.Add(new ZipEntryEx()
                        {
                            Entry = entry,
                            OriginalPath = f
                        });
                    }
                }
            }

            // If its alive kill them

            foreach (var p in CreatedProcess)
            {
                try { p.KillProcess(); } catch { }
            }

            // Compress to zip

            if (fileAppend.Count <= 0)
            {
                return false;
            }

            // Check exploitability

            for (int x = 0, m = fileAppend.Count; x < m; x++)
            {
                var dump = fileAppend[x];

                if (dump.OriginalPath.ToLowerInvariant().EndsWith(".dmp"))
                {
                    var l = DumpAnalyzer.WinDbgAnalyzer.CheckMemoryDump(dump.OriginalPath, out errorId, out exploitResult);

                    if (!string.IsNullOrEmpty(l))
                    {
                        fileAppend.Add(new ZipEntryEx()
                        {
                            Entry = new ZipHelper.FileEntry($"exploitable-{exploitResult.ToString().ToLowerInvariant()}.log", Encoding.UTF8.GetBytes(l))
                        });

                        break;
                    }
                }
            }

            // Compress information

            byte[] zip = null;
            if (ZipHelper.AppendOrCreateZip(ref zip, fileAppend.Select(u => u.Entry).ToArray()) > 0 && zip != null && zip.Length > 0)
            {
                zipCrashData = zip;
            }

            return true;
        }

        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {
            if (CreatedProcess != null)
            {
                foreach (var p in CreatedProcess)
                {
                    try { p.KillProcess(); } catch { }
                    try { p.Dispose(); } catch { }
                }
            }
        }
    }
}
