using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuringMachine.ConsoleStyle;
using TuringMachine.Core.Fuzzers;
using TuringMachine.Core.Fuzzers.Patch;
using TuringMachine.Core.Helpers;
using TuringMachine.Core.Interfaces;
using TuringMachine.Core.Logs;
using TuringMachine.RPC;

namespace TuringMachine
{
    class Program
    {
        private static bool _showMutations = true;
        private static int TitleColumnLength = 35;
        private static readonly int StatColumnLength = 13;
        private volatile static int _refreshRequested = 0;

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CmdOptions, CmdInstrumentOptions>(args)
                .WithParsed<CmdInstrumentOptions>(o => Instrument(o))
                .WithParsed<CmdOptions>(o => Run(o))
                .WithNotParsed((errs) => { });
        }

        private static void Instrument(CmdInstrumentOptions opts)
        {
            if (!CoverageHelper.Instrument
                (
                opts.Output,
                opts.Path, opts.Include, opts.IncludeDirectory, opts.Exclude,
                opts.ExcludeByFile, opts.ExcludeByAttribute, opts.IncludeTestAssembly,
                opts.SingleHit, opts.MergeWith, opts.UseSourceLink
                ))
            {
                Console.WriteLine("Instrumentation failure.");
            }
            else
            {
                Console.WriteLine("Instrumented successfully.");
            }
        }

        private static void Run(CmdOptions opts)
        {
            using (var server = new FuzzerServer())
            {
                RpcServer rpc = null;

                if (!string.IsNullOrEmpty(opts.RpcEndPoint))
                {
                    rpc = new RpcServer(opts.RpcEndPoint.ToIpEndPoint(), server);
                    rpc.Start();
                }

                // Parse

                foreach (var file in CmdOptions.GetFiles(opts.Inputs))
                {
                    foreach (var i in SerializationHelper.DeserializeFromFile<FuzzingInputBase>(file))
                    {
                        server.Inputs.Add(i.Id, new FuzzerStat<FuzzingInputBase>(i));
                    }
                }

                foreach (var file in CmdOptions.GetFiles(opts.Configs))
                {
                    foreach (var i in SerializationHelper.DeserializeFromFile<FuzzingConfigBase>(file))
                    {
                        server.Configurations.Add(i.Id, new FuzzerStat<FuzzingConfigBase>(i));
                    }
                }

                // Checks

                if (server.Inputs.Count == 0 && rpc == null)
                {
                    Console.WriteLine("No inputs found");
                    return;
                }

                if (server.Configurations.Count == 0)
                {
                    // Create an empty mutation, only the original value

                    _showMutations = false;
                    var empty = new PatchConfig()
                    {
                        Changes = new List<PatchChange>(),
                        Description = "Original input",
                        Id = Guid.Empty
                    };
                    server.Configurations.Add(empty.Id, new FuzzerStat<FuzzingConfigBase>(empty));
                }

                server.Start(opts.GetConnection());

                // Listener

                server.OnNewConnection += (s, e) =>
                {
                    PrintStats(server);
                };

                server.OnReceiveLog += (s2, logs) =>
                {
                    foreach (var log in logs)
                    {
                        log.Error?.Save(opts.OnlyUniques);
                    }
                    PrintStats(server);
                };

                // UI

                do
                {
                    Console.Clear();
                    PrintStats(server);
                }
                while (Console.ReadKey().Key != ConsoleKey.Enter);

                rpc?.Dispose();
            }
        }

        private static void PrintStats(FuzzerServer server)
        {
            if (Interlocked.Exchange(ref _refreshRequested, 0x01) == 0x01)
            {
                return;
            }

            new Task(() =>
            {
                AsyncPrintStats(server);
                Interlocked.Exchange(ref _refreshRequested, 0x00);
            })
            .Start();
        }

        private static void AsyncPrintStats(FuzzerServer server)
        {
            TitleColumnLength = Math.Max(20, (Console.WindowWidth - 1) - StatColumnLength * 3);
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);

            // Write data

            var colorBottom = ConsoleColor.White;

            var cellTitle = new ConsoleBox.Cell("Inputs", TitleColumnLength);
            var cellConfigurations = new ConsoleBox.Cell("Configurations", TitleColumnLength);
            var cellConnections = new ConsoleBox.Cell("Connections", TitleColumnLength - (StatColumnLength * 2));

            var cellSpeed = new ConsoleBox.Cell("Speed".PadLeft(StatColumnLength - 1, ' '), StatColumnLength);
            var cellCoverage = new ConsoleBox.Cell("Coverage".PadLeft(StatColumnLength - 1, ' '), StatColumnLength);
            var cellTest = new ConsoleBox.Cell("Test".PadLeft(StatColumnLength - 1, ' '), StatColumnLength);
            var cellErrors = new ConsoleBox.Cell("Errors".PadLeft(StatColumnLength - 1, ' '), StatColumnLength);
            var cellCrashes = new ConsoleBox.Cell("Crashes".PadLeft(StatColumnLength - 1, ' '), StatColumnLength);

            WriteList(server.Inputs.Values, ConsoleColor.Yellow, colorBottom, cellTitle, cellTest, cellErrors, cellCrashes);
            if (_showMutations)
            {
                WriteList(server.Configurations.Values, ConsoleColor.Yellow, colorBottom, cellConfigurations, cellTest, cellErrors, cellCrashes);
            }
            WriteList(server.Connections.Values.ToArray(), ConsoleColor.Yellow, colorBottom, cellConnections, cellCoverage, cellSpeed, cellTest, cellErrors, cellCrashes);

            // Write summary

            var l = (TitleColumnLength + (StatColumnLength * 3)) / 4;
            var lg = (TitleColumnLength + (StatColumnLength * 3)) - (l * 3);

            Console.ForegroundColor = colorBottom = ConsoleColor.Yellow;

            ConsoleBox.WriteCorner(CornerStyle.BoldTop, lg, l, l, l);

            if (server.UniqueErrors + server.TotalErrors > 0)
            {
                ConsoleBox.WriteContent(CornerStyle.BoldTop, "Unique", lg, BoxContentType.First);

                // Red color

                Console.ForegroundColor = server.UniqueErrors <= 0 ? colorBottom : ConsoleColor.Red;
                Console.Write(server.UniqueErrors.ToString().PadLeft(l - 1, ' '));

                Console.ForegroundColor = colorBottom;
                Console.Write(CornerStyle.BoldTop.BoxChar.ToString());
                ConsoleBox.WriteContent(CornerStyle.BoldTop, "Total", l, BoxContentType.Middle);

                Console.ForegroundColor = server.TotalErrors <= 0 ? colorBottom : ConsoleColor.Red;
                Console.Write(server.TotalErrors.ToString().PadLeft(l - 1, ' '));

                Console.ForegroundColor = colorBottom;
                Console.WriteLine(CornerStyle.BoldTop.BoxChar.ToString());
            }
            else
            {
                ConsoleBox.WriteContent(CornerStyle.BoldTop, "Unique", lg, BoxContentType.First);
                ConsoleBox.WriteContent(CornerStyle.BoldTop, server.UniqueErrors.ToString().PadLeft(l - 1, ' '), l, BoxContentType.Middle);
                ConsoleBox.WriteContent(CornerStyle.BoldTop, "Total", l, BoxContentType.Middle);
                ConsoleBox.WriteContent(CornerStyle.BoldTop, server.TotalErrors.ToString().PadLeft(l - 1, ' '), l, BoxContentType.Last);
            }

            ConsoleBox.WriteCorner(CornerStyle.BoldBot, lg, l, l, l);
        }

        private static void WriteList<T>
            (
            IEnumerable<FuzzerStat<T>> data,
            ConsoleColor colorTop,
            ConsoleColor colorBottom,
            params ConsoleBox.Cell[] title
            )
            where T : IIdentificable
        {
            FuzzerStat<T>[] array;
            lock (data)
            {
                array = data.ToArray();
            }

            if (array.Length <= 0)
            {
                return;
            }

            // Print header

            Console.ForegroundColor = colorTop;
            ConsoleBox.WriteCorner(CornerStyle.BoldTop, title);

            for (int x = 0, mx = title.Length; x < mx; x++)
            {
                var type = x == 0 ? BoxContentType.First : (x == mx - 1 ? BoxContentType.Last : BoxContentType.Middle);
                ConsoleBox.WriteContent(CornerStyle.BoldTop, title[x].Text, title[x].Width, type);
            }

            ConsoleBox.WriteCorner(CornerStyle.BoldBot, title);
            Console.ForegroundColor = colorBottom;

            // Print content

            var ix = 0;
            var botMid = array.Length > 1 ? array.Length - 1 : int.MinValue;

            foreach (var input in array)
            {
                if (ix == 0)
                {
                    ConsoleBox.WriteCorner(CornerStyle.SingleTop, title);
                }

                ix++;

                if (input.Source is FuzzerClientInfo ci)
                {
                    // Title

                    ConsoleBox.WriteContent(CornerStyle.SingleBot, input.Description, TitleColumnLength - (StatColumnLength * 2), BoxContentType.First);

                    // Coverage

                    Console.ForegroundColor = ci.Coverage <= 20 ? ConsoleColor.Red : ConsoleColor.Green;
                    Console.Write(ci.Coverage.ToString("0.00 '%'").PadLeft(StatColumnLength - 1, ' '));

                    Console.ForegroundColor = colorBottom;
                    Console.Write(CornerStyle.SingleBot.BoxChar.ToString());

                    // Speed

                    Console.Write(ci.Speed.ToString("#,###,##0.00").PadLeft(StatColumnLength - 1, ' ') + CornerStyle.SingleBot.BoxChar.ToString());
                }
                else
                {
                    ConsoleBox.WriteContent(CornerStyle.SingleBot, input.Description, TitleColumnLength, BoxContentType.First);
                }

                ConsoleBox.WriteContent(CornerStyle.SingleBot, input.Tests.ToString("#,###,###,##0").PadLeft(StatColumnLength - 1, ' '), StatColumnLength, BoxContentType.Middle);

                if (input.Errors + input.Crashes > 0)
                {
                    // Red color

                    Console.ForegroundColor = input.Errors <= 0 ? colorBottom : ConsoleColor.Red;
                    Console.Write(input.Errors.ToString("#,###,###,##0").PadLeft(StatColumnLength - 1, ' '));

                    Console.ForegroundColor = colorBottom;
                    Console.Write(CornerStyle.SingleBot.BoxChar.ToString());

                    Console.ForegroundColor = input.Crashes <= 0 ? colorBottom : ConsoleColor.Red;
                    Console.Write(input.Crashes.ToString("#,###,###,##0").PadLeft(StatColumnLength - 1, ' '));

                    Console.ForegroundColor = colorBottom;
                    Console.WriteLine(CornerStyle.SingleBot.BoxChar.ToString());
                }
                else
                {
                    ConsoleBox.WriteContent(CornerStyle.SingleBot, input.Errors.ToString("#,###,###,##0").PadLeft(StatColumnLength - 1, ' '), StatColumnLength, BoxContentType.Middle);
                    ConsoleBox.WriteContent(CornerStyle.SingleBot, input.Crashes.ToString("#,###,###,##0").PadLeft(StatColumnLength - 1, ' '), StatColumnLength, BoxContentType.Last);
                }

                if (ix <= botMid)
                {
                    ConsoleBox.WriteCorner(CornerStyle.SingleBotMid, title);
                }
                else
                {
                    ConsoleBox.WriteCorner(CornerStyle.SingleBot, title);
                }
            }
        }
    }
}
