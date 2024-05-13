using DemoScanner.DemoStuff.GoldSource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DemoScanner.DemoStuff
{
    /// <summary>
    ///     Type of the demo
    /// </summary>
    public enum Parseresult
    {
        /// <summary>
        ///     Not a demo/Unsupported
        /// </summary>
        UnsupportedFile,
        /// <summary>
        ///     GoldSource demo
        /// </summary>
        GoldSource,
        /// <summary>
        ///     HLS:OOE Demo
        /// </summary>
        Hlsooe,
        /// <summary>
        ///     Demo from the L4D2 Branch eg.: Portal 2,Left 4 Dead 2, Alien Swarm
        /// </summary>
        L4D2Branch,
        /// <summary>
        ///     Portal 1 demo
        /// </summary>
        Portal,
        /// <summary>
        ///     Source engine demo
        /// </summary>
        Source
    }

    /// <summary>
    ///     Data about the demo
    /// </summary>
    public class CrossParseResult
    {
        /// <summary>
        ///     The first values are exapnded to the same length (the length of the longest)
        ///     with spaces the seconds ones are as long as they are
        ///     this lets you print the data of the demo in human readable form
        /// </summary>
        public List<Tuple<string, string>> DisplayData;
        /// <summary>
        ///     The data about the GoldSource demo
        /// </summary>
        public GoldSourceDemoInfo GsDemoInfo;
        /// <summary>
        ///     The data about the HLS:OOE demo
        /// </summary>
        public GoldSourceDemoInfoHlsooe HlsooeDemoInfo;
        /// <summary>
        ///     Type of the demo
        /// </summary>
        public Parseresult Type;
        public byte MaxClients;
        /// <summary>
        ///     Empty constructor
        /// </summary>
        public CrossParseResult()
        {
        }
    }

    /// <summary>
    ///     Checking the type of the demo and parsing it accordingly
    /// </summary>
    public static class CrossDemoParser
    {
        /// <summary>
        ///     Parsing multiple demos asynchronously
        /// </summary>
        /// <param name = "filenames">String array with the paths to the files</param>
        /// <returns></returns>
        public static CrossParseResult[] MultiDemoParse(string[] filenames)
        {
            var results = new List<CrossParseResult>{new CrossParseResult()};
            //filenames.Select(AsyncParse).ToArray();
            return results.ToArray();
        }

        /// <summary>
        ///     This does an asynchronous demo parse.
        /// </summary>
        /// <param name = "filepath"></param>
        /// <returns></returns>
        /// <summary>
        ///     Parses a demo file from any engine
        /// </summary>
        /// <param name = "filename">Path to the file</param>
        /// <returns></returns>
        public static CrossParseResult Parse(string filename)
        {
            var cpr = new CrossParseResult();
            switch (CheckDemoType(filename))
            {
                case Parseresult.GoldSource:
                    cpr.Type = Parseresult.GoldSource;
                    cpr.GsDemoInfo = GoldSourceParser.ReadGoldSourceDemo(filename);
                    break;
                case Parseresult.UnsupportedFile:
                    cpr.Type = Parseresult.UnsupportedFile;
                    //Main.//Log("Demotype check resulted in an unsupported file.");
                    break;
                case Parseresult.Hlsooe:
                    cpr.Type = Parseresult.Hlsooe;
                    cpr.HlsooeDemoInfo = GoldSourceParser.ParseDemoHlsooe(filename);
                    break;
                default:
                    cpr.Type = Parseresult.UnsupportedFile;
                    break;
            }

            cpr.DisplayData = GetDemoDataTuples(cpr);
            return cpr;
        }

        /// <summary>
        ///     This returns a nice string which can be assigned to the richtexbox only call it if you
        ///     are extremely sure the demo is not corrupt.
        /// </summary>
        /// <param name = "demo"></param>
        /// <returns></returns>
        public static List<Tuple<string, string>> GetDemoDataTuples(CrossParseResult demo)
        {
            //Maybe enum as 3rd tuple item?
            var result = new List<Tuple<string, string>>();
#region Print
            switch (demo.Type)
            {
                case Parseresult.UnsupportedFile:
                    result.Add(new Tuple<string, string>("Unsupported file!", ""));
                    break;
                case Parseresult.GoldSource:
                    result = new List<Tuple<string, string>>();
                    try
                    {
                        result.Add(new Tuple<string, string>($"Analyzed GoldSource engine demo file ({demo.GsDemoInfo.Header.GameDir}):", ""));
                        result.Add(new Tuple<string, string>("Demo protocol", $"{demo.GsDemoInfo.Header.DemoProtocol}"));
                        result.Add(new Tuple<string, string>("Net protocol", $"{demo.GsDemoInfo.Header.NetProtocol}"));
                        result.Add(new Tuple<string, string>("Directory Offset", $"{demo.GsDemoInfo.Header.DirectoryOffset}"));
                        result.Add(new Tuple<string, string>("MapCRC", $"{demo.GsDemoInfo.Header.MapCrc}"));
                        result.Add(new Tuple<string, string>("Map name", $"{demo.GsDemoInfo.Header.MapName}"));
                        result.Add(new Tuple<string, string>("Game directory", $"{demo.GsDemoInfo.Header.GameDir}"));
                        result.Add(new Tuple<string, string>("Length in seconds", $"{demo.GsDemoInfo.DirectoryEntries.Sum(x => x.TrackTime).ToString("n3")}s"));
                        result.Add(new Tuple<string, string>("Directory entries", $"{demo.GsDemoInfo.DirectoryEntries.Count}"));
                        for (var i = 0; i < demo.GsDemoInfo.DirectoryEntries.Count; i++)
                        {
                            try
                            {
                                result.Add(new Tuple<string, string>("---------------------", "---------------------"));
                            }
                            catch
                            {
                            }

                            try
                            {
                                result.Add(new Tuple<string, string>("[Directory entry ", $"{i}]"));
                            }
                            catch
                            {
                            }

                            try
                            {
                                result.Add(new Tuple<string, string>("Length ", $"{demo.GsDemoInfo.DirectoryEntries[i].FileLength}"));
                            }
                            catch
                            {
                            }

                            try
                            {
                                result.Add(new Tuple<string, string>("Description ", $"{demo.GsDemoInfo.DirectoryEntries[i].Description}"));
                            }
                            catch
                            {
                            }

                            try
                            {
                                result.Add(new Tuple<string, string>("Frames ", $"{demo.GsDemoInfo.DirectoryEntries[i].FrameCount}"));
                            }
                            catch
                            {
                            }

                            try
                            {
                                result.Add(new Tuple<string, string>("Type ", $"{demo.GsDemoInfo.DirectoryEntries[i].Type}"));
                            }
                            catch
                            {
                            }

                            try
                            {
                                result.Add(new Tuple<string, string>("---------------------", "---------------------"));
                            }
                            catch
                            {
                            }
                        }

                        result.Add(new Tuple<string, string>("Frame count", $"{demo.GsDemoInfo.DirectoryEntries.Sum(x => x.FrameCount)}"));
                        result.Add(new Tuple<string, string>("Highest FPS", $"{demo.GsDemoInfo.DirectoryEntries.Sum(x => x.FrameCount)}"));
                    }
                    catch
                    {
                    }

                    break;
                case Parseresult.Hlsooe:
                    result = new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("Demo protocol", $"{demo.HlsooeDemoInfo.Header.DemoProtocol}"),
                        new Tuple<string, string>("Net protocol", $"{demo.HlsooeDemoInfo.Header.NetProtocol}"),
                        new Tuple<string, string>("Directory offset", $"{demo.HlsooeDemoInfo.Header.DirectoryOffset}"),
                        new Tuple<string, string>("Map name", $"{demo.HlsooeDemoInfo.Header.MapName}"),
                        new Tuple<string, string>("Game directory", $"{demo.HlsooeDemoInfo.Header.GameDir}"),
                        new Tuple<string, string>("Length in seconds",
                            $"{demo.HlsooeDemoInfo.DirectoryEntries.SkipWhile(x => x.FrameCount < 1).Max(x => x.Frames.Max(y => y.Key.Index)) * 0.015}s [{demo.HlsooeDemoInfo.DirectoryEntries.SkipWhile(x => x.FrameCount < 1).Max(x => x.Frames.Max(y => y.Key.Index))}ticks]"),
                        new Tuple<string, string>("Save flag:",
                            $"{demo.HlsooeDemoInfo.DirectoryEntries.SkipWhile(x => x.FrameCount < 1).Max(x => x.Frames.Where(y => y.Key.Type == Hlsooe.DemoFrameType.ConsoleCommand).FirstOrDefault(z => ((Hlsooe.ConsoleCommandFrame) z.Value).Command.Contains("#SAVE#")).Key.Index) * 0.015} [{demo.HlsooeDemoInfo.DirectoryEntries.SkipWhile(x => x.FrameCount < 1).Max(x => x.Frames.Where(y => y.Key.Type == Hlsooe.DemoFrameType.ConsoleCommand).FirstOrDefault(z => ((Hlsooe.ConsoleCommandFrame) z.Value).Command.Contains("#SAVE#")).Key.Index)}ticks]")
                    };
                    break;
            }

#endregion
            return FormatTuples(result);
        }

        /// <summary>
        ///     Formats the list of tuples' first value so all of them are padded to the same length as
        ///     the longest on +8 spaces so its nice
        /// </summary>
        /// <param name = "original">This is the original tuple list which we get from GetDemoDataTuples()</param>
        /// <param name = "spacebetween">
        ///     This is a not required parameter which supplies the space between the
        ///     two tuple items
        /// </param>
        /// <returns></returns>
        public static List<Tuple<string, string>> FormatTuples(List<Tuple<string, string>> original, int spacebetween = 8)
        {
            var result = original;
            var longest = result.Max(x => x.Item1.Length) + spacebetween;
            for (var index = 0; index < result.Count; index++)
                result[index] = new Tuple<string, string>(result[index].Item1.PadRight(longest), result[index].Item2);
            return result;
        }

        /// <summary>
        ///     Checks the demo type a Parseresult is returned
        /// </summary>
        /// <param name = "file">The path to the demo file to check</param>
        /// <returns></returns>
        public static Parseresult CheckDemoType(string file)
        {
            var attr = new FileInfo(file);
            if (attr.Length < 540)
                return Parseresult.UnsupportedFile;
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var br = new BinaryReader(fs))
                {
                    var mw = Encoding.ASCII.GetString(br.ReadBytes(8)).TrimEnd('\0');
                    switch (mw)
                    {
                        case "HLDEMO":
                            return br.ReadByte() <= 2 ? Parseresult.Hlsooe : Parseresult.GoldSource;
                        case "HL2DEMO":
                            return br.ReadInt32() < 4 ? Parseresult.Source : Parseresult.L4D2Branch; //TODO: Remove L4D2 Branch once costumdata parsing is done
                        default:
                            return Parseresult.UnsupportedFile;
                    }
                }
        }
    }
}