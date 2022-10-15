using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace DemoScanner.DemoStuff.Source
{
    public struct Saveflag
    {
        public string Name;
        public int Tick;
        public float Time;
    }

    public struct SourceDemoInfo
    {
        public int DemoProtocol, NetProtocol, TickCount, EventCount, SignonLength;
        public List<Saveflag> Flags;
        public List<SourceParser.DemoMessage> Messages;
        public List<string> ParsingErrors;
        public float Seconds;
        public string ServerName, ClientName, MapName, GameDirectory;
    }

    public class SourceParser
    {
        public enum MessageType
        {
            Signon = 1,
            Packet,
            SyncTick,
            ConsoleCmd,
            UserCmd,
            DataTables,
            Stop,
            // CustomData, // L4D2
            StringTables
        }

        private readonly Stream _fstream;
        public SourceDemoInfo Info;

        public SourceParser(Stream s)
        {
            _fstream = s;
            Info.Messages = new List<DemoMessage>();
            Parse();
        }

        private void Parse()
        {
            var reader = new BinaryReader(_fstream);
            Info.Flags = new List<Saveflag>();
            Info.ParsingErrors = new List<string>();
            var id = reader.ReadBytes(8);

            if (Encoding.ASCII.GetString(id) != "HL2DEMO\0")
            {
                Info.ParsingErrors.Add("Source parser: Incorrect mw");
            }

            Info.DemoProtocol = reader.ReadInt32();
            if (Info.DemoProtocol >> 2 > 0)
            {
                Info.ParsingErrors.Add("Unsupported L4D2 branch demo!");
                //return;
            }

            Info.NetProtocol = reader.ReadInt32();

            Info.ServerName = new string(reader.ReadChars(260)).Replace("\0", "");
            Info.ClientName = new string(reader.ReadChars(260)).Replace("\0", "");
            Info.MapName = new string(reader.ReadChars(260)).Replace("\0", "");
            Info.GameDirectory = new string(reader.ReadChars(260)).Replace("\0", "");

            Info.Seconds = Math.Abs(reader.ReadSingle());
            Info.TickCount = Math.Abs(reader.ReadInt32());
            Info.EventCount = Math.Abs(reader.ReadInt32());

            Info.SignonLength = reader.ReadInt32();

            while (true)
            {
                var msg = new DemoMessage { Type = (MessageType)reader.ReadByte() };
                if (msg.Type == MessageType.Stop)
                    break;
                msg.Tick = reader.ReadInt32();

                switch (msg.Type)
                {
                    case MessageType.Signon:
                    case MessageType.Packet:
                    case MessageType.ConsoleCmd:
                    case MessageType.UserCmd:
                    case MessageType.DataTables:
                    case MessageType.StringTables:
                        if (msg.Type == MessageType.Packet || msg.Type == MessageType.Signon)
                            reader.BaseStream.Seek(0x54, SeekOrigin.Current); // command/sequence info
                        else if (msg.Type == MessageType.UserCmd)
                            reader.BaseStream.Seek(0x4, SeekOrigin.Current); // unknown
                        msg.Data = reader.ReadBytes(reader.ReadInt32());
                        break;
                    case MessageType.SyncTick:
                        msg.Data = new byte[0];
                        break;
                    default:
                        //Main.Log("Unknown demo message type encountered: " + msg.Type + "at " +
                        //         reader.BaseStream.Position);
                        Info.ParsingErrors.Add("Unknown demo message type encountered: " + msg.Type + "at " +
                                               reader.BaseStream.Position);
                        return;
                }

                if (msg.Data != null)
                {
                    if (Encoding.ASCII.GetString(msg.Data).Contains("#SAVE#") ||
                        Encoding.ASCII.GetString(msg.Data).Contains("autosave"))
                    {
                        var tempf = new Saveflag
                        {
                            Tick = msg.Tick,
                            Time = (float)(msg.Tick * 0.015)
                        };
                        if (Encoding.ASCII.GetString(msg.Data).Contains("#SAVE#"))
                            tempf.Name = "#SAVE#";
                        if (Encoding.ASCII.GetString(msg.Data).Contains("autosave"))
                            tempf.Name = "autosave";
                        Info.Flags.Add(tempf);
                    }
                }
                Info.Messages.Add(msg);
            }
        }

        public class DemoMessage
        {
            public byte[] Data;
            public int Tick;
            public MessageType Type;
        }
    }

    public class UserCmd
    {
        public static void ParseIntoTreeNode(byte[] data, TreeNode node)
        {
            var bb = new BitBuffer(data);
            if (bb.ReadBoolean()) node.Nodes.Add("Command number: " + bb.ReadBits(32));
            if (bb.ReadBoolean()) node.Nodes.Add("Tick count: " + bb.ReadBits(32));
            if (bb.ReadBoolean()) node.Nodes.Add("Viewangle pitch: " + bb.ReadSingle());
            if (bb.ReadBoolean()) node.Nodes.Add("Viewangle yaw: " + bb.ReadSingle());
            if (bb.ReadBoolean()) node.Nodes.Add("Viewangle roll: " + bb.ReadSingle());
            if (bb.ReadBoolean())
            {
                var xDiff = bb.ReadSingle();
                node.Nodes.Add("Foward move: " + xDiff);
                node.Nodes.Add("X velocity: " + xDiff / 0.015 + "ups");
            }
            if (bb.ReadBoolean())
            {
                var xDiff = bb.ReadSingle();
                node.Nodes.Add("Side move: " + xDiff);
                node.Nodes.Add("Y velocity: " + xDiff / 0.015 + "ups");
            }
            if (bb.ReadBoolean())
            {
                var xDiff = bb.ReadSingle();
                node.Nodes.Add("Foward move: " + xDiff);
                node.Nodes.Add("Z velocity: " + xDiff / 0.015 + "ups");
            }
            if (bb.ReadBoolean()) node.Nodes.Add("Buttons: " + KeyInterop.KeyFromVirtualKey(Convert.ToInt32(bb.ReadBits(32))));
            if (bb.ReadBoolean()) node.Nodes.Add("Impulse: " + bb.ReadBits(8));
            if (bb.ReadBoolean()) node.Nodes.Add("Weaponselect: " + bb.ReadBits(11));
            if (bb.ReadBoolean()) node.Nodes.Add("Weapon subtype: " + bb.ReadBits(6));
            if (bb.ReadBoolean()) node.Nodes.Add("Mouse X: " + bb.ReadCoord());
            if (bb.ReadBoolean()) node.Nodes.Add("Mouse Y: " + bb.ReadCoord());
        }
    }
}