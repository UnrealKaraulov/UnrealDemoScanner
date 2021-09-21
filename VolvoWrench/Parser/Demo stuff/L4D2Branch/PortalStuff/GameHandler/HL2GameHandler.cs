using System;
using System.IO;
using System.Text;
using DemoScanner.DemoStuff.L4D2Branch.PortalStuff.Result;

namespace DemoScanner.DemoStuff.L4D2Branch.PortalStuff.GameHandler
{
    internal class HL2GameHandler : GameHandler
    {
        public HL2GameHandler()
        {
            DemoVersion = DemoProtocolVersion.HL2;
        }

        public override DemoProtocolVersion DemoVersion { get; protected set; }

        public override DemoParseResult GetResult()
        {
            var demoParseResult = new DemoParseResult
            {
                FileName = FileName,
                MapName = Map,
                PlayerName = PlayerName,
                GameDir = GameDir,
                TotalTicks = CurrentTick,
                StartAdjustmentType = MapStartAdjustType,
                EndAdjustmentType = MapEndAdjustType
            };
            return demoParseResult;
        }

        public override long HandleCommand(byte command, int tick, BinaryReader br)
        {
            if (CurrentTick == -1)
            {
                if (tick == 0) CurrentTick = tick;
            }
            else if (tick > 0 && tick > CurrentTick)
            {
                CurrentTick = tick;
            }

            Enum.IsDefined(typeof(HL2DemoCommands), (HL2DemoCommands) command);
            if (command == 1) return ProcessSignOn(br);
            if (command == 2) return ProcessPacket(br).Read;
            if (command == 3) return 0;
            if (command == 4) return ProcessConsoleCmd(br).Read;
            if (command == 5) return ProcessUserCmd(br);
            if (command == 6) throw new NotImplementedException();
            if (command != 8) throw new Exception(string.Concat("Unknown command: 0x", command.ToString("x")));
            return ProcessStringTables(br);
        }

        public override bool IsStop(byte command)
        {
            return command == 7;
        }

        protected override ConsoleCmdResult ProcessConsoleCmd(BinaryReader br)
        {
            var position = br.BaseStream.Position;
            var num = br.ReadInt32();
            var str = Encoding.ASCII.GetString(br.ReadBytes(num)).TrimEnd(new char[1]);
            var consoleCmdResult = new ConsoleCmdResult
            {
                Read = br.BaseStream.Position - position,
                Command = str
            };
            return consoleCmdResult;
        }

        protected override long ProcessCustomData(BinaryReader br)
        {
            throw new NotImplementedException();
        }

        protected override PacketResult ProcessPacket(BinaryReader br)
        {
            var position = br.BaseStream.Position;
            br.BaseStream.Seek(4, SeekOrigin.Current);
            var single = br.ReadSingle();
            var single1 = br.ReadSingle();
            var single2 = br.ReadSingle();
            br.BaseStream.Seek(68, SeekOrigin.Current);
            var num = br.ReadInt32();
            br.BaseStream.Seek(num, SeekOrigin.Current);
            var packetResult = new PacketResult
            {
                Read = br.BaseStream.Position - position,
                CurrentPosition = new Point3D(single, single1, single2)
            };
            return packetResult;
        }

        protected override long ProcessStringTables(BinaryReader br)
        {
            var position = br.BaseStream.Position;
            var num = br.ReadInt32();
            br.BaseStream.Seek(num, SeekOrigin.Current);
            return br.BaseStream.Position - position;
        }

        protected override long ProcessUserCmd(BinaryReader br)
        {
            var position = br.BaseStream.Position;
            br.BaseStream.Seek(4, SeekOrigin.Current);
            var num = br.ReadInt32();
            br.BaseStream.Seek(num, SeekOrigin.Current);
            return br.BaseStream.Position - position;
        }

        protected enum HL2DemoCommands
        {
            SignOn = 1,
            Packet = 2,
            SyncTick = 3,
            ConsoleCmd = 4,
            UserCmd = 5,
            DataTables = 6,
            Stop = 7,
            StringTables = 8
        }
    }
}