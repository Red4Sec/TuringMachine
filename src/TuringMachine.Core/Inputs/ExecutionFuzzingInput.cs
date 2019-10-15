using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core.Inputs
{
    public class ExecutionFuzzingInput : FuzzingInputBase, IEquatable<ExecutionFuzzingInput>
    {
        /// <summary>
        /// Exit time out
        /// </summary>
        public TimeSpan ExitTimeOut { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// FileName
        /// </summary>
        [Category("2 - Execution")]
        public string FileName { get; set; }

        /// <summary>
        /// Arguments
        /// </summary>
        [Category("2 - Execution")]
        public string Arguments { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ExecutionFuzzingInput() : base("Execution") { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">File</param>
        /// <param name="args">Args</param>
        public ExecutionFuzzingInput(string filename, string args) : base("Execution")
        {
            FileName = filename;
            Arguments = args;
        }

        /// <summary>
        /// Get process output
        /// </summary>
        /// <returns>Process output</returns>
        public override byte[] GetStream()
        {
            var pr = Process.Start(new ProcessStartInfo()
            {
                FileName = FileName,
                Arguments = Arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            using (var ms = new MemoryStream())
            {
                pr.StandardOutput.BaseStream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public bool Equals(ExecutionFuzzingInput obj)
        {
            if (obj == null) return false;

            return Equals((FuzzingInputBase)obj)
                && FileName.Equals(obj.FileName)
                && Arguments.Equals(obj.Arguments);
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Return true if are equals</returns>
        public override bool Equals(object obj)
        {
            if (obj is ExecutionFuzzingInput o)
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
            var hashCode = -1289872652;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Arguments);
            return hashCode;
        }
    }
}
