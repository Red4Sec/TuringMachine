using System;
using System.Linq;

namespace TuringMachine.ConsoleStyle
{
    public class ConsoleBox
    {
        public class Cell
        {
            public int Width = 0;
            public string Text;

            public Cell(string text, int width)
            {
                Width = width;
                Text = text;
            }
        }

        public static void WriteContent(CornerStyle style, string input, int width, BoxContentType type)
        {
            if (input == null) input = "";

            var print = new char[width];
            print[width - 1] = style.BoxChar;

            switch (type)
            {
                case BoxContentType.First:
                    {
                        print[0] = style.BoxChar;

                        for (var x = 1; x < width - 1; x++)
                        {
                            if (input.Length > x - 1)
                            {
                                print[x] = input[x - 1];
                            }
                            else
                            {
                                print[x] = ' ';
                            }
                        }
                        break;
                    }
                case BoxContentType.Middle:
                    {
                        for (var x = 0; x < width - 1; x++)
                        {
                            if (input.Length > x)
                            {
                                print[x] = input[x];
                            }
                            else
                            {
                                print[x] = ' ';
                            }
                        }
                        break;
                    }
                case BoxContentType.Last:
                    {
                        for (var x = 0; x < width - 1; x++)
                        {
                            if (input.Length > x)
                            {
                                print[x] = input[x];
                            }
                            else
                            {
                                print[x] = ' ';
                            }
                        }
                        break;
                    }
                case BoxContentType.Unique:
                    {
                        print[0] = style.BoxChar;

                        for (var x = 1; x < width - 1; x++)
                        {
                            if (input.Length >= x)
                            {
                                print[x] = input[x - 1];
                            }
                            else
                            {
                                print[x] = ' ';
                            }
                        }
                        break;
                    }
            }

            if (type == BoxContentType.Last || type == BoxContentType.Unique)
            {
                Console.WriteLine(new string(print));
            }
            else
            {
                Console.Write(new string(print));
            }
        }

        public static void WriteCorner(CornerStyle style, params Cell[] corners)
        {
            var width = corners.Sum(u => u.Width);
            var print = new char[width];

            print[0] = style.First;

            for (int x = 1; x < width - 1; x++) print[x] = style.Middle;

            var cx = 0;
            var dec = 1;
            foreach (var corner in corners)
            {
                cx += corner.Width - dec;
                print[cx] = cx == width - 1 ? style.Last : style.Gap;
                dec = 0;
            }

            Console.WriteLine(new string(print));
        }

        public static void WriteCorner(CornerStyle style, params int[] corners)
        {
            var width = corners.Sum();
            var print = new char[width];

            print[0] = style.First;

            for (int x = 1; x < width - 1; x++) print[x] = style.Middle;

            var cx = 0;
            var dec = 1;
            foreach (var corner in corners)
            {
                cx += corner - dec;
                print[cx] = cx == width - 1 ? style.Last : style.Gap;
                dec = 0;
            }

            Console.WriteLine(new string(print));
        }
    }
}
