using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
//using System.Windows.Forms; // MethodInvoker
using System.Collections;

namespace compLexity_Demo_Player
{
    public class SourceDemoParser : DemoParser<SourceDemo>
    {
        public enum MessageId : byte
        {
            Nop = 0,
            Download = 2, // ???
            NET_Tick = 3,
            NET_StringCmd = 4,
            NET_SetConVar = 5,
            NET_SignonState = 6,
            SVC_Print = 7,
            SVC_ServerInfo = 8,
            SVC_ClassInfo = 10,
            SVC_SetPause = 11,
            SVC_CreateStringTable = 12,
            SVC_UpdateStringTable = 13,
            SVC_VoiceInit = 14,
            SVC_VoiceData = 15,
            SVC_Sounds = 17,
            SVC_SetView = 18,
            SVC_FixAngle = 19,
            SVC_BSPDecal = 21,
            SVC_UserMessage = 23,
            SVC_GameEvent = 25,
            SVC_PacketEntities = 26,
            SVC_TempEntities = 27,
            SVC_Prefetch = 28,
            SVC_GameEventList = 30,
            SVC_GetCvarValue = 31
        }

        public enum FrameType
        {
            Signon = 1, // it's a startup message, process as fast as possible
            Packet, // it's a normal network packet that we stored off
            Synctick, // sync client clock to demo tick
            Console, // console command 
            User, // user input command
            DataTables, // network data tables
            Stop, // end of time.
            StringTables
        }

        public class FrameHeader
        {
            public FrameType Type; // read/write as byte
            public Int32 Tick;
        }

        public class Vector
        {
            public Single X;
            public Single Y;
            public Single Z;
        }

        public class Quaternion
        {
            public Single X;
            public Single Y;
            public Single Z;
        }

        public class CommandInfo
        {
            public Int32 Flags;

            public Vector ViewOrigin;
            public Quaternion ViewAngles;
            public Quaternion LocalViewAngles;

            public Vector ViewOriginResampled;
            public Quaternion ViewAnglesResampled;
            public Quaternion LocalViewAnglesResampled;
        }

        public class SequenceInfo
        {
            public Int32 In;
            public Int32 Out;
        }

        public class GameEvent
        {
            public String Name;
            public Procedure Callback;

            public GameEvent(String name, Procedure callback)
            {
                Name = name;
                Callback = callback;
            }
        }

        private BitBuffer bitBuffer;
        private Boolean parsingPacket = false; // used to determine what to seek (binaryreader or bitbuffer)
        private Hashtable gameEventTable;
        private Hashtable gameEventStringTable;

        public BitBuffer BitBuffer
        {
            get
            {
                return bitBuffer;
            }
        }

        public SourceDemoParser(SourceDemo demo)
        {
            this.demo = demo;

            // message handlers
            AddMessageHandler((Byte)MessageId.Nop, 0);
            AddMessageHandler((Byte)MessageId.Download, MessageDownload);
            AddMessageHandler((Byte)MessageId.NET_Tick, MessageNetTick);
            AddMessageHandler((Byte)MessageId.NET_StringCmd, MessageNetStringCmd);
            AddMessageHandler((Byte)MessageId.NET_SetConVar, MessageNetSetConVar);
            AddMessageHandler((Byte)MessageId.NET_SignonState, MessageSignonState);
            AddMessageHandler((Byte)MessageId.SVC_Print, MessagePrint);
            AddMessageHandler((Byte)MessageId.SVC_ServerInfo, MessageServerInfo);
            AddMessageHandler((Byte)MessageId.SVC_ClassInfo, MessageClassInfo);
            AddMessageHandler((Byte)MessageId.SVC_SetPause, MessageSetPause);
            AddMessageHandler((Byte)MessageId.SVC_CreateStringTable, MessageCreateStringTable);
            AddMessageHandler((Byte)MessageId.SVC_UpdateStringTable, MessageUpdateStringTable);
            AddMessageHandler((Byte)MessageId.SVC_VoiceInit, MessageVoiceInit);
            AddMessageHandler((Byte)MessageId.SVC_VoiceData, MessageVoiceData);
            AddMessageHandler((Byte)MessageId.SVC_Sounds, MessageSounds);
            AddMessageHandler((Byte)MessageId.SVC_SetView, MessageSetView);
            AddMessageHandler((Byte)MessageId.SVC_FixAngle, MessageFixAngle);
            AddMessageHandler((Byte)MessageId.SVC_BSPDecal, MessageBspDecal);
            AddMessageHandler((Byte)MessageId.SVC_UserMessage, MessageUserMessage);
            AddMessageHandler((Byte)MessageId.SVC_GameEvent, MessageGameEvent);
            AddMessageHandler((Byte)MessageId.SVC_PacketEntities, MessagePacketEntities);
            AddMessageHandler((Byte)MessageId.SVC_TempEntities, MessageTempEntities);
            AddMessageHandler((Byte)MessageId.SVC_Prefetch, MessagePrefetch);
            AddMessageHandler((Byte)MessageId.SVC_GameEventList, MessageGameEventList);
            AddMessageHandler((Byte)MessageId.SVC_GetCvarValue, MessageGetCvarValue);

            // game events
            gameEventTable = new Hashtable();
            gameEventStringTable = new Hashtable();
        }

        public void AddGameEvent(UInt32 id, String name)
        {
            gameEventTable.Add(id, new GameEvent(name, null));
            gameEventStringTable.Add(name, id);
        }

        public void AddGameEventCallback(String name, Procedure callback)
        {
            // TODO: assert that gameeventlist has been parsed
            UInt32? id = (UInt32?)gameEventStringTable[name];

            if (id == null)
            {
                return; // shouldn't happen
            }

            GameEvent gameEvent = (GameEvent)gameEventTable[id];

            if (gameEvent == null)
            {
                gameEventTable.Add(id, new GameEvent(name, callback));
            }
            else
            {
                gameEvent.Callback = callback;
            }
        }

        public FrameHeader ReadFrameHeader()
        {
            Int64 offset = fileStream.Position;

            FrameHeader frameHeader = new FrameHeader();
            frameHeader.Type = (FrameType)fileReader.ReadByte();

            if (frameHeader.Type != FrameType.Stop)
            {
                frameHeader.Tick = fileReader.ReadInt32();
            }
            else
            {
                // tick is probably really 3 bytes, and non-Stop frames have an extra byte that's always 0
                Byte[] temp = fileReader.ReadBytes(3);
                frameHeader.Tick = temp[0] + (temp[1] << 8) + (temp[2] << 16);
            }

            return frameHeader;
        }

        public void Seek(Int64 deltaOffset)
        {
            Seek(deltaOffset, SeekOrigin.Current);
        }

        public void Seek(Int64 offset, SeekOrigin origin)
        {
            if (parsingPacket)
            {
                bitBuffer.SeekBits((Int32)(offset*8), origin);
            }
            else
            {
                fileStream.Seek(offset, origin);
            }
        }

        public void ParseFrame(FrameType frameType)
        {
            if (frameType == FrameType.User)
            {
                Seek(4);
            }

            if (frameType != FrameType.Synctick)
            {
                Int32 frameLength = Reader.ReadInt32();
                Seek(frameLength);
            }
        }

        public void ParsePacketMessages(Int32 dataLength)
        {
            Int64 packetStartOffset = Position;
            parsingPacket = true;

            // read in data block
            Byte[] frameData = fileReader.ReadBytes(dataLength);
            bitBuffer = new BitBuffer(frameData);

            BeginMessageLog(packetStartOffset, frameData);

            // parse messages
            while (true)
            {
                if (bitBuffer.BitsLeft < 8)
                {
                    // done
                    break;
                }

                Int32 messageFrameOffset = bitBuffer.CurrentBit;
                Byte messageId = (Byte)bitBuffer.ReadUnsignedBits(demo.NetworkProtocol >= 16 ? 6 : 5);

                if (demo.Protocol15Hack)
                {
                    bitBuffer.SeekBits(1);
                }

                String messageName = Enum.GetName(typeof(MessageId), messageId);

                LogMessage(messageId, messageName, messageFrameOffset);

                MessageHandler messageHandler = FindMessageHandler(messageId);

                // unknown message
                if (messageHandler == null)
                {
                    throw new ApplicationException(String.Format("Cannot find message handler for message id \"[{0}] {1}\"", messageId, messageName));
                }

                // callback takes priority over length
                if (messageHandler.Callback != null)
                {
                    messageHandler.Callback();
                }
                else if (messageHandler.Length != -1)
                {
                    Seek(messageHandler.Length);
                }
                else
                {
                    throw new ApplicationException(String.Format("Unknown message id \"{0}\"", messageId));
                }
            }

            parsingPacket = false;
        }

        public CommandInfo ReadCommandInfo()
        {
            CommandInfo ci = new CommandInfo();

            ci.Flags = fileReader.ReadInt32();
            ci.ViewOrigin = ReadVector();
            ci.ViewAngles = ReadQuaternion();
            ci.LocalViewAngles = ReadQuaternion();
            ci.ViewOriginResampled = ReadVector();
            ci.ViewAnglesResampled = ReadQuaternion();
            ci.LocalViewAnglesResampled = ReadQuaternion();

            return ci;
        }

        public void WriteCommandInfo(CommandInfo ci, BinaryWriter bw)
        {
            bw.Write(ci.Flags);
            WriteVector(ci.ViewOrigin, bw);
            WriteQuaternion(ci.ViewAngles, bw);
            WriteQuaternion(ci.LocalViewAngles, bw);
            WriteVector(ci.ViewOriginResampled, bw);
            WriteQuaternion(ci.ViewAnglesResampled, bw);
            WriteQuaternion(ci.LocalViewAnglesResampled, bw);
        }

        public SequenceInfo ReadSequenceInfo()
        {
            SequenceInfo si = new SequenceInfo();

            si.In = fileReader.ReadInt32();
            si.Out = fileReader.ReadInt32();

            return si;
        }

        public void WriteSequenceInfo(SequenceInfo si, BinaryWriter bw)
        {
            bw.Write(si.In);
            bw.Write(si.Out);
        }

        private Vector ReadVector()
        {
            Vector v = new Vector();

            v.X = fileReader.ReadSingle();
            v.Y = fileReader.ReadSingle();
            v.Z = fileReader.ReadSingle();

            return v;
        }

        private Quaternion ReadQuaternion()
        {
            Quaternion q = new Quaternion();

            q.X = fileReader.ReadSingle();
            q.Y = fileReader.ReadSingle();
            q.Z = fileReader.ReadSingle();

            return q;
        }

        public void WriteVector(Vector v, BinaryWriter bw)
        {
            bw.Write(v.X);
            bw.Write(v.Y);
            bw.Write(v.Z);
        }

        public void WriteQuaternion(Quaternion q, BinaryWriter bw)
        {
            bw.Write(q.X);
            bw.Write(q.Y);
            bw.Write(q.Z);
        }

        #region Messages
        public void MessageDownload()
        {
            bitBuffer.SeekBits(32);
            bitBuffer.ReadString();
            bitBuffer.SeekBits(1);
        }

        public void MessageNetTick()
        {
            bitBuffer.SeekBits(32);

            if (demo.NetworkProtocol >= 14)
            {
                bitBuffer.SeekBits(32);
            }
        }

        public void MessageNetStringCmd()
        {
            bitBuffer.ReadString();
        }

        public void MessageNetSetConVar()
        {
            Int32 nConVars = bitBuffer.ReadByte();

            for (Int32 i = 0; i < nConVars; i++)
            {
                bitBuffer.ReadString(); // name
                bitBuffer.ReadString(); // value
            }
        }

        public void MessageSignonState()
        {
            bitBuffer.SeekBits(40);
        }

        public void MessagePrint()
        {
            bitBuffer.ReadString();
        }

        public void MessageServerInfo()
        {
            bitBuffer.SeekBits(16); // network protocol (same as header)
            bitBuffer.SeekBits(32); // spawn count
            bitBuffer.SeekBits(138); // ?
            bitBuffer.ReadString(); // game dir (same as header)
            bitBuffer.ReadString(); // map name (same as header - unless there's a map change?)
            bitBuffer.ReadString(); // sky name
            bitBuffer.ReadString(); // server name (different from header - header is usually the address)

            if (demo.NetworkProtocol >= 16)
            {
                bitBuffer.SeekBits(1);
            }
        }

        public void MessageClassInfo()
        {
            bitBuffer.SeekBits(17);
        }

        public void MessageSetPause()
        {
            bitBuffer.SeekBits(1);
        }

        public void MessageCreateStringTable()
        {
            bitBuffer.ReadString(); // table name

            UInt32 maxEntries = bitBuffer.ReadUnsignedBits(16);
            UInt32 nEntries = bitBuffer.ReadUnsignedBits(Common.LogBase2((Int32)maxEntries) + 1);

            UInt32 nBits = bitBuffer.ReadUnsignedBits(20);

            if (bitBuffer.ReadBoolean()) // userdata bit
            {
                bitBuffer.SeekBits(12); // "user data size"
                bitBuffer.SeekBits(4); // "user data bits"
            }

            bitBuffer.SeekBits((Int32)nBits);

            if (demo.NetworkProtocol >= 15)
            {
                bitBuffer.SeekBits(1);
            }
        }

        public void MessageUpdateStringTable()
        {
            // table id
            if (demo.DemoProtocol <= 2)
            {
                bitBuffer.SeekBits(4);
            }
            else
            {
                bitBuffer.SeekBits(5);
            }

            if (bitBuffer.ReadBoolean())
            {
                bitBuffer.SeekBits(16); // nEntries
            }

            UInt32 nBits;

            if (demo.NetworkProtocol >= 14)
            {
                nBits = bitBuffer.ReadUnsignedBits(20);
            }
            else
            {
                nBits = bitBuffer.ReadUnsignedBits(16);
            }

            bitBuffer.SeekBits((Int32)nBits);
        }

        public void MessageVoiceInit()
        {
            bitBuffer.ReadString(); // codec
            bitBuffer.SeekBits(8); // quality
        }

        public void MessageVoiceData()
        {
            bitBuffer.SeekBits(8); // client
            bitBuffer.SeekBits((Int32)bitBuffer.ReadUnsignedBits(16));
        }

        public void MessageSounds()
        {
            UInt32 nBits;

            if (bitBuffer.ReadBoolean())
            {
                nBits = bitBuffer.ReadUnsignedBits(8);
            }
            else
            {
                bitBuffer.SeekBits(8);
                nBits = bitBuffer.ReadUnsignedBits(16);
            }

            bitBuffer.SeekBits((Int32)nBits);
        }

        public void MessageSetView()
        {
            bitBuffer.SeekBits(11); // view entity number
        }

        public void MessageFixAngle()
        {
            // 3x bit angle (16 bits)
            bitBuffer.SeekBits(48);
        }

        public void MessageBspDecal()
        {
            bitBuffer.ReadVectorCoord();
            bitBuffer.SeekBits(9); // "tex"

            Boolean bit1 = bitBuffer.ReadBoolean();

            if (bit1)
            {
                bitBuffer.SeekBits(11); // ent
                bitBuffer.SeekBits(11); // mod
            }

            bitBuffer.SeekBits(1); // low priority bit?
        }

        public void MessageUserMessage()
        {
            bitBuffer.SeekBits(8); // id
            bitBuffer.SeekBits((Int32)bitBuffer.ReadUnsignedBits(11));
        }

        public void MessageGameEvent()
        {
            UInt32 length = bitBuffer.ReadUnsignedBits(11);
            UInt32 id = bitBuffer.ReadUnsignedBits(9);

            GameEvent gameEvent = (GameEvent)gameEventTable[id];

            if (gameEvent != null && gameEvent.Callback != null)
            {
                gameEvent.Callback();
            }
            else
            {
                bitBuffer.SeekBits((Int32)length - 9); // 9 is number of id bits (see above)
            }
        }

        public void MessagePacketEntities()
        {
            bitBuffer.SeekBits(11); // max

            if (bitBuffer.ReadBoolean()) // delta bit
            {
                bitBuffer.SeekBits(32); // delta
            }

            bitBuffer.SeekBits(12); // uk bit, 11 bits changed
            bitBuffer.SeekBits(1 + (Int32)bitBuffer.ReadUnsignedBits(20));
        }

        public void MessageTempEntities()
        {
            bitBuffer.SeekBits(8); // #
            bitBuffer.SeekBits((Int32)bitBuffer.ReadUnsignedBits(17));
        }

        public void MessagePrefetch()
        {
            bitBuffer.SeekBits(13);
        }

        public void MessageGameEventList()
        {
            Int32 nGameEvents = bitBuffer.ReadByte();

            bitBuffer.SeekBits(21);

            for (Int32 i = 0; i < nGameEvents; i++)
            {
                UInt32 id = bitBuffer.ReadUnsignedBits(9);
                String name = bitBuffer.ReadString();

                AddGameEvent(id, name);

                while (true)
                {
                    UInt32 entryType = bitBuffer.ReadUnsignedBits(3);

                    if (entryType == 0)
                    {
                        // end of event description
                        break;
                    }

                    bitBuffer.ReadString(); // entry name
                }
            }
        }

        public void MessageGetCvarValue()
        {
            bitBuffer.SeekBits(32); // cookie. Cookies? Who told you you could eat MY cookies? PUT DAT COOKIE DOWN, NOW!
            bitBuffer.ReadString(); // cvar name
        }
        #endregion
    }
}
