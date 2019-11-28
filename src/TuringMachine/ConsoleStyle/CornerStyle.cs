namespace TuringMachine.ConsoleStyle
{
	public struct CornerStyle
	{
		public static readonly CornerStyle SingleTop = new CornerStyle('┌', '─', '┬', '┐', '│');
		public static readonly CornerStyle SingleBot = new CornerStyle('└', '─', '┴', '┘', '│');
		public static readonly CornerStyle SingleBotMid = new CornerStyle('├', '─', '┼', '┤', '│');

		public static readonly CornerStyle BoldTop = SingleTop; // new CornerStyle('╔', '═', '╦', '╗', '║');
		public static readonly CornerStyle BoldBotMid = SingleBotMid; // new CornerStyle('╚', '═', '╩', '╝', '║');
		public static readonly CornerStyle BoldBot = SingleBot; // new CornerStyle('╚', '═', '╩', '╝', '║');

		public readonly char First;
		public readonly char Middle;
		public readonly char Last;
		public readonly char Gap;

		public readonly char BoxChar;

		public CornerStyle(char first, char middle, char gap, char last, char boxChar)
		{
			First = first;
			Last = last;
			Gap = gap;
			Middle = middle;
			BoxChar = boxChar;

			//Console.WriteLine("┌─────┬─────┐");
			//Console.WriteLine("│     │     │");
			//Console.WriteLine("└─────┴─────┘");

			//Console.WriteLine("╔═════╦═════╗");
			//Console.WriteLine("║     ║     ║");
			//Console.WriteLine("╚═════╩═════╝");
		}
	}
}
