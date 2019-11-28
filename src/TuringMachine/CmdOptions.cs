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

		[Option('i', "inputs", Required = false, HelpText = "Set input folder.")]
		public IEnumerable<string> Inputs { get; set; }

		[Option('c', "configs", Required = false, HelpText = "Set config folder.")]
		public IEnumerable<string> Configs { get; set; }

		[Option('r', "rpc", Required = false, HelpText = "Rpc EndPoint (127.0.0.1,1234)")]
		public string RpcEndPoint { get; set; }

		[Option('s', "rpc-cert", Required = false, HelpText = "Rpc HTTPS certificate")]
		public string RpcHttpsCertificate { get; set; }

		[Option('p', "rpc-cert-password", Required = false, HelpText = "Rpc HTTPS certificate password")]
		public string RpcHttpsCertificatePassword { get; set; }

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
