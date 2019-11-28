using CommandLine;

namespace TuringMachine
{
	[Verb("instrument", HelpText = "Instrument files.")]
	public class CmdInstrumentOptions
	{
		[Option('o', "output", Required = false, HelpText = "Instrument output file.")]
		public string Output { get; set; } = "InstrumentationResult.xml";

		// Instrumentation variables

		[Option('p', "path", Required = true, HelpText = "Instrument path.")]
		public string Path { get; set; }

		[Option('i', "include", Required = false, HelpText = "Inclument path.")]
		public string Include { get; set; }

		[Option('d', "includeDir", Required = false, HelpText = "Inclument dir path.")]
		public string IncludeDirectory { get; set; }

		[Option('e', "exclude", Required = false, HelpText = "Exclude dir path.")]
		public string Exclude { get; set; }

		[Option('f', "excludeByFile", Required = false, HelpText = "Exclude by file.")]
		public string ExcludeByFile { get; set; }

		[Option('a', "excludeByAttribute", Required = false, HelpText = "Exclude by attribute.")]
		public string ExcludeByAttribute { get; set; }

		[Option('t', "includeTestAssembly", Required = false, HelpText = "Include Test Assembly.")]
		public bool IncludeTestAssembly { get; set; } = false;

		[Option('s', "singleHit", Required = false, HelpText = "Single hit.")]
		public bool SingleHit { get; set; }

		[Option('m', "mergeWith", Required = false, HelpText = "Merge with path.")]
		public string MergeWith { get; set; }

		[Option('l', "useSourceLink", Required = false, HelpText = "Use source link.")]
		public bool UseSourceLink { get; set; }
	}
}
