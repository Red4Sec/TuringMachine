using CommandLine;
using System.Collections.Generic;
using System.IO;
using TuringMachine.Core;

namespace TuringMachine
{
    [Verb("listen", HelpText = "Listen server")]
    public class CmdOptions : CommandLineOptions
    {
        [Option('u', "onlyUniques", Required = false, HelpText = "Store only unique errors.")]
        public bool OnlyUniques { get; set; } = true;

        [Option('i', "inputs", Required = true, HelpText = "Set input folder.")]
        public IEnumerable<string> Inputs { get; set; }

        [Option('c', "configs", Required = false, HelpText = "Set config folder.")]
        public IEnumerable<string> Configs { get; set; }

        /// <summary>
        /// Get files
        /// </summary>
        /// <param name="inputs">Inputs</param>
        /// <returns>Files</returns>
        public static IEnumerable<string> GetFiles(IEnumerable<string> inputs)
        {
            foreach (var entry in inputs)
            {
                if (File.Exists(entry))
                {
                    yield return entry;
                }

                if (Directory.Exists(entry))
                {
                    foreach (var file in Directory.GetFiles(entry))
                    {
                        yield return file;
                    }
                }
            }
        }
    }
}
