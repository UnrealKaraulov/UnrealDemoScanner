using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections;

namespace compLexity_Demo_Player
{
    public class SourceDemo : Demo
    {
        public class Player
        {
            public String Name { get; set; }
            public String SteamId { get; set; }
        }

        public const Int32 HeaderSizeInBytes = 1072;

        private SourceDemoParser parser = null;
        private Int32 currentStringTableIndex = 0;
        private Int32 userInfoStringTableIndex = -1;
        private Int32 userInfoStringTableEntryIndexBits = -1;
        private List<Player> playerList;
        private Single timeDeltaPerTick;

        #region Properties
        public override String EngineName
        {
            get
            {
                return "-";
            }
        }

        public List<Player> PlayerList
        {
            get
            {
                return playerList;
            }
        }

        public Single TimeDeltaPerTick
        {
            get
            {
                return timeDeltaPerTick;
            }
        }

        public Boolean Protocol15Hack { get; private set; }
        public Boolean UnsupportedDemoProtocol { get; private set; }
        public Boolean UnsupportedNetworkProtocol { get; private set; }
        #endregion

        public SourceDemo(String fileName)
        {
            fileFullPath = fileName;

            engineType = Engines.Source;
            status = StatusEnum.Ok;

            playerList = new List<Player>();
            Protocol15Hack = false;
            UnsupportedDemoProtocol = false;
            UnsupportedNetworkProtocol = false;
        }

        #region Reading
        protected override void ReadingThread()
        {
            try
            {
                ReadingThreadWorker();
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                String errorMessage = "Error reading demo file \"" + fileFullPath + "\".";

                if (UnsupportedDemoProtocol || UnsupportedNetworkProtocol)
                {
                    errorMessage += String.Format("\n\nProbable cause(s) for error:\n");

                    if (UnsupportedDemoProtocol)
                    {
                        errorMessage += String.Format("demo uses unsupported demo protocol \"{0}\"\n", demoProtocol);
                    }

                    if (UnsupportedNetworkProtocol)
                    {
                        errorMessage += String.Format("demo uses unsupported network protocol \"{0}\"\n", networkProtocol);
                    }
                }

                mainWindowInterface.Error(errorMessage, ex);
                demoListViewInterface.DemoLoadingFinished(null);
                return;
            }

            demoListViewInterface.DemoLoadingFinished(this);
        }

        private void ReadingThreadWorker()
        {
            FileStream fs = null;
            BinaryReader br = null;

            try
            {
                fs = File.OpenRead(FileFullPath);
                br = new BinaryReader(fs);

                // read header
                if (fs.Length < HeaderSizeInBytes)
                {
                    throw new ApplicationException("File length is too short to parse the header.");
                }

                ReadHeader(br);
            }
            finally
            {
                if (br != null)
                {
                    br.Close();
                }

                if (fs != null)
                {
                    fs.Close();
                }
            }

            // don't try to parse messages if the demo uses an unsupported network protocol
            if (UnsupportedNetworkProtocol)
                return;

            try
            {
                // initialise parser
                parser = new SourceDemoParser(this);
                parser.AddMessageHandler((Byte)SourceDemoParser.MessageId.SVC_Print, ReadMessagePrint);
                parser.AddMessageHandler((Byte)SourceDemoParser.MessageId.SVC_ServerInfo, ReadMessageServerInfo);
                parser.AddMessageHandler((Byte)SourceDemoParser.MessageId.SVC_CreateStringTable, ReadMessageCreateStringTable);
                parser.AddMessageHandler((Byte)SourceDemoParser.MessageId.SVC_UpdateStringTable, ReadMessageUpdateStringTable);
                parser.Open();
                parser.Seek(HeaderSizeInBytes);

                Boolean firstSignonFrameParsed = false;

                while (true)
                {
                    // frame header
                    SourceDemoParser.FrameHeader frameHeader = parser.ReadFrameHeader();

                    // special cases - additional frame header info
                    if (frameHeader.Type == SourceDemoParser.FrameType.Stop || frameHeader.Type == SourceDemoParser.FrameType.Packet)
                    {
                        break;
                    }
                    else if (frameHeader.Type == SourceDemoParser.FrameType.Signon)
                    {
                        // command info
                        parser.ReadCommandInfo();

                        // sequence info
                        parser.ReadSequenceInfo();
                    }
                    else if (frameHeader.Type == SourceDemoParser.FrameType.User)
                    {
                        parser.Seek(4); // outgoing sequence number
                    }

                    // frame data
                    if (frameHeader.Type != SourceDemoParser.FrameType.Synctick)
                    {
                        // get frame length
                        Int32 frameLength = parser.Reader.ReadInt32();

                        if (frameLength != 0)
                        {
                            if (frameHeader.Type == SourceDemoParser.FrameType.Signon)
                            {
                                try
                                {
                                    if (!firstSignonFrameParsed)
                                    {
                                        firstSignonFrameParsed = true;

                                        if (networkProtocol == 15)
                                        {
                                            Int64 start = parser.Position;

                                            try
                                            {
                                                CalculateProtocol15Hack(frameLength);
                                            }
                                            catch (Exception)
                                            {
                                                Protocol15Hack = true;
                                            }

                                            parser.Seek(start, SeekOrigin.Begin);
                                        }
                                    }

                                    parser.ParsePacketMessages(frameLength);
                                }
                                catch (ThreadAbortException)
                                {
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    throw new ApplicationException("Message parsing error.\n\n" + parser.ComputeMessageLog(), ex);
                                }
                            }
                            else
                            {
                                parser.Seek(frameLength);
                            }
                        }
                    }
                }
            }
            finally
            {
                parser.Close();
            }
        }

        private void CalculateProtocol15Hack(Int32 dataLength)
        {
            /*
             * Valve broke network protocol 15 by changing the protocol without incrementing the protocol number.
             * 
             * The change adds an extra bit after each message ID, so the only thing that's guaranteed to read correctly with every protocol 15 demo now if the header.
             * 
             * The first message in the first sigon frame is always svc_print or svc_serverinfo, so if either of those are invalid, odds are that the demo uses the changed protocol.
             * 
             */

            // read in data block
            Byte[] frameData = parser.Reader.ReadBytes(dataLength);
            BitBuffer bitBuffer = new BitBuffer(frameData);

            // parse messages
            SourceDemoParser.MessageId messageId = (SourceDemoParser.MessageId)bitBuffer.ReadUnsignedBits(5);

            if (messageId == SourceDemoParser.MessageId.SVC_Print)
            {
                String s = bitBuffer.ReadString();

                if (s.Contains("Map") || s.Contains("Build") || s.Contains("Players"))
                {
                    // Looks like a valid svc_print string.
                    return;
                }

                Protocol15Hack = true;
            }
            else if (messageId == SourceDemoParser.MessageId.SVC_ServerInfo)
            {
                UInt32 networkProtocol = bitBuffer.ReadUnsignedBits(16);

                if (networkProtocol != this.networkProtocol)
                {
                    // Should match header, must be invalid.
                    Protocol15Hack = true;
                }
            }
        }

        private void ReadHeader(BinaryReader br)
        {
            br.BaseStream.Seek(8, SeekOrigin.Begin); // skip magic, DemoFactory checks it

            demoProtocol = br.ReadUInt32();

            if (demoProtocol > 3)
            {
                // the format of the rest of the header may have changed, but continue anyway since it's not critical information
                UnsupportedDemoProtocol = true;
            }

            networkProtocol = br.ReadUInt32();

            if (networkProtocol > 18)
            {
                UnsupportedNetworkProtocol = true;
            }

            serverName = Common.ReadNullTerminatedString(br, 260);
            recorderName = Common.ReadNullTerminatedString(br, 260);
            mapName = Common.ReadNullTerminatedString(br, 260).ToLower();
            gameFolderName = Common.ReadNullTerminatedString(br, 260).ToLower();
            durationInSeconds = Math.Abs(br.ReadSingle());

            if (recorderName == "SourceTV Demo")
            {
                perspective = Perspectives.SourceTv;
            }
            else
            {
                perspective = Perspectives.Pov;
            }

            Game = GameManager.Find(this);
        }

        private void ParseUserInfo()
        {
            if (NetworkProtocol >= 14)
            {
                parser.BitBuffer.SeekBits(2);
            }

            Player player = new Player();

            player.Name = parser.BitBuffer.ReadString(32);
            parser.BitBuffer.SeekBits(32); // player id
            player.SteamId = parser.BitBuffer.ReadString(33);

            // ignore the rest

            playerList.Add(player);
        }

        // TODO: regex
        private void ReadMessagePrint()
        {
            String s = parser.BitBuffer.ReadString();

            if (perspective == Perspectives.SourceTv)
            {
                return;
            }

            if (buildNumber == 0)
            {
                String buildString = "Build ";
                Int32 buildIndex = s.IndexOf(buildString);

                if (buildIndex == -1)
                {
                    // Try an alternate format.
                    buildString = "Build: ";
                    buildIndex = s.IndexOf(buildString);
                }

                if (buildIndex != -1)
                {
                    String temp = s.Remove(0, buildIndex + buildString.Length);
                    Int32 newLineIndex = temp.IndexOf('\n');

                    if (newLineIndex != -1)
                    {
                        temp = temp.Remove(newLineIndex, temp.Length - newLineIndex);
                        buildNumber = Convert.ToInt32(temp);
                    }
                }
            }

            if (maxClients == 0)
            {
                Int32 slotsIndex = s.IndexOf("Players: ");

                if (slotsIndex != -1)
                {
                    String temp = s.Remove(0, slotsIndex + 9);
                    Int32 slashIndex = temp.IndexOf('/');

                    if (slashIndex != -1)
                    {
                        temp = temp.Remove(0, slashIndex + 2);
                        Int32 newLineIndex = temp.IndexOf('\n');

                        if (newLineIndex != -1)
                        {
                            temp = temp.Remove(newLineIndex, temp.Length - newLineIndex);
                            maxClients = Convert.ToByte(temp);
                        }
                    }
                }
            }
        }

        public void ReadMessageServerInfo()
        {
            parser.BitBuffer.SeekBits(16); // network protocol (same as header)
            parser.BitBuffer.SeekBits(32); // spawn count

            if (NetworkProtocol >= 18)
            {
                parser.BitBuffer.SeekBits(186); // ...
            }
            else
            {
                parser.BitBuffer.SeekBits(90); // ...
            }

            maxClients = parser.BitBuffer.ReadByte();
            timeDeltaPerTick = parser.BitBuffer.ReadSingle();

            parser.BitBuffer.SeekBits(8); // server type char
            parser.BitBuffer.ReadString(); // game dir (same as header)
            parser.BitBuffer.ReadString(); // map name (same as header - unless there's a map change?)
            parser.BitBuffer.ReadString(); // sky name

            // server name (different from header - header is usually the address)
            // if this message is never parsed, the header value is used, otherwise it is expanded upon.
            serverName = parser.BitBuffer.ReadString() + " (" + serverName + ")";

            if (NetworkProtocol >= 16)
            {
                parser.BitBuffer.SeekBits(1);
            }
        }

        public void ReadMessageCreateStringTable()
        {
            if (NetworkProtocol >= 15)
            {
                parser.MessageCreateStringTable();
                return;
            }

            String tableName = parser.BitBuffer.ReadString();
            Int32 maxEntries = (Int32)parser.BitBuffer.ReadUnsignedBits(16); // TODO: sanity check on maxEntries?
            Int32 entriesBits = Common.LogBase2(maxEntries) + 1;
            Int32 nEntries = (Int32)parser.BitBuffer.ReadUnsignedBits(entriesBits);
            Int32 nBits = (Int32)parser.BitBuffer.ReadUnsignedBits(20);

            UInt32 userDataSize = 0;

            if (parser.BitBuffer.ReadBoolean()) // userdata bit
            {
                userDataSize = parser.BitBuffer.ReadUnsignedBits(12);
                parser.BitBuffer.SeekBits(4); // "user data bits"
            }

            if (tableName == "userinfo")
            {
                userInfoStringTableIndex = currentStringTableIndex;
                userInfoStringTableEntryIndexBits = entriesBits;

                for (int i = 0; i < nEntries; i++)
                {
                    parser.BitBuffer.SeekBits(2); // unknown

                    if (parser.BitBuffer.ReadBoolean()) // delta bit
                    {
                        parser.BitBuffer.SeekBits(10); // 5 bits history index, 5 bits history length
                        parser.BitBuffer.ReadString(); // delta entry
                    }
                    else
                    {
                        parser.BitBuffer.ReadString(); // entry
                    }

                    if (userDataSize == 0) // ??? could be wrong
                    {
                        if (parser.BitBuffer.ReadBoolean()) // userdata bit
                        {
                            // parse userinfo
                            UInt32 nUserDataBytes = parser.BitBuffer.ReadUnsignedBits(12);

                            if (nUserDataBytes > 0)
                            {
                                Int32 currentOffset = parser.BitBuffer.CurrentBit;
                                ParseUserInfo();
                                parser.BitBuffer.SeekBits(currentOffset + (Int32)nUserDataBytes * 8, SeekOrigin.Begin);
                            }

                            if (NetworkProtocol >= 14)
                            {
                                parser.BitBuffer.SeekBits(2); // unknown
                            }
                        }
                    }
                    else
                    {
                        parser.BitBuffer.SeekBits(3); // unknown
                    }
                }
            }
            else
            {
                parser.BitBuffer.SeekBits(nBits);
            }

            currentStringTableIndex++;
        }

        public void ReadMessageUpdateStringTable()
        {
            UInt32 tableId;

            if (DemoProtocol <= 2)
            {
                tableId = parser.BitBuffer.ReadUnsignedBits(4);
            }
            else
            {
                tableId = parser.BitBuffer.ReadUnsignedBits(5);
            }

            UInt32 nEntries = 1;

            if (parser.BitBuffer.ReadBoolean()) // nEntries bit
            {
                nEntries = parser.BitBuffer.ReadUnsignedBits(16);
            }

            UInt32 nBits;

            if (NetworkProtocol >= 14)
            {
                nBits = parser.BitBuffer.ReadUnsignedBits(20);
            }
            else
            {
                nBits = parser.BitBuffer.ReadUnsignedBits(16);
            }

            if (tableId == userInfoStringTableIndex)
            {
                if (userInfoStringTableEntryIndexBits == -1)
                {
                    throw new ApplicationException("SVC_UpdateStringTable without SVC_CreateStringTable first (userinfo).");
                }

                for (int i = 0; i < nEntries; i++)
                {
                    if (!parser.BitBuffer.ReadBoolean()) // relative index bit
                    {
                        parser.BitBuffer.SeekBits(userInfoStringTableEntryIndexBits - 1);
                    }

                    if (parser.BitBuffer.ReadBoolean()) // name bit
                    {
                        parser.BitBuffer.SeekBits(1); // unknown
                        parser.BitBuffer.ReadString();
                    }

                    if (parser.BitBuffer.ReadBoolean()) // userdata bit
                    {
                        UInt32 nUserDataBytes = parser.BitBuffer.ReadUnsignedBits(12);

                        if (nUserDataBytes > 0)
                        {
                            Int32 currentOffset = parser.BitBuffer.CurrentBit;
                            ParseUserInfo();
                            parser.BitBuffer.SeekBits(currentOffset + (Int32)nUserDataBytes * 8, SeekOrigin.Begin);
                        }
                    }
                }
            }
            else
            {
                parser.BitBuffer.SeekBits((Int32)nBits);
            }
        }

        #endregion

        #region Writing
        protected override void WritingThread(object _destinationFileName)
        {
            try
            {
                WritingThreadWorker((String)_destinationFileName);
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                String errorMessage = "Error writing demo file \"" + fileFullPath + "\".";

                if (UnsupportedDemoProtocol || UnsupportedNetworkProtocol)
                {
                    errorMessage += String.Format("\n\nProbable cause(s) for error:\n");

                    if (UnsupportedDemoProtocol)
                    {
                        errorMessage += String.Format("demo uses unsupported demo protocol \"{0}\"\n", demoProtocol);
                    }

                    if (UnsupportedNetworkProtocol)
                    {
                        errorMessage += String.Format("demo uses unsupported network protocol \"{0}\"\n", networkProtocol);
                    }
                }

                writeProgressWindowInterface.Error(errorMessage, ex, false, null);
                writeProgressWindowInterface.CloseWithResult(false);
                return;
            }

            writeProgressWindowInterface.CloseWithResult(true);
        }

        private void WritingThreadWorker(String destinationFileName)
        {
            // don't know the demo format, just do a straight file copy
            if (UnsupportedDemoProtocol)
            {
                using (FileStream destStream = File.Open(destinationFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (FileStream sourceStream = File.OpenRead(fileFullPath))
                    {
                        Byte[] buffer = new Byte[1024 * 1024];
                        Int32 percentRead = 0;

                        while (true)
                        {
                            Int32 bytesRead = sourceStream.Read(buffer, 0, buffer.Length);

                            if (bytesRead == 0)
                                break;

                            destStream.Write(buffer, 0, bytesRead);

                            // calculate what percent of the file has been read
                            Int32 oldPercentRead = percentRead;

                            percentRead = (Int32)(sourceStream.Position / (Single)sourceStream.Length * 100.0f);

                            if (percentRead != oldPercentRead)
                            {
                                writeProgressWindowInterface.UpdateProgress(percentRead);
                            }
                        }
                    }
                }

                return;
            }

            // create output file
            FileStream stream = File.Open(destinationFileName, FileMode.Create, FileAccess.Write, FileShare.None);
            BinaryWriter writer = new BinaryWriter(stream);

            try
            {
                // initialise parser
                parser = new SourceDemoParser(this);
                parser.AddMessageHandler((Byte)SourceDemoParser.MessageId.SVC_UserMessage, WriteMessageUserMessage);
                parser.Open();

                Int32 percentRead = 0;

                // header
                Byte[] header = parser.Reader.ReadBytes(HeaderSizeInBytes);
                writer.Write(header);

                while (true)
                {
                    // frame header
                    SourceDemoParser.FrameHeader frameHeader = parser.ReadFrameHeader();
                    writer.Write((Byte)frameHeader.Type);

                    if (frameHeader.Type != SourceDemoParser.FrameType.Stop)
                    {
                        writer.Write(frameHeader.Tick);
                    }
                    else
                    {
                        // write 3 bytes
                        writer.Write((Byte)(frameHeader.Tick & 0xFF));
                        writer.Write((Byte)((frameHeader.Tick >> 8) & 0xFF));
                        writer.Write((Byte)((frameHeader.Tick >> 16) & 0xFF));
                    }

                    // special cases - additional frame header info
                    if (frameHeader.Type == SourceDemoParser.FrameType.Stop)
                    {
                        break;
                    }
                    else if (frameHeader.Type == SourceDemoParser.FrameType.Signon || frameHeader.Type == SourceDemoParser.FrameType.Packet)
                    {
                        // command info
                        parser.WriteCommandInfo(parser.ReadCommandInfo(), writer);

                        // sequence info
                        parser.WriteSequenceInfo(parser.ReadSequenceInfo(), writer);
                    }
                    else if (frameHeader.Type == SourceDemoParser.FrameType.User)
                    {
                        Int32 outgoingSequence = parser.Reader.ReadInt32();
                        writer.Write(outgoingSequence);
                    }

                    // frame data
                    if (frameHeader.Type != SourceDemoParser.FrameType.Synctick)
                    {
                        // get frame length
                        Int32 frameLength = parser.Reader.ReadInt32();
                        writer.Write(frameLength);

                        if (frameLength != 0)
                        {
                            Byte[] frameData = null;

                            if (DemoProtocol == 3 && NetworkProtocol == 7 && Config.Settings.PlaybackRemoveFtb && frameHeader.Type == SourceDemoParser.FrameType.Packet)
                            {
                                // fade to black removal
                                parser.ParsePacketMessages(frameLength);
                                frameData = parser.BitBuffer.Data;
                            }
                            else
                            {
                                frameData = parser.Reader.ReadBytes(frameLength);
                            }

                            writer.Write(frameData);
                        }
                    }

                    // calculate what percent of the file has been read
                    Int32 oldPercentRead = percentRead;

                    percentRead = (Int32)(parser.Position / (Single)parser.FileLength * 100.0f);

                    if (percentRead != oldPercentRead)
                    {
                        writeProgressWindowInterface.UpdateProgress(percentRead);
                    }
                }
            }
            finally
            {
                parser.Close();
                writer.Close();
                stream.Close();
            }
        }

        private void WriteMessageUserMessage()
        {
            UInt32 id = parser.BitBuffer.ReadUnsignedBits(8);
            UInt32 length = parser.BitBuffer.ReadUnsignedBits(11);
            Int32 endPosition = parser.BitBuffer.CurrentBit + (Int32)length;

            if (id == 12 && length == 80) // ScreenFade
            {
                // make sure it's FTB
                UInt32 duration = parser.BitBuffer.ReadUnsignedBits(16);
                UInt32 holdTime = parser.BitBuffer.ReadUnsignedBits(16);
                UInt32 flags = parser.BitBuffer.ReadUnsignedBits(16);
                Byte r = parser.BitBuffer.ReadByte();
                Byte g = parser.BitBuffer.ReadByte();
                Byte b = parser.BitBuffer.ReadByte();
                Byte a = parser.BitBuffer.ReadByte();

                if (duration == 1536 && holdTime == 1536 && flags == 10 && r == 0 && g == 0 && b == 0 && a == 255)
                {
                    parser.BitBuffer.SeekBits(-80);
                    parser.BitBuffer.ZeroOutBits(80);
                }
            }

            parser.BitBuffer.SeekBits(endPosition, SeekOrigin.Begin);
        }
        #endregion
    }
}
