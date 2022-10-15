using DemoScanner.DemoStuff.L4D2Branch.PortalStuff.Result;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DemoScanner.DemoStuff.L4D2Branch.PortalStuff.GameHandler
{
    public abstract class GameHandler
    {
        protected GameHandler()
        {
            MapStartAdjustType = "Map Start";
            MapEndAdjustType = "Map End";
            Maps = new List<string>();
            Flags = new List<KeyValuePair<string, int>>();
        }

        protected int CurrentTick { get; set; } = -1;

        public abstract DemoProtocolVersion DemoVersion { get; protected set; }

        public string FileName { get; set; }

        public string GameDir { get; set; }

        public string Map { get; set; }

        protected string MapEndAdjustType { get; }

        protected List<string> Maps { get; }

        protected string MapStartAdjustType { get; }

        public int NetworkProtocol { get; set; }

        public string PlayerName { get; set; }

        public List<KeyValuePair<string, int>> Flags { get; set; }

        public int SignOnLen { get; set; }

        public static GameHandler getGameHandler(string gameDir, string map)
        {
            if (gameDir == "portal") return new PortalGameHandler();
            if (gameDir == "portal2")
            {
                if (Category.Portal2Sp.Maps.Contains(map)) return new Portal2SpGameHandler();
                if (Category.Portal2Coop.Maps.Contains(map)) return new Portal2CoopGameHandler();
                if (Category.Portal2CoopCourse6.Maps.Contains(map)) return new Portal2CoopCourse6GameHandler();
            }

            throw new Exception("Unknown game");
        }

        public abstract DemoParseResult GetResult();
        public abstract long HandleCommand(byte command, int tick, BinaryReader br);
        public abstract bool IsStop(byte command);
        protected abstract ConsoleCmdResult ProcessConsoleCmd(BinaryReader br);
        protected abstract long ProcessCustomData(BinaryReader br);
        protected abstract PacketResult ProcessPacket(BinaryReader br);

        protected int ProcessSignOn(BinaryReader br)
        {
            br.BaseStream.Seek(SignOnLen, SeekOrigin.Current);
            return SignOnLen;
        }

        protected abstract long ProcessStringTables(BinaryReader br);
        protected abstract long ProcessUserCmd(BinaryReader br);
    }
}