using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections; // ArrayList
using System.Threading;
using System.Collections.Specialized;

namespace compLexity_Demo_Player
{
    public class HalfLifeDemo : Demo
    {
        public class Player
        {
            public Byte Slot { get; set; } // svc_updateuserinfo messages can change a player's slot.
            public Int32 Id { get; private set; }
            public StringDictionary InfoKeys { get; private set; }

            public Player(Byte slot, Int32 id)
            {
                Slot = slot;
                Id = id;
                InfoKeys = new StringDictionary();
            }
        }

        public enum EngineVersions
        {
            Unknown,
            HalfLife1104,
            HalfLife1106,
            HalfLife1107,
            HalfLife1108,
            HalfLife1109,
            HalfLife1108or1109,
            HalfLife1110,
            HalfLife1111, // Steam
            HalfLife1110or1111
        }

        public const Byte CurrentNetworkProtocol = 48;

        public const Int32 HeaderSizeInBytes = 544;
        public const Int32 DirectoryEntrySizeInBytes = 92;

        private Int64 fileLengthInBytes;
        private Byte recorderSlot;
        private List<Player> playerList = new List<Player>();
        private EngineVersions engineVersion = EngineVersions.Unknown;
        private UInt32 mungedMapChecksum;

        private HalfLifeDemoParser parser;

        // duplicate loading segments bug
        private Int32 currentFrameIndex;
        private Int32 firstFrameToWriteIndex;

        // "no loading segment" bug
        // GotFrag Demo 16977 (moon vs Catch-Gamer).zip
        private Boolean serverInfoParsed = false;

        #region Properties
        public override String EngineName
        {
            get
            {
                String s = "Half-Life v";
                
                switch (engineVersion)
                {
                    case EngineVersions.HalfLife1104:
                        s += "1.1.0.4";
                        break;

                    case EngineVersions.HalfLife1106:
                        s += "1.1.0.6";
                        break;

                    case EngineVersions.HalfLife1107:
                        s += "1.1.0.7";
                        break;

                    case EngineVersions.HalfLife1108:
                        s += "1.1.0.8";
                        break;

                    case EngineVersions.HalfLife1109:
                        s += "1.1.0.9";
                        break;

                    case EngineVersions.HalfLife1108or1109:
                        s += "1.1.0.8 or v1.1.0.9";
                        break;

                    case EngineVersions.HalfLife1110:
                        s += "1.1.1.0";
                        break;

                    case EngineVersions.HalfLife1111:
                        s += "1.1.1.1";
                        break;

                    case EngineVersions.HalfLife1110or1111:
                        s += "1.1.1.0 or v1.1.1.1";
                        break;

                    default:
                        return "Half-Life Unknown Version";
                }

                return s;
            }
        }

        public List<Player> Players
        {
            get
            {
                return playerList;
            }
        }
        #endregion

        public HalfLifeDemo(String fileName)
        {
            fileFullPath = fileName;

            // initialise variables not guaranteed to be initialised by reading the file
            recorderName = "";
            status = StatusEnum.Ok;
            perspective = Perspectives.Pov;
        }

        #region Misc
        /// <summary>
        /// Returns true if the demo should be converted to the current network protocol during writing.
        /// </summary>
        /// <returns></returns>
        public Boolean ConvertNetworkProtocol()
        {
            if (GameFolderName != "cstrike")
            {
                return false;
            }

            if (IsBetaSteam())
            {
                // Return true, although most messages shouldn't be converted - just the network protocol numbers in the header, svc_serverinfo and user message id's (since they're different). Messages should call IsBetaSteam and handle this issue themselves.
                return true;
            }

            if (NetworkProtocol >= 43 && NetworkProtocol <= 46 && (Config.Settings.PlaybackProgramOldCs == ProgramSettings.PlaybackProgram.CounterStrike || Config.Settings.PlaybackProgramOldCs == ProgramSettings.PlaybackProgram.Steam))
            {
                return true;
            }

            return false;
        }

        public Boolean ConvertToCurrentNetworkProtocol()
        {
            return (ConvertNetworkProtocol() || (NetworkProtocol == 47 && Config.Settings.PlaybackProgramOldCs == ProgramSettings.PlaybackProgram.Steam));
        }

        public Boolean IsBetaSteam()
        {
            if (NetworkProtocol != 46)
            {
                // workaround for johnny r pov on inferno against ocrana
                // newer protocol than usual beta demos but same messed up usermessage indicies...
                if (NetworkProtocol == 47 && Perspective == Perspectives.Pov && BuildNumber <= 2573)
                {
                    return true;
                }
                else if (NetworkProtocol == 47 && Perspective == Perspectives.Hltv)
                {
                    // Different user message indicies.
                    // http://www.sk-gaming.com/file/2047-SKswe_vs_gmpo
                    return true;
                }

                return false;
            }

            if (Game == null)
            {
                // Unknown game, can't determine whether it's a beta Steam demo, so assume it isn't.
                return false;
            }

            // May be beta Steam HLTV. This can only determine via the client.dll checksum (i.e. this is mod specific).
            return Game.IsBetaSteamHltvDemo(this);
        }
        #endregion

        #region Map Checksum Un-munging
        private static Byte[] MungeTable3 =
        {
            0x20, 0x07, 0x13, 0x61,
            0x03, 0x45, 0x17, 0x72,
            0x0A, 0x2D, 0x48, 0x0C,
            0x4A, 0x12, 0xA9, 0xB5
        };

        private static UInt32 FlipBytes32(UInt32 value)
        {
            return (((value & 0xFF000000) >> 24) | ((value & 0x00FF0000) >> 8) | ((value & 0x0000FF00) << 8) | ((value & 0x000000FF) << 24));
        }

        private static UInt32 UnMunge3(UInt32 value, Int32 z)
        {
            z = (0xFF - z) & 0xFF;
            value = (UInt32)(value ^ z);

            Byte[] temp = BitConverter.GetBytes(value);

            for (Int32 i = 0; i < 4; i++)
            {
                temp[i] ^= (Byte)((((Int32)MungeTable3[i & 0x0F] | i << i) | i) | 0xA5);
            }

            return (UInt32)(FlipBytes32(BitConverter.ToUInt32(temp, 0)) ^ (~z));
        }
        #endregion

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
                mainWindowInterface.Error("Error reading demo file \"" + FileFullPath + "\"", ex);
                demoListViewInterface.DemoLoadingFinished(null);
                return;
            }

            demoListViewInterface.DemoLoadingFinished(this);
        }

        private void ReadingThreadWorker()
        {
            FileStream fs = null;
            BinaryReader br = null;

            // read header
            try
            {
                fs = File.OpenRead(FileFullPath);
                br = new BinaryReader(fs);

                fileLengthInBytes = fs.Length;

                if (fileLengthInBytes < HeaderSizeInBytes)
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

            // create parser
            parser = new HalfLifeDemoParser(this);

            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_setview, ReadMessageSetView);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_print, ReadMessagePrint);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_serverinfo, ReadMessageServerInfo);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_updateuserinfo, ReadMessageUpdateUserInfo);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_hltv, ReadMessageHltv);

            parser.Open();
            parser.Seek(HeaderSizeInBytes); // seek past header
                
            try
            {
                // read and parse frames until the end of the loading segment
                while (true)
                {
                    HalfLifeDemoParser.FrameHeader frameHeader = parser.ReadFrameHeader();

                    // "no loading segment" bug
                    if (frameHeader.Type == 1)
                    {
                        if (serverInfoParsed)
                        {
                            break;
                        }
                    }

                    if (frameHeader.Type == 0 || frameHeader.Type == 1)
                    {
                        HalfLifeDemoParser.GameDataFrameHeader gameDataFrameHeader = parser.ReadGameDataFrameHeader();

                        if (gameDataFrameHeader.Length > 0)
                        {
                            Byte[] frameData = parser.Reader.ReadBytes((Int32)gameDataFrameHeader.Length);

                            try
                            {
                                parser.ParseGameDataMessages(frameData);
                            }
                            catch (ThreadAbortException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                throw new ApplicationException("Error parsing gamedata frame.\n\n" + parser.ComputeMessageLog(), ex);
                            }
                        }
                    }
                    else
                    {
                        parser.SkipFrame(frameHeader.Type);
                    }
                }
            }
            finally
            {
                parser.Close();
            }

            // get demo recorder's name
            if (perspective == Perspectives.Pov)
            {
                foreach (Player p in playerList)
                {
                    if (p.Slot == recorderSlot)
                    {
                        recorderName = p.InfoKeys["name"];
                    }
                }
            }
        }

        private void ReadHeader(BinaryReader br)
        {
            br.BaseStream.Seek(8, SeekOrigin.Current); // skip magic, DemoFactory checks it
            
            demoProtocol = br.ReadUInt32();

            if (demoProtocol != 5)
            {
                throw new ApplicationException(String.Format("Unknown demo protocol \"{0}\", should be 5.", demoProtocol));
            }

            networkProtocol = br.ReadUInt32();

            if (networkProtocol < 43) // don't support demos older than HL 1.1.0.4/CS 1.0
            {
                throw new ApplicationException(String.Format("Unsupported network protcol \"{0}\", only 43 and higher are supported."));
            }

            mapName = Common.ReadNullTerminatedString(br, 260).ToLower();
            gameFolderName = Common.ReadNullTerminatedString(br, 260).ToLower();
            mapChecksum = br.ReadUInt32();
            Int64 directoryEntriesOffset = br.ReadUInt32();
       
            // check directory entries

            // check offset, should be exactly file length - no. of dir entries (int) + dir entry size * 2
            // otherwise, assume they are corrupt
            if (directoryEntriesOffset != fileLengthInBytes - 4 - (DirectoryEntrySizeInBytes * 2))
            {
                status = StatusEnum.CorruptDirEntries;
                return;
            }

            // seek to directory entries offset
            Int64 newPosition = br.BaseStream.Seek(directoryEntriesOffset, SeekOrigin.Begin); // CHECK ME: is this correct or is an exception thrown if we seek to far?

            if (newPosition != directoryEntriesOffset)
            {
                status = StatusEnum.CorruptDirEntries;
                return;
            }
            
            // read no. of directory entries
            Int32 nDirectoryEntries = br.ReadInt32();

            if (nDirectoryEntries != 2)
            {
                status = StatusEnum.CorruptDirEntries;
                return;
            }

            // read directory entries
            for (Int32 i = 0; i < nDirectoryEntries; i++)
            {
                br.BaseStream.Seek(4, SeekOrigin.Current); // skip number
                String dirEntryTitle = Common.ReadNullTerminatedString(br, 64);
                br.BaseStream.Seek(8, SeekOrigin.Current); // skip flags, cdtrack
                Single dirEntryTime = br.ReadSingle();
                br.BaseStream.Seek(12, SeekOrigin.Current); // skip frames, offset and length (we calculate these ourselves, so corrupt directory entries or not, the demo is treated the same)

                if (dirEntryTitle.ToLower() == "playback")
                {
                    // store demo duration
                    durationInSeconds = Math.Abs(dirEntryTime);
                }
            }
        }

        /// <summary>
        /// Calculates the engine version and type based on the network protocol and build numbers. CalculateGameVersion sould be called before this becaues of issues with HLTV demos and beta Steam demos.
        /// </summary>
        private void CalculateEngineVersionAndType()
        {
            engineVersion = EngineVersions.Unknown;
            engineType = Engines.HalfLife;

            if (networkProtocol == 43)
            {
                if (buildNumber >= 1712)
                {
                    engineVersion = EngineVersions.HalfLife1107;
                }
                else if (buildNumber >= 1600)
                {
                    engineVersion = EngineVersions.HalfLife1106;
                }
                else if (buildNumber >= 1460)
                {
                    engineVersion = EngineVersions.HalfLife1104;
                }
            }
            else if (networkProtocol == 45)
            {
                if (Perspective == Perspectives.Hltv)
                {
                    engineVersion = EngineVersions.HalfLife1108or1109;
                }
                else if (buildNumber >= 2006)
                {
                    engineVersion = EngineVersions.HalfLife1109;
                }
                else
                {
                    engineVersion = EngineVersions.HalfLife1108;
                }
            }
            else if (networkProtocol == 46)
            {
                if (IsBetaSteam())
                {
                    engineVersion = EngineVersions.HalfLife1111;
                    engineType = Engines.HalfLifeSteam;
                }
                else if (Perspective == Perspectives.Hltv)
                {
                    engineVersion = EngineVersions.HalfLife1110or1111;
                }
                else
                {
                    engineVersion = EngineVersions.HalfLife1110;
                }
            }
            else if (networkProtocol >= 47)
            {
                engineVersion = EngineVersions.HalfLife1111;
                engineType = Engines.HalfLifeSteam;
            }
        }

        private void CalculateGameAndGameVersion()
        {
            Game = GameManager.Find(this);

            if (Game != null)
            {
                GameVersion = Game.FindVersion(clientDllChecksum);
            }
        }
        #endregion

        #region Writing

        /// <summary>
        /// Writes the demo to the destination folder while performing modifications such as removing the scoreboard or fade to black, possibly converting messages to the current network protocol, as well as re-writing directory entries.
        /// </summary>
        /// <param name="_destinationPath">The destination folder.</param>
        protected override void WritingThread(object _destinationFileName)
        {
            firstFrameToWriteIndex = 0;

            try
            {
                /*
                 * Converted demos: pre-process the loading segment and get the frame index of the last 
                 * svc_serverinfo message in the loading segment.
                 * 
                 * This fixes several bugs:
                 *      1. long (for Half-Life) loading times, since the resources of several maps may be
                 *      loaded.
                 *      
                 *      2. wrong map in resource list
                 *      
                 *      3. random SendAudio CTD (indirectly)
                 */
                if (ConvertNetworkProtocol() && !IsBetaSteam())
                {
                    currentFrameIndex = 0;

                    // initialise parser
                    parser = new HalfLifeDemoParser(this);
                    parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_serverinfo, PreWriteMessageServerInfo);
                    parser.Open();

                    try
                    {
                        parser.Seek(HeaderSizeInBytes);

                        while (true)
                        {
                            HalfLifeDemoParser.FrameHeader frameHeader = parser.ReadFrameHeader();

                            if (frameHeader.Type == 1)
                            {
                                break;
                            }

                            if (frameHeader.Type == 0)
                            {
                                HalfLifeDemoParser.GameDataFrameHeader gameDataFrameHeader = parser.ReadGameDataFrameHeader();
                                Byte[] frameData = parser.Reader.ReadBytes((Int32)gameDataFrameHeader.Length);
                                parser.ParseGameDataMessages(frameData);
                            }
                            else if (frameHeader.Type != 5)
                            {
                                parser.SkipFrame(frameHeader.Type);
                            }

                            currentFrameIndex++;
                        }
                    }
                    finally
                    {
                        parser.Close();
                    }
                }

                // demo writer
                HalfLifeDemoConverter demoConverter = new HalfLifeDemoConverter(this);
                HalfLifeDemoWriter demoWriter = new HalfLifeDemoWriter(this, (IHalfLifeDemoWriter)demoConverter, writeProgressWindowInterface, firstFrameToWriteIndex);

                demoWriter.ThreadWorker((String)_destinationFileName);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (HalfLifeDemoWriter.AbortWritingException)
            {
                writeProgressWindowInterface.CloseWithResult(false);
                return;
            }
            catch (Exception ex)
            {
                writeProgressWindowInterface.Error("Error writing demo file \"" + fileFullPath + "\".", ex, false, null);
                writeProgressWindowInterface.CloseWithResult(false);
                return;
            }

            writeProgressWindowInterface.CloseWithResult(true);
        }
        #endregion

        #region Read Message Handlers
        private void ReadMessageSetView()
        {
            UInt16 edict = parser.BitBuffer.ReadUInt16();

            if (mapChecksum == 0)
            {
                mapChecksum = UnMunge3(mungedMapChecksum, (Int32)edict - 1);
            }
        }

        private void ReadMessagePrint()
        {
            String s = parser.BitBuffer.ReadString();

            // get build number
            // FIXME: use regex?
            if (perspective == Perspectives.Pov && s.Contains("BUILD"))
            {
                s = s.Remove(0, 2);
                s = s.Replace("BUILD ", "");
                s = s.Remove(s.IndexOf(' '));

                buildNumber = Convert.ToInt32(s);
            }
        }

        private void ReadMessageServerInfo()
        {
            parser.Seek(4); // network protocol
            parser.Seek(4); // process count
            mungedMapChecksum = parser.BitBuffer.ReadUInt32();

            // read client.dll checksum
            Byte[] checksum = parser.BitBuffer.ReadBytes(16);

            // convert client.dll checksum to string
            StringBuilder sb = new StringBuilder();

            for (Int32 i = 0; i < checksum.Length; i++)
            {
                sb.Append(checksum[i].ToString("X2"));
            }

            clientDllChecksum = sb.ToString();

            // determine engine and game versions
            CalculateGameAndGameVersion();
            CalculateEngineVersionAndType();

            maxClients = parser.BitBuffer.ReadByte();
            recorderSlot = parser.BitBuffer.ReadByte();

            // skip unknown byte, game folder
            parser.Seek(1);
            parser.BitBuffer.ReadString();

            // see base handler
            if (networkProtocol > 43)
            {
                serverName = parser.BitBuffer.ReadString();
            }
            else
            {
                serverName = "Unknown";
            }

            // skip map
            parser.BitBuffer.ReadString();

            if (NetworkProtocol == 45)
            {
                Byte extraInfo = parser.BitBuffer.ReadByte();
                parser.Seek(-1);

                if (extraInfo != (Byte)HalfLifeDemoParser.MessageId.svc_sendextrainfo)
                {
                    parser.BitBuffer.ReadString(); // skip mapcycle

                    if (parser.BitBuffer.ReadByte() > 0)
                    {
                        parser.Seek(36);
                    }
                }
            }
            else
            {
                parser.BitBuffer.ReadString(); // skip mapcycle

                if (NetworkProtocol > 43)
                {
                    if (parser.BitBuffer.ReadByte() > 0)
                    {
                        parser.Seek(21);
                    }
                }
            }

            // "no loading segment" bug
            serverInfoParsed = true;
        }

        private void ReadMessageUpdateUserInfo()
        {
            Byte slot = parser.BitBuffer.ReadByte();
            Int32 id = parser.BitBuffer.ReadInt32();
            String s = parser.BitBuffer.ReadString();

            if (NetworkProtocol > 43)
            {
                parser.Seek(16);
            }

            if (s.Length == 0)
            {
                // 0 length text = a player just left and another player's slot is being changed
                // TODO: ?
                return;
            }

            Player player = null;

            // see if player with matching id exists
            foreach (Player p in playerList)
            {
                if (p.Id == id)
                {
                    player = p;
                    break;
                }
            }
              
            // create player if it doesn't exist
            if (player == null)
            {
                player = new Player(slot, id);
                playerList.Add(player);
            }

            if (s.Length == 0)
            {
                // 0 length text = a player just left and another player's slot is being changed
                player.Slot = slot;
                return;
            }

            // parse infokey string
            s = s.Remove(0, 1); // trim leading slash
            string[] infoKeyTokens = s.Split('\\');

            for (Int32 i = 0; i < infoKeyTokens.Length; i += 2)
            {
                if (i + 1 >= infoKeyTokens.Length)
                {
                    // Must be an odd number of strings - a key without a value - ignore it.
                    break;
                }

                String key = infoKeyTokens[i];

                // If the key already exists, overwrite it.
                if (player.InfoKeys.ContainsKey(key))
                {
                    player.InfoKeys.Remove(key);
                }

                player.InfoKeys.Add(key, infoKeyTokens[i + 1]);
            }
        }

        private void ReadMessageHltv()
        {
            perspective = Perspectives.Hltv;

            /*
            #define HLTV_ACTIVE				0	// tells client that he's an spectator and will get director commands
            #define HLTV_STATUS				1	// send status infos about proxy 
            #define HLTV_LISTEN				2	// tell client to listen to a multicast stream
             */

            Byte subCommand = parser.BitBuffer.ReadByte();

            if (subCommand == 2) // HLTV_LISTEN
            {
                parser.Seek(8);
            }
            else if (subCommand == 1)
            {
                // TODO: fix this
            }
        }
        #endregion

        #region Pre-Write Message Handlers
        private void PreWriteMessageServerInfo()
        {
            firstFrameToWriteIndex = currentFrameIndex;

            // skip the rest

            parser.Seek(4); // network protocol
            parser.Seek(4); // process count
            parser.Seek(4); // munged map checksum
            parser.Seek(16); // client.dll checksum
            parser.Seek(1); // max clients
            parser.Seek(1); // recorder slot
            parser.Seek(1); // unknown byte
            parser.BitBuffer.ReadString(); // game folder

            // server name
            if (NetworkProtocol > 43)
            {
                parser.BitBuffer.ReadString();
            }

            // skip map
            parser.BitBuffer.ReadString();

            if (NetworkProtocol == 45)
            {
                Byte extraInfo = parser.BitBuffer.ReadByte();
                parser.Seek(-1);

                if (extraInfo != (Byte)HalfLifeDemoParser.MessageId.svc_sendextrainfo)
                {
                    parser.BitBuffer.ReadString(); // skip mapcycle

                    if (parser.BitBuffer.ReadByte() > 0)
                    {
                        parser.Seek(36);
                    }
                }
            }
            else
            {
                parser.BitBuffer.ReadString(); // skip mapcycle

                if (NetworkProtocol > 43)
                {
                    if (parser.BitBuffer.ReadByte() > 0)
                    {
                        parser.Seek(21);
                    }
                }
            }
        }
        #endregion
    }
}
