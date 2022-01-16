using System;
using System.Collections.Generic;
using System.IO;
using DemoScanner.DemoStuff.L4D2Branch.BitStreamUtil;
using DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo;
using DemoScanner.DemoStuff.L4D2Branch.PortalStuff.Result;

namespace DemoScanner.DemoStuff.L4D2Branch
{
    /// <summary>
    ///     The header of the demo
    /// </summary>
    public class DemoHeader
    {
        private const int MaxOspath = 260;
        public string Filestamp { get; private set; } // Should be HL2DEMO
        public int Protocol { get; private set; } // Should be DEMO_PROTOCOL (4)
        public int NetworkProtocol { get; private set; } // Should be PROTOCOL_VERSION
        public string ServerName { get; private set; } // Name of server
        public string ClientName { get; private set; } // Name of client who recorded the game
        public string MapName { get; private set; } // Name of map
        public string GameDirectory { get; private set; } // Name of game directory (com_gamedir)
        public float PlaybackTime { get; private set; } // Time of track
        public int PlaybackTicks { get; private set; } // Number of ticks in track
        public int EventCount { get; private set; } // Number of frames in track
        public int SignonLength { get; private set; } // Length of signondata in bytes

        public int Tickrate
            => (int) Math.Round(PlaybackTicks / PlaybackTime);

        public float TicksPerSecond
            => PlaybackTime / PlaybackTicks;

        public static DemoHeader ParseFrom(IBitStream reader)
        {
            return new DemoHeader
            {
                Filestamp = reader.ReadCString(8),
                Protocol = reader.ReadSignedInt(32),
                NetworkProtocol = Math.Abs(reader.ReadSignedInt(32)),
                ServerName = reader.ReadCString(MaxOspath),
                ClientName = reader.ReadCString(MaxOspath),
                MapName = reader.ReadCString(MaxOspath),
                GameDirectory = reader.ReadCString(MaxOspath),
                PlaybackTime = Math.Abs(reader.ReadFloat()),
                PlaybackTicks = Math.Abs(reader.ReadSignedInt(32)),
                EventCount = Math.Abs(reader.ReadSignedInt(32)),
                SignonLength = Math.Abs(reader.ReadSignedInt(32))
            };
        }
    }

    /// <summary>
    ///     The details of the parsed L4D2 branch demo
    /// </summary>
    public class L4D2BranchDemoInfo
    {
        public DemoParser CsgoDemoInfo;
        public Category DemoType;
        public DemoHeader Header;
        public List<string> Parsingerrors;
        public DemoParseResult PortalDemoInfo;
    }

    internal class L4D2BranchParser
    {
        public L4D2BranchDemoInfo Parse(string filename)
        {
            var info = new L4D2BranchDemoInfo
            {
                Parsingerrors = new List<string>(),
                Header = DemoHeader.ParseFrom(new BitArrayStream(File.ReadAllBytes(filename)))
            };

            var map = info.Header.MapName;
            var game = info.Header.GameDirectory;
            if (game == "portal")
            {
                if (Category.Portal.HasMap(map)) info.DemoType = Category.Portal;

                info.PortalDemoInfo = PortalStuff.DemoParser.ParseDemo(filename);
            }
            else if (game == "portal2")
            {
                if (Category.Portal2Sp.HasMap(map))
                    info.DemoType = Category.Portal2Sp;
                else if (Category.Portal2Coop.HasMap(map))
                    info.DemoType = Category.Portal2Coop;
                else if (Category.Portal2CoopCourse6.HasMap(map))
                    info.DemoType = Category.Portal2CoopCourse6;
                else
                    info.DemoType = Category.Portal2Workshop;

                info.PortalDemoInfo = PortalStuff.DemoParser.ParseDemo(filename);
            }
            else if (game == "aperturetag")
            {
                if (Category.ApertureTag.HasMap(map))
                    info.DemoType = Category.ApertureTag;
                else
                    info.DemoType = Category.ApertureTagWorkshop;

                info.PortalDemoInfo = PortalStuff.DemoParser.ParseDemo(filename);
            }
            else if (game == "portal_stories")
            {
                if (Category.PortalStoriesMel.HasMap(map)) info.DemoType = Category.PortalStoriesMel;

                info.PortalDemoInfo = PortalStuff.DemoParser.ParseDemo(filename);
            }
            else if (game == "infra")
            {
                if (Category.Infra.HasMap(map, true))
                    info.DemoType = Category.Infra;
                else
                    info.DemoType = Category.InfraWorkshop;

                info.PortalDemoInfo = PortalStuff.DemoParser.ParseDemo(filename);
            }
            else if (game == "csgo")
            {
                info.DemoType = Category.Csgo;
                info.CsgoDemoInfo = CsgoDemoParser(filename);
            }

            info.DemoType = info.DemoType ?? Category.Uncommon;
            return info;
        }

        public static DemoParser CsgoDemoParser(string file)
        {
            var csgodemo = new DemoParser(File.OpenRead(file));
            csgodemo.ParseHeader();
            csgodemo.ParseToEnd();
            return csgodemo;
        }
    }
}