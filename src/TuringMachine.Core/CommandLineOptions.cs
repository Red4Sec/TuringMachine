using CommandLine;
using System;
using TuringMachine.Core.Fuzzers;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;

namespace TuringMachine.Core
{
    public class CommandLineOptions
    {
        [Option('s', "server", Required = false, HelpText = "Set server connection.")]
        public string Server { get; set; }

        [Option('p', "pipe", Required = false, HelpText = "Set pipe name.")]
        public string PipeName { get; set; } = "TuringMachine";

        /// <summary>
        /// Get connection from arguments
        /// </summary>
        /// <returns>Connection</returns>
        public FuzzerConnectionBase GetConnection()
        {
            if (!string.IsNullOrEmpty(Server))
            {
                return new FuzzerTcpConnection() { EndPoint = IPHelper.ToIpEndPoint(Server) };
            }

            return new FuzzerNamedPipeConnection()
            {
                PipeName = PipeName
            };
        }

        /// <summary>
        /// Parse arguments
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>CommandLineOptions</returns>
        public static CommandLineOptions Parse(string[] args = null)
        {
            if (args == null)
            {
                args = Environment.GetCommandLineArgs();
            }

            var ret = new CommandLineOptions();

            if (args?.Length > 0)
            {
                using (var parser = new Parser(u =>
                 {
                     u.AutoHelp = false;
                     u.AutoVersion = false;
                     u.CaseInsensitiveEnumValues = true;
                     u.CaseSensitive = false;
                     u.IgnoreUnknownArguments = true;
                 }))
                {
                    parser
                        .ParseArguments<CommandLineOptions>(args)
                        .WithParsed(opt => { ret = opt; });
                }
            }

            return ret;
        }
    }
}
