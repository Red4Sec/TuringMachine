using Coverlet.Core;
using Coverlet.Core.Abstracts;
using Coverlet.Core.Reporters;
using Palmmedia.ReportGenerator.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

//namespace Coverlet.Core.Instrumentation.XXX.Tracker
//{
//    public static class Payload_Test
//    {
//        public static int[] HitsArray = new int[0];
//    }
//}

namespace TuringMachine.Core.Helpers
{
    /// <summary>
    /// Coverage helper
    /// </summary>
    public static class CoverageHelper
    {
        class CoverletLogger : Coverlet.Core.Logging.ILogger
        {
            public void LogError(string message) => Console.WriteLine(message);
            public void LogError(Exception exception) => Console.WriteLine(exception.ToString());
            public void LogInformation(string message, bool important = false) => Console.WriteLine(message);
            public void LogVerbose(string message) => Console.WriteLine(message);
            public void LogWarning(string message) => Console.WriteLine(message);
        }

        private static double _currentCoverage = 0;

        /// <summary>
        /// Current Coverage
        /// </summary>
        public static double CurrentCoverage
        {
            get => _currentCoverage;
            set
            {
                if (value < 0) value = 0;
                else if (value > 100) value = 100;

                _currentCoverage = value;
            }
        }

        /// <summary>
        /// Is instrumented
        /// </summary>
        public static bool IsInstrumented => _hitsArray.Length > 0;

        /// <summary>
        /// Hits array length
        /// </summary>
        private static readonly int _hitsArrayLength;

        /// <summary>
        /// Hits arrays
        /// </summary>
        private static readonly int[][] _hitsArray;

        /// <summary>
        /// Unload modules
        /// </summary>
        private static readonly MethodInfo[] _unloadModule;

        /// <summary>
        /// Logger
        /// </summary>
        private static readonly CoverletLogger _logger = new CoverletLogger();

        /// <summary>
        /// Coverage task
        /// </summary>
        private static Task _coverageTask;

        /// <summary>
        /// Constructor
        /// </summary>
        static CoverageHelper()
        {
            // Find instrumentation payload

            var hitsArrays = new List<int[]>();
            var unloadModule = new List<MethodInfo>();
            var regex = new Regex(@"Coverlet\.Core\.Instrumentation\.Tracker\..*\.Payload_\.*");

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes())
                {
                    if (regex.IsMatch(type.FullName))
                    {
                        var field = type.GetField("HitsArray", BindingFlags.Public | BindingFlags.Static);
                        if (field != null)
                        {
                            hitsArrays.Add((int[])field.GetValue(null));
                        }

                        var method = type.GetMethod("UnloadModule", BindingFlags.Public | BindingFlags.Static);
                        if (method != null)
                        {
                            unloadModule.Add(method);
                        }
                    }
                }
            }

            _hitsArray = hitsArrays.ToArray();
            _unloadModule = unloadModule.ToArray();

            // Compute length

            _hitsArrayLength = 0;

            foreach (var hits in _hitsArray)
            {
                _hitsArrayLength += hits.Length;
            }
        }

        /// <summary>
        /// Create coverage Listener if need
        /// </summary>
        public static void CreateCoverageListener()
        {
            if (_coverageTask != null)
            {
                return;
            }

            // Start coverage listener

            _coverageTask = InternalCreateCoverageListener();

            if (_coverageTask != Task.CompletedTask)
            {
                _coverageTask.Start();
            }
            else if (!IsInstrumented)
            {
                Console.Error.WriteLine("The module is not instrumented!");
            }
        }

        /// <summary>
        /// Create a refresh task for coverage
        /// </summary>
        /// <returns>Task</returns>
        private static Task InternalCreateCoverageListener()
        {
            if (!IsInstrumented)
            {
                return Task.CompletedTask;
            }

            // Task

            AppDomain.CurrentDomain.ProcessExit += (o, s) =>
            {
                // Report after exit

                DoReport();
            };

            return new Task(() =>
            {
                var coverage = 0D;

                while (_hitsArrayLength > 0)
                {
                    var cur = 0;

                    foreach (var hits in _hitsArray)
                    {
                        var array = hits;
                        cur += array.Count(u => u != 0);
                    }

                    if (_hitsArrayLength <= cur)
                    {
                        coverage = 100D;
                    }
                    else
                    {
                        coverage = cur * 100.0D / _hitsArrayLength;
                    }

                    CurrentCoverage = coverage;
                    Thread.Sleep(500);
                }
            });
        }

        /// <summary>
        /// Do report
        /// </summary>
        /// <param name="path">Path (default "InstrumentationResult.xml")</param>
        /// <param name="output">Output (default "opencover.xml")</param>
        /// <param name="format">Format (default "opencover")</param>
        /// <returns>Return true if it works</returns>
        public static bool DoReport(string path = "InstrumentationResult.xml", string output = "opencover.xml", string format = "opencover")
        {
            if (!IsInstrumented)
            {
                _logger.LogError("Is not instrumented");
                return false;
            }

            if (!File.Exists(path))
            {
                _logger.LogError("Result of instrumentation task not found");
                return false;
            }

            // Update hits

            if (_unloadModule != null)
            {
                foreach (var method in _unloadModule)
                {
                    method.Invoke(null, new object[] { null, null });
                }
            }

            // Parse instrumentation result

            Coverage coverage = null;
            using (var instrumenterStateStream = File.OpenRead(path))
            {
                coverage = new Coverage(CoveragePrepareResult.Deserialize(instrumenterStateStream),
                    _logger, (IInstrumentationHelper)DependencyInjection.Current.GetService(typeof(IInstrumentationHelper)),
                    (IFileSystem)DependencyInjection.Current.GetService(typeof(IFileSystem)));
            }

            // Get coverage

            // TODO: Require https://github.com/tonerdo/coverlet/pull/577

            var result = coverage.GetCoverageResult(false);
            var reporter = new ReporterFactory(format).CreateReporter();

            if (reporter == null)
            {
                throw new Exception($"Specified output format '{format}' is not supported");
            }

            // Create opencover file

            File.WriteAllText(output, reporter.Report(result));

            try
            {
                // Reporting

                var file = new FileInfo(output);
                Program.Main(new string[] { $"-reports:{file.FullName}", $"-targetdir:{Path.GetDirectoryName(file.FullName)}\\coverageReport" });
            }
            catch { }
            return true;
        }

        /// <summary>
        /// Instrument
        ///     Require patch in ProcessExitHandler for prevent restore files
        /// </summary>
        public static bool Instrument
            (
            string output, string path,
            string include, string includeDirectory,
            string exclude, string excludeByFile, string excludeByAttribute,
            bool includeTestAssembly,
            bool singleHit,
            string mergeWith,
            bool useSourceLink
            )
        {
            // Add default values

            if (string.IsNullOrEmpty(exclude))
            {
                exclude = "[coverlet.core]*,[TuringMachine.*]*";
            }
            else
            {
                exclude += ",[coverlet.core]*,[TuringMachine.*]*";
            }

            var includeFilters = include?.Split(',');
            var includeDirectories = includeDirectory?.Split(',');
            var excludeFilters = exclude?.Split(',');
            var excludedSourceFiles = excludeByFile?.Split(',');
            var excludeAttributes = excludeByAttribute?.Split(',');
            var fileSystem = (IFileSystem)DependencyInjection.Current.GetService(typeof(IFileSystem));

            try
            {
                var coverage = new Coverage(
                    path,
                    includeFilters,
                    includeDirectories,
                    excludeFilters,
                    excludedSourceFiles,
                    excludeAttributes,
                    includeTestAssembly,
                    singleHit,
                    mergeWith,
                    useSourceLink,
                    _logger,
                    (IInstrumentationHelper)DependencyInjection.Current.GetService(typeof(IInstrumentationHelper)),
                    fileSystem);

                var prepareResult = coverage.PrepareModules();

                if (File.Exists(output))
                {
                    File.Delete(output);
                }

                using (var instrumentedStateFile = File.OpenWrite(output))
                {
                    using (var serializedState = CoveragePrepareResult.Serialize(prepareResult))
                    {
                        serializedState.CopyTo(instrumentedStateFile);
                    }
                }

                if (Directory.Exists(path))
                {
                    try
                    {
                        var dest = Path.GetFileName(output);
                        dest = Path.Combine(path, output);

                        if (dest.ToLowerInvariant() == output.ToLowerInvariant())
                        {
                            return true;
                        }

                        if (File.Exists(dest))
                        {
                            File.Delete(dest);
                        }

                        File.Copy(output, dest);
                    }
                    catch { }
                }

                // Prevent revert cleaning the backup list

                var helper = (IInstrumentationHelper)DependencyInjection.Current.GetService(typeof(IInstrumentationHelper));
                var field = (ConcurrentDictionary<string, string>)helper
                    .GetType()
                    .GetField("_backupList", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(helper);

                field.Clear();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return false;
            }
        }
    }
}
