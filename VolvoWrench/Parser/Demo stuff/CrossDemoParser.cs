﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VolvoWrench.DemoStuff.GoldSource;
using VolvoWrench.DemoStuff.L4D2Branch;
using VolvoWrench.DemoStuff.Source;

namespace VolvoWrench.DemoStuff
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
    ///     Different importance levels for demo details
    /// </summary>
    public enum DemoDataLevel
    {
        /// <summary>
        ///     Not necesarry but important
        /// </summary>
        Aditional,

        /// <summary>
        ///     Normal
        /// </summary>
        Netural,

        /// <summary>
        ///     Really important
        /// </summary>
        Important
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
        ///     The data about the L4D2 Branch demo
        /// </summary>
        public L4D2BranchDemoInfo L4D2BranchInfo;

        public byte MaxClients;

        /// <summary>
        ///     The data about the Source engine demo
        /// </summary>
        public SourceDemoInfo Sdi;

        /// <summary>
        ///     Type of the demo
        /// </summary>
        public Parseresult Type;

        /// <summary>
        ///     Full constructor
        /// </summary>
        public CrossParseResult(GoldSourceDemoInfoHlsooe gsdi, Parseresult pr, SourceDemoInfo sdi,
            GoldSourceDemoInfo gd, L4D2BranchDemoInfo lbi, List<Tuple<string, string>> dd)
        {
            HlsooeDemoInfo = gsdi;
            Type = pr;
            Sdi = sdi;
            GsDemoInfo = gd;
            L4D2BranchInfo = lbi;
            DisplayData = dd;
        }

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
        /// <param name="filenames">String array with the paths to the files</param>
        /// <returns></returns>
        public static CrossParseResult[] MultiDemoParse(string[] filenames)
        {
            var results = new List<CrossParseResult> {new CrossParseResult()};
            //filenames.Select(AsyncParse).ToArray();
            return results.ToArray();
        }

        /// <summary>
        ///     This does an asynchronous demo parse.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        /// <summary>
        ///     Parses a demo file from any engine
        /// </summary>
        /// <param name="filename">Path to the file</param>
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
                case Parseresult.Source:
                    cpr.Type = Parseresult.Source;
                    var a = new SourceParser(new MemoryStream(File.ReadAllBytes(filename)));
                    cpr.Sdi = a.Info;
                    if (cpr.Sdi.GameDirectory == "portal")
                    {
                        cpr.Type = Parseresult.Portal;
                        var lp = new L4D2BranchParser();
                        cpr.L4D2BranchInfo = lp.Parse(filename);
                    }

                    break;
                case Parseresult.Hlsooe:
                    cpr.Type = Parseresult.Hlsooe;
                    cpr.HlsooeDemoInfo = GoldSourceParser.ParseDemoHlsooe(filename);
                    break;
                case Parseresult.L4D2Branch:
                    cpr.Type = Parseresult.L4D2Branch;
                    var l = new L4D2BranchParser();
                    cpr.L4D2BranchInfo = l.Parse(filename);
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
        /// <param name="demo"></param>
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
                        result.Add(
                            new Tuple<string, string>(
                                $"Analyzed GoldSource engine demo file ({demo.GsDemoInfo.Header.GameDir}):", ""));

                        result.Add(new Tuple<string, string>("Demo protocol",
                            $"{demo.GsDemoInfo.Header.DemoProtocol}"));
                        result.Add(new Tuple<string, string>("Net protocol", $"{demo.GsDemoInfo.Header.NetProtocol}"));
                        result.Add(new Tuple<string, string>("Directory Offset",
                            $"{demo.GsDemoInfo.Header.DirectoryOffset}"));
                        result.Add(new Tuple<string, string>("MapCRC", $"{demo.GsDemoInfo.Header.MapCrc}"));
                        result.Add(new Tuple<string, string>("Map name", $"{demo.GsDemoInfo.Header.MapName}"));
                        result.Add(new Tuple<string, string>("Game directory", $"{demo.GsDemoInfo.Header.GameDir}"));
                        result.Add(new Tuple<string, string>("Length in seconds",
                            $"{demo.GsDemoInfo.DirectoryEntries.Sum(x => x.TrackTime).ToString("n3")}s"));
                        result.Add(new Tuple<string, string>("Directory entries",
                            $"{demo.GsDemoInfo.DirectoryEntries.Count}"));


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
                                result.Add(new Tuple<string, string>("Length ",
                                    $"{demo.GsDemoInfo.DirectoryEntries[i].FileLength}"));
                            }
                            catch
                            {
                            }

                            try
                            {
                                result.Add(new Tuple<string, string>("Description ",
                                    $"{demo.GsDemoInfo.DirectoryEntries[i].Description}"));
                            }
                            catch
                            {
                            }

                            try
                            {
                                result.Add(new Tuple<string, string>("Frames ",
                                    $"{demo.GsDemoInfo.DirectoryEntries[i].FrameCount}"));
                            }
                            catch
                            {
                            }

                            try
                            {
                                result.Add(new Tuple<string, string>("Type ",
                                    $"{demo.GsDemoInfo.DirectoryEntries[i].Type}"));
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

                        result.Add(new Tuple<string, string>("Frame count",
                            $"{demo.GsDemoInfo.DirectoryEntries.Sum(x => x.FrameCount)}"));
                        result.Add(new Tuple<string, string>("Highest FPS",
                            $"{(1 / demo.GsDemoInfo.AditionalStats.FrametimeMin).ToString("N2")}"));
                        result.Add(new Tuple<string, string>("Lowest FPS",
                            $"{(1 / demo.GsDemoInfo.AditionalStats.FrametimeMax).ToString("N2")}"));
                        result.Add(new Tuple<string, string>("Average FPS",
                            $"{(demo.GsDemoInfo.AditionalStats.Count / demo.GsDemoInfo.AditionalStats.FrametimeSum).ToString("N2")}"));
                        result.Add(new Tuple<string, string>("Lowest msec",
                            $"{(1000.0 / demo.GsDemoInfo.AditionalStats.MsecMin).ToString("N2")} FPS"));
                        result.Add(new Tuple<string, string>("Highest msec",
                            $"{(1000.0 / demo.GsDemoInfo.AditionalStats.MsecMax).ToString("N2")} FPS"));
                        result.Add(new Tuple<string, string>("Frame count",
                            $"{demo.GsDemoInfo.DirectoryEntries.Sum(x => x.FrameCount)}"));
                        result.Add(new Tuple<string, string>("Average msec",
                            $"{(1000.0 / (demo.GsDemoInfo.AditionalStats.MsecSum / (double) demo.GsDemoInfo.AditionalStats.Count)).ToString("N2")} FPS"));
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
                case Parseresult.Source:
                    result = new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("Demo protocol", $"{demo.Sdi.DemoProtocol}"),
                        new Tuple<string, string>("Net protocol", $"{demo.Sdi.NetProtocol}"),
                        new Tuple<string, string>("Server name", $"{demo.Sdi.ServerName}"),
                        new Tuple<string, string>("Client name", $"{demo.Sdi.ClientName}"),
                        new Tuple<string, string>("Map name", $"{demo.Sdi.MapName}"),
                        new Tuple<string, string>("Playback seconds", $"{demo.Sdi.Seconds.ToString("n3")}s"),
                        new Tuple<string, string>("Playback tick count", $"{demo.Sdi.TickCount}"),
                        new Tuple<string, string>("Event count", $"{demo.Sdi.EventCount}"),
                        new Tuple<string, string>("Length",
                            $"{(demo.Sdi.Messages.SkipWhile(x => x.Type != SourceParser.MessageType.SyncTick).Max(x => x.Tick) * 0.015).ToString("n3")}s"),
                        new Tuple<string, string>("Ticks",
                            $"{demo.Sdi.Messages.SkipWhile(x => x.Type != SourceParser.MessageType.SyncTick).Max(x => x.Tick)}")
                    };
                    break;
                case Parseresult.Portal:
                case Parseresult.L4D2Branch:
                    result = new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("Demo protocol", $"{demo.L4D2BranchInfo.Header.Protocol}"),
                        new Tuple<string, string>("Net protocol", $"{demo.L4D2BranchInfo.Header.NetworkProtocol}"),
                        new Tuple<string, string>("Server name", $"{demo.L4D2BranchInfo.Header.ServerName}"),
                        new Tuple<string, string>("Client name", $"{demo.L4D2BranchInfo.Header.ClientName}"),
                        new Tuple<string, string>("Mapname", $"{demo.L4D2BranchInfo.Header.MapName}"),
                        new Tuple<string, string>("GameDir", $"{demo.L4D2BranchInfo.Header.GameDirectory}"),
                        new Tuple<string, string>("Playback seconds",
                            $"{demo.L4D2BranchInfo.Header.PlaybackTime.ToString("n3")}s"),
                        new Tuple<string, string>("Playback tick count", $"{demo.L4D2BranchInfo.Header.PlaybackTicks}"),
                        new Tuple<string, string>("Event count", $"{demo.L4D2BranchInfo.Header.EventCount}"),
                        new Tuple<string, string>("Signon Length", $"{demo.L4D2BranchInfo.Header.SignonLength}"),
                        new Tuple<string, string>("Tickrate", $"{demo.L4D2BranchInfo.Header.Tickrate}"),
                        new Tuple<string, string>("Start tick",
                            $"{demo.L4D2BranchInfo.PortalDemoInfo?.StartAdjustmentTick}"),
                        new Tuple<string, string>("Type", $"{demo.L4D2BranchInfo.PortalDemoInfo?.StartAdjustmentType}"),
                        new Tuple<string, string>("End tick",
                            $"{demo.L4D2BranchInfo.PortalDemoInfo?.EndAdjustmentTick}"),
                        new Tuple<string, string>("Type", $"{demo.L4D2BranchInfo.PortalDemoInfo?.EndAdjustmentType}"),
                        new Tuple<string, string>("Adjusted time",
                            $"{demo.L4D2BranchInfo.PortalDemoInfo?.AdjustTime(demo.L4D2BranchInfo.Header.TicksPerSecond).ToString("n3")}s"),
                        new Tuple<string, string>("Adjusted ticks",
                            $"{demo.L4D2BranchInfo.PortalDemoInfo?.AdjustedTicks}")
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
        /// <param name="original">This is the original tuple list which we get from GetDemoDataTuples()</param>
        /// <param name="spacebetween">
        ///     This is a not required parameter which supplies the space between the
        ///     two tuple items
        /// </param>
        /// <returns></returns>
        public static List<Tuple<string, string>> FormatTuples(List<Tuple<string, string>> original,
            int spacebetween = 8)
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
        /// <param name="file">The path to the demo file to check</param>
        /// <returns></returns>
        public static Parseresult CheckDemoType(string file)
        {
            var attr = new FileInfo(file);
            if (attr.Length < 540) return Parseresult.UnsupportedFile;
            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var br = new BinaryReader(fs))
            {
                var mw = Encoding.ASCII.GetString(br.ReadBytes(8)).TrimEnd('\0');
                switch (mw)
                {
                    case "HLDEMO": return br.ReadByte() <= 2 ? Parseresult.Hlsooe : Parseresult.GoldSource;
                    case "HL2DEMO":
                        return br.ReadInt32() < 4
                            ? Parseresult.Source
                            : Parseresult.L4D2Branch; //TODO: Remove L4D2 Branch once costumdata parsing is done
                    default: return Parseresult.UnsupportedFile;
                }
            }
        }
    }
}