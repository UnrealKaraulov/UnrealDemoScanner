using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Diagnostics; // Assert

namespace compLexity_Demo_Player
{
    public class HalfLifeDemoParser : DemoParser<HalfLifeDemo>
    {
        public enum MessageId : byte
        {
            svc_nop = 1,
            svc_disconnect = 2,
            svc_event = 3,
            svc_version = 4,
            svc_setview = 5,
            svc_sound = 6,
            svc_time = 7,
            svc_print = 8,
            svc_stufftext = 9,
            svc_setangle = 10,
            svc_serverinfo = 11,
            svc_lightstyle = 12,
            svc_updateuserinfo = 13,
            svc_deltadescription = 14,
            svc_clientdata = 15,
            svc_stopsound = 16,
            svc_pings = 17,
            svc_particle = 18,
            //svc_damage = 19,
            svc_spawnstatic = 20,
            svc_event_reliable = 21,
            svc_spawnbaseline = 22,
            svc_tempentity = 23,
            svc_setpause = 24,
            svc_signonnum = 25,
            svc_centerprint = 26,
            //svc_killedmonster = 27,
            //svc_foundsecret = 28,
            svc_spawnstaticsound = 29,
            svc_intermission = 30,
            svc_finale = 31,
            svc_cdtrack = 32,
            //svc_restore = 33, // TEST ME!!! something to do with loading/saving
            //svc_cutscene = 34,
            svc_weaponanim = 35,
            //svc_decalname = 36,
            svc_roomtype = 37,
            svc_addangle = 38,
            svc_newusermsg = 39,
            svc_packetentities = 40,
            svc_deltapacketentities = 41,
            svc_choke = 42,
            svc_resourcelist = 43,
            svc_newmovevars = 44,
            svc_resourcerequest = 45,
            svc_customization = 46,
            svc_crosshairangle = 47,
            svc_soundfade = 48,
            svc_filetxferfailed = 49,
            svc_hltv = 50,
            svc_director = 51,
            svc_voiceinit = 52,
            svc_voicedata = 53,
            svc_sendextrainfo = 54,
            svc_timescale = 55,
            svc_resourcelocation = 56,
            svc_sendcvarvalue = 57,
            svc_sendcvarvalue2 = 58
        }

        private class UserMessage
        {
            public Byte Id;
            public SByte Length;
        }

        public class FrameHeader
        {
            public Byte Type;
            public Single Timestamp;
            public UInt32 Number;
        }

        public class GameDataFrameHeader
        {
            public UInt32 ResolutionWidth;
            public UInt32 ResolutionHeight;
            public UInt32 Length;
        }

        private BitBuffer bitBuffer = null;
        private Boolean readingGameData = false;
        private Hashtable deltaDecoderTable;

        private Hashtable userMessageTable; // name -> id, length
        private Hashtable userMessageCallbackTable; // name -> Common.NoArgsDelegate callback

        Boolean inLoadingSegment = true;

        #region Properties
        public Boolean InLoadingSegment
        {
            get
            {
                return inLoadingSegment;
            }
        }

        public BitBuffer BitBuffer
        {
            get
            {
                return bitBuffer;
            }
        }

        public Int32 GameDataDemoInfoLength
        {
            get
            {
                if (demo.NetworkProtocol <= 43)
                {
                    return 532;
                }
                else
                {
                    return 436;
                }
            }
        }

        // doesn't need to be a property, but for consistencies sake...
        public Int32 GameDataSequenceInfoLength
        {
            get
            {
                return 28;
            }
        }
        #endregion

        public HalfLifeDemoParser(HalfLifeDemo demo)
        {
            this.demo = demo;

            // message handlers
            AddMessageHandler((Byte)MessageId.svc_nop, 0);
            AddMessageHandler((Byte)MessageId.svc_disconnect, MessageDisconnect);
            AddMessageHandler((Byte)MessageId.svc_event, MessageEvent);
            AddMessageHandler((Byte)MessageId.svc_version, MessageVersion);
            AddMessageHandler((Byte)MessageId.svc_setview, 2);
            AddMessageHandler((Byte)MessageId.svc_sound, MessageSound);
            AddMessageHandler((Byte)MessageId.svc_time, 4);
            AddMessageHandler((Byte)MessageId.svc_print, MessagePrint);
            AddMessageHandler((Byte)MessageId.svc_stufftext, MessageStuffText);
            AddMessageHandler((Byte)MessageId.svc_setangle, 6);
            AddMessageHandler((Byte)MessageId.svc_serverinfo, MessageServerInfo);
            AddMessageHandler((Byte)MessageId.svc_lightstyle, MessageLightStyle);
            AddMessageHandler((Byte)MessageId.svc_updateuserinfo, MessageUpdateUserInfo);
            AddMessageHandler((Byte)MessageId.svc_deltadescription, MessageDeltaDescription);
            AddMessageHandler((Byte)MessageId.svc_clientdata, MessageClientData);
            AddMessageHandler((Byte)MessageId.svc_stopsound, 2);
            AddMessageHandler((Byte)MessageId.svc_pings, MessagePings);
            AddMessageHandler((Byte)MessageId.svc_particle, 11);
            AddMessageHandler((Byte)MessageId.svc_spawnstatic, MessageSpawnStatic);
            AddMessageHandler((Byte)MessageId.svc_event_reliable, MessageEventReliable);
            AddMessageHandler((Byte)MessageId.svc_spawnbaseline, MessageSpawnBaseline);
            AddMessageHandler((Byte)MessageId.svc_tempentity, MessageTempEntity);
            AddMessageHandler((Byte)MessageId.svc_setpause, MessageSetPause);
            AddMessageHandler((Byte)MessageId.svc_signonnum, 1);
            AddMessageHandler((Byte)MessageId.svc_centerprint, MessageCenterPrint);
            AddMessageHandler((Byte)MessageId.svc_spawnstaticsound, 14);
            AddMessageHandler((Byte)MessageId.svc_intermission, 0);
            AddMessageHandler((Byte)MessageId.svc_finale, 1);
            AddMessageHandler((Byte)MessageId.svc_cdtrack, 2);
            AddMessageHandler((Byte)MessageId.svc_weaponanim, 2);
            AddMessageHandler((Byte)MessageId.svc_roomtype, 2);
            AddMessageHandler((Byte)MessageId.svc_addangle, 2);
            AddMessageHandler((Byte)MessageId.svc_newusermsg, MessageNewUserMsg);
            AddMessageHandler((Byte)MessageId.svc_packetentities, MessagePacketEntities);
            AddMessageHandler((Byte)MessageId.svc_deltapacketentities, MessageDeltaPacketEntities);
            AddMessageHandler((Byte)MessageId.svc_choke, 0);
            AddMessageHandler((Byte)MessageId.svc_resourcelist, MessageResourceList);
            AddMessageHandler((Byte)MessageId.svc_newmovevars, MessageNewMoveVars);
            AddMessageHandler((Byte)MessageId.svc_resourcerequest, 8);
            AddMessageHandler((Byte)MessageId.svc_customization, MessageCustomization);
            AddMessageHandler((Byte)MessageId.svc_crosshairangle, 2);
            AddMessageHandler((Byte)MessageId.svc_soundfade, 4);
            AddMessageHandler((Byte)MessageId.svc_filetxferfailed, MessageFileTransferFailed);
            AddMessageHandler((Byte)MessageId.svc_hltv, MessageHltv);
            AddMessageHandler((Byte)MessageId.svc_director, MessageDirector);
            AddMessageHandler((Byte)MessageId.svc_voiceinit, MessageVoiceInit);
            AddMessageHandler((Byte)MessageId.svc_voicedata, MessageVoiceData);
            AddMessageHandler((Byte)MessageId.svc_sendextrainfo, MessageSendExtraInfo);
            AddMessageHandler((Byte)MessageId.svc_timescale, 4);
            AddMessageHandler((Byte)MessageId.svc_resourcelocation, MessageResourceLocation);
            AddMessageHandler((Byte)MessageId.svc_sendcvarvalue, MessageSendCvarValue);
            AddMessageHandler((Byte)MessageId.svc_sendcvarvalue2, MessageSendCvarValue2);

            // user messages
            userMessageTable = new Hashtable();
            userMessageCallbackTable = new Hashtable();

            // delta descriptions
            deltaDecoderTable = new Hashtable();

            HalfLifeDeltaStructure deltaDescription = new HalfLifeDeltaStructure("delta_description_t");
            AddDeltaStructure(deltaDescription);

            deltaDescription.AddEntry("flags", 32, 1.0f, HalfLifeDeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("name", 8, 1.0f, HalfLifeDeltaStructure.EntryFlags.String);
            deltaDescription.AddEntry("offset", 16, 1.0f, HalfLifeDeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("size", 8, 1.0f, HalfLifeDeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("nBits", 8, 1.0f, HalfLifeDeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("divisor", 32, 4000.0f, HalfLifeDeltaStructure.EntryFlags.Float);
            deltaDescription.AddEntry("preMultiplier", 32, 4000.0f, HalfLifeDeltaStructure.EntryFlags.Float);
        }

        // public so svc_deltadescription can be parsed elsewhere
        public void AddDeltaStructure(HalfLifeDeltaStructure structure)
        {
            // remove decoder if it already exists (duplicate svc_deltadescription message)
            // e.g. GotFrag Demo 6 (rs vs TSO).zip

            if (deltaDecoderTable[structure.Name] != null)
            {
                deltaDecoderTable.Remove(structure.Name);
            }

            deltaDecoderTable.Add(structure.Name, structure);
        }

        public HalfLifeDeltaStructure GetDeltaStructure(String name)
        {
            HalfLifeDeltaStructure structure = (HalfLifeDeltaStructure)deltaDecoderTable[name];

            if (structure == null)
            {
                throw new ApplicationException("Delta structure \"" + name + "\" not found.");
            }

            return structure;
        }

        // public so that svc_newusermsg can be parsed elsewhere
        public void AddUserMessage(Byte id, SByte length, String name)
        {
            // some demos contain duplicate user message definitions
            // e.g. GotFrag Demo 13 (mTw.nine vs CosaNostra).zip
            // others, due to what seems like completely fucking retarded servers, have many copies of the 
            // same user message registered with different id's
            if (userMessageTable.Contains(name))
            {
                userMessageTable.Remove(name);
            }

            UserMessage userMessage = new UserMessage();
            userMessage.Id = id;
            userMessage.Length = length;

            userMessageTable.Add(name, userMessage);
            AddMessageIdString(id, name);

            // see if there's a handler waiting to be attached to this message
            Procedure callback = (Procedure)userMessageCallbackTable[name];

            if (callback == null)
            {
                AddMessageHandler(id, length);
            }
            else
            {
                AddMessageHandler(id, callback);
            }
        }

        public void AddUserMessageHandler(String name, Procedure callback)
        {
            // override existing callback
            if (userMessageCallbackTable.Contains(name))
            {
                userMessageCallbackTable.Remove(name);
            }

            userMessageCallbackTable.Add(name, callback);

            // TODO: assert that all svc_newusermsg messages have been parsed?

            // find user message id
            /*UserMessage userMessage = (UserMessage)userMessageTable[name];

            // should be ok if message doesn't exist, since different mods may not have certain messages
            if (userMessage != null)
            {
                // add message handler
                AddMessageHandler(userMessage.Id, callback);
            }*/
        }

        public Int32 FindUserMessageLength(String name)
        {
            // shouldn't return null, since this method should be called from a message handler, and that message handler couldn't be called if there wasn't a UserMessage entry
            UserMessage userMessage = (UserMessage)userMessageTable[name];

            if (userMessage.Length == -1)
            {
                // if svc_newusermsg length is -1, first byte is length
                return bitBuffer.ReadByte();                
            }

            return userMessage.Length;
        }

        public FrameHeader ReadFrameHeader()
        {
            FrameHeader header = new FrameHeader();

            header.Type = fileReader.ReadByte();
            header.Timestamp = fileReader.ReadSingle();
            header.Number = fileReader.ReadUInt32();

            inLoadingSegment = (header.Type == 0);

            return header;
        }

        public Int32 GetFrameLength(Byte frameType)
        {
            Int32 length = 0;

            switch (frameType)
            {
                case 2: // ???
                    break;

                case 3: // client command
                    length = 64;
                    break;

                case 4:
                    length = 32;
                    break;

                case 5: // end of segment
                    break;

                case 6:
                    length = 84;
                    break;

                case 7:
                    length = 8;
                    break;

                case 8:
                    Seek(4);
                    length = fileReader.ReadInt32();
                    Seek(-8);
                    length += 24;
                    break;

                case 9:
                    length = 4 + fileReader.ReadInt32();
                    Seek(-4);
                    break;

                default:
                    throw new ApplicationException("Unknown frame type.");
            }

            return length;
        }

        public void SkipFrame(Byte frameType)
        {
            Seek(GetFrameLength(frameType));
        }

        public GameDataFrameHeader ReadGameDataFrameHeader()
        {
            GameDataFrameHeader header = new GameDataFrameHeader();

            if (demo.NetworkProtocol <= 43)
            {
                // TODO: read resolution
                Seek(560);

                // 264 bytes
                // movevars
            }
            else
            {
                // 464 bytes
                Seek(220);
                header.ResolutionWidth = fileReader.ReadUInt32();
                header.ResolutionHeight = fileReader.ReadUInt32();
                // supposedly bpp here (int)
                // TODO: check if true
                Seek(236);

                // 436 bytes "demo info"
                // -first 64 bytes: view angles (16 floats)
                //      -0 (4 bytes)
                //      -x, y, z (12 bytes)
                //      -pitch, yaw... presumably roll
                // -156 bytes unknown
                // -8 bytes: resolution
                // -60 bytes unknown
                // -98 bytes + string (0?): movevars
                // -81 bytes unknown (assuming string is null)
                //      last 15 bytes control view model animation

                // 28 bytes (7 ints) "sequence info"
            }

            header.Length = fileReader.ReadUInt32();

            return header;
        }

        public void ParseGameDataMessages(Byte[] frameData)
        {
            ParseGameDataMessages(frameData, null);
        }

        public void ParseGameDataMessages(Byte[] frameData, Function<Byte, Byte> userMessageCallback)
        {
            Int64 gameDataStartOffset = fileStream.Position - frameData.Length;

            // read game data frame into memory
            bitBuffer = new BitBuffer(frameData);
            readingGameData = true;

            try
            {
                BeginMessageLog(gameDataStartOffset, frameData);

                // start parsing messages
                while (true)
                {
                    Int32 messageFrameOffset = bitBuffer.CurrentByte;
                    Byte messageId = bitBuffer.ReadByte();
                    String messageName = Enum.GetName(typeof(MessageId), messageId);

                    if (messageName == null) // a user message, presumably
                    {
                        messageName = FindMessageIdString(messageId);
                    }

                    LogMessage(messageId, messageName, messageFrameOffset);

                    MessageHandler messageHandler = FindMessageHandler(messageId);

                    // Handle the conversion of user message id's.
                    // Used by demo writing to convert to the current network protocol.
                    if (messageId > 64 && userMessageCallback != null)
                    {
                        Byte newMessageId = userMessageCallback(messageId);

                        if (newMessageId != messageId)
                        {
                            // write the new id to the bitbuffer
                            bitBuffer.SeekBytes(-1);
                            bitBuffer.RemoveBytes(1);
                            bitBuffer.InsertBytes(new Byte[] { newMessageId });
                        }
                    }

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
                        // user messages
                        if (messageId >= 64)
                        {
                            // All non-engine user messages start with a byte that is the number of bytes in the message remaining.
                            Byte length = bitBuffer.ReadByte();
                            Seek(length);
                        }
                        else
                        {
                            throw new ApplicationException(String.Format("Unknown message id \"{0}\"", messageId));
                        }
                    }

                    // Check if we've reached the end of the frame, or if any of the messages have called SkipGameDataFrame (readingGameData will be false).
                    if (bitBuffer.CurrentByte == bitBuffer.Length || !readingGameData)
                    {
                        break;
                    }
                }
            }
            finally
            {
                readingGameData = false;
            }
        }

        public void SkipGameDataFrame()
        {
            readingGameData = false;
        }

        public void Seek(Int64 deltaOffset)
        {
            Seek(deltaOffset, SeekOrigin.Current);
        }

        public void Seek(Int64 offset, SeekOrigin origin)
        {
            if (readingGameData)
            {
                Debug.Assert(offset <= Int32.MaxValue);
                bitBuffer.SeekBytes((Int32)offset, origin);
            }
            else
            {
                fileStream.Seek(offset, origin);
            }
        }

        public Single ReadCoord() // TODO: move to bitbuffer?
        {
            return (Single)bitBuffer.ReadInt16() * (1.0f / 8.0f);
        }

        #region Engine Messages
        private void MessageDisconnect()
        {
            bitBuffer.ReadString(); // Disconnect reason?
        }

        public void MessageEvent()
        {
            if (demo.NetworkProtocol <= 43)
            {
                bitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            UInt32 nEvents = bitBuffer.ReadUnsignedBits(5);

            for (Int32 i = 0; i < nEvents; i++)
            {
                bitBuffer.SeekBits(10); // event index

                Boolean packetIndexBit = bitBuffer.ReadBoolean();

                if (packetIndexBit)
                {
                    bitBuffer.SeekBits(11); // packet index

                    Boolean deltaBit = bitBuffer.ReadBoolean();

                    if (deltaBit)
                    {
                        GetDeltaStructure("event_t").ReadDelta(bitBuffer, null);
                    }
                }

                Boolean fireTimeBit = bitBuffer.ReadBoolean();

                if (fireTimeBit)
                {
                    bitBuffer.SeekBits(16); // fire time
                }
            }

            bitBuffer.SkipRemainingBits();
            bitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessageVersion()
        {
            Seek(4); // uint: server network protocol number.
        }

        public void MessageSound()
        {
            if (demo.NetworkProtocol <= 43)
            {
                bitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            UInt32 flags = bitBuffer.ReadUnsignedBits(9);

            if ((flags & (1 << 0)) != 0) // volume
            {
                bitBuffer.SeekBits(8);
            }

            if ((flags & (1 << 1)) != 0) // attenuation * 64
            {
                bitBuffer.SeekBits(8);
            }

            bitBuffer.SeekBits(3); // channel
            bitBuffer.SeekBits(11); // edict number

            if ((flags & (1 << 2)) != 0) // sound index (short)
            {
                bitBuffer.SeekBits(16);
            }
            else // sound index (byte)
            {
                bitBuffer.SeekBits(8);
            }

            bitBuffer.ReadVectorCoord(true); // position

            if ((flags & (1 << 3)) != 0) // pitch
            {
                bitBuffer.SeekBits(8);
            }

            bitBuffer.SkipRemainingBits();
            bitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessagePrint()
        {
            // null-terminated string
            bitBuffer.ReadString();
        }

        private void MessageStuffText()
        {
            // null terminated string
            bitBuffer.ReadString();
        }

        private void MessageServerInfo()
        {
            Seek(28);

            demo.MaxClients = bitBuffer.ReadByte();

            Seek(2); // ???, byte multiplayer

            bitBuffer.ReadString(); // game dir

            if (demo.NetworkProtocol > 43)
            {
                bitBuffer.ReadString(); // server name                
            }

            // skip map
            bitBuffer.ReadString();

            if (demo.NetworkProtocol == 45)
            {
                Byte extraInfo = bitBuffer.ReadByte();
                Seek(-1);

                if (extraInfo != (Byte)HalfLifeDemoParser.MessageId.svc_sendextrainfo)
                {
                    bitBuffer.ReadString(); // skip mapcycle

                    if (bitBuffer.ReadByte() > 0)
                    {
                        Seek(36);
                    }
                }
            }
            else
            {
                bitBuffer.ReadString(); // skip mapcycle

                if (demo.NetworkProtocol > 43)
                {
                    if (bitBuffer.ReadByte() > 0)
                    {
                        Seek(21);
                    }
                }
            }
        }

        private void MessageLightStyle()
        {
            Seek(1);
            bitBuffer.ReadString();
        }

        public void MessageUpdateUserInfo()
        {
            Seek(5);
            bitBuffer.ReadString();

            if (demo.NetworkProtocol > 43)
            {
                Seek(16); // string hash
            }
        }

        public void MessageDeltaDescription()
        {
            String structureName = bitBuffer.ReadString();

            if (demo.NetworkProtocol == 43)
            {
                bitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            UInt32 nEntries = bitBuffer.ReadUnsignedBits(16);

            HalfLifeDeltaStructure newDeltaStructure = new HalfLifeDeltaStructure(structureName);
            AddDeltaStructure(newDeltaStructure);

            HalfLifeDeltaStructure deltaDescription = GetDeltaStructure("delta_description_t");

            for (UInt16 i = 0; i < nEntries; i++)
            {
                HalfLifeDelta newDelta = deltaDescription.CreateDelta();
                deltaDescription.ReadDelta(bitBuffer, newDelta);

                newDeltaStructure.AddEntry(newDelta);
            }

            bitBuffer.SkipRemainingBits();
            bitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        public void MessageClientData()
        {
            if (demo.Perspective == Demo.Perspectives.Hltv)
            {
                return;
            }

            if (demo.NetworkProtocol <= 43)
            {
                bitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            Boolean deltaSequence = bitBuffer.ReadBoolean();

            if (deltaSequence)
            {
                bitBuffer.SeekBits(8); // delta sequence number
            }

            GetDeltaStructure("clientdata_t").ReadDelta(bitBuffer, null);
            
            while (bitBuffer.ReadBoolean())
            {
                if (demo.NetworkProtocol < 47 && !demo.IsBetaSteam())
                {
                    bitBuffer.SeekBits(5); // weapon index
                }
                else
                {
                    bitBuffer.SeekBits(6); // weapon index
                }

                GetDeltaStructure("weapon_data_t").ReadDelta(bitBuffer, null);
            }

            bitBuffer.SkipRemainingBits();
            bitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        public void MessagePings()
        {
            if (demo.NetworkProtocol <= 43)
            {
                bitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            while (bitBuffer.ReadBoolean())
            {
                bitBuffer.SeekBits(24); // int32 each: slot, ping, loss
            }

            bitBuffer.SkipRemainingBits();
            bitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessageSpawnStatic()
        {
            bitBuffer.SeekBytes(18);
            Byte renderMode = bitBuffer.ReadByte();

            if (renderMode != 0)
            {
                bitBuffer.SeekBytes(5);
            }
        }

        private void MessageEventReliable()
        {
            if (demo.NetworkProtocol <= 43)
            {
                bitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            bitBuffer.SeekBits(10); // event index

            GetDeltaStructure("event_t").ReadDelta(bitBuffer, null);

            Boolean delayBit = bitBuffer.ReadBoolean();

            if (delayBit)
            {
                bitBuffer.SeekBits(16); // delay / 100.0f
            }

            bitBuffer.SkipRemainingBits();
            bitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        public void MessageSpawnBaseline()
        {
            if (demo.NetworkProtocol <= 43)
            {
                bitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            while (true)
            {
                UInt32 entityIndex = bitBuffer.ReadUnsignedBits(11);

                if (entityIndex == (1<<11)-1) // all 1's
                {
                    break;
                }

                UInt32 entityType = bitBuffer.ReadUnsignedBits(2);
                String entityTypeString;

                if ((entityType & 1) != 0) // is bit 1 set?
                {
                    if (entityIndex > 0 && entityIndex <= demo.MaxClients)
                    {
                        entityTypeString = "entity_state_player_t";
                    }
                    else
                    {
                        entityTypeString = "entity_state_t";
                    }
                }
                else
                {
                    entityTypeString = "custom_entity_state_t";
                }

                GetDeltaStructure(entityTypeString).ReadDelta(bitBuffer, null);
            }

            UInt32 footer = bitBuffer.ReadUnsignedBits(5); // should be all 1's

            if (footer != (1 << 5) - 1)
            {
                throw new ApplicationException("Bad svc_spawnbaseline footer.");
            }

            UInt32 nExtraData = bitBuffer.ReadUnsignedBits(6);

            for (Int32 i = 0; i < nExtraData; i++)
            {
                GetDeltaStructure("entity_state_t").ReadDelta(bitBuffer, null);
            }

            bitBuffer.Endian = BitBuffer.EndianType.Little;
            bitBuffer.SkipRemainingBits();
        }

        private void MessageTempEntity()
        {
            Byte type = bitBuffer.ReadByte();

            switch (type)
            {
                // obsolete
                case 16: // TE_BEAM
                case 26: // TE_BEAMHOSE
                    break;

                // simple coord format messages
                case 2: // TE_GUNSHOT
                case 4: // TE_TAREXPLOSION 
                case 9: // TE_SPARKS
                case 10: // TE_LAVASPLASH
                case 11: // TE_TELEPORT
                    Seek(6);
                    break;

                case 0: // TE_BEAMPOINTS
                    Seek(24);
                    break;

                case 1: // TE_BEAMENTPOINT
                    Seek(20);
                    break;

                case 3: // TE_EXPLOSION
                    Seek(11);
                    break;

                case 5: // TE_SMOKE
                    Seek(10);
                    break;

                case 6: // TE_TRACER
                    Seek(12);
                    break;

                case 7: // TE_LIGHTNING 
                    Seek(17);
                    break;

                case 8: // TE_BEAMENTS
                    Seek(16);
                    break;

                case 12: // TE_EXPLOSION2
                    Seek(8);
                    break;

                case 13: // TE_BSPDECAL
                    Seek(8);

                    UInt16 entityIndex = bitBuffer.ReadUInt16();

                    if (entityIndex != 0)
                    {
                        Seek(2);
                    }
                    break;

                case 14: // TE_IMPLOSION
                    Seek(9);
                    break;

                case 15: // TE_SPRITETRAIL
                    Seek(19);
                    break;

                case 17: // TE_SPRITE
                    Seek(10);
                    break;

                case 18: // TE_BEAMSPRITE
                    Seek(16);
                    break;

                case 19: // TE_BEAMTORUS
                case 20: // TE_BEAMDISK
                case 21: // TE_BEAMCYLINDER
                    Seek(24);
                    break;

                case 22: // TE_BEAMFOLLOW
                    Seek(10);
                    break;

                case 23: // TE_GLOWSPRITE
                    // SDK is wrong
                    /* 
                        write_coord()	 position
                        write_coord()
                        write_coord()
                        write_short()	 model index
                        write_byte()	 life in 0.1's
                        write_byte()	scale in 0.1's
                        write_byte()	brightness
                    */
                    Seek(11);
                    break;

                case 24: // TE_BEAMRING
                    Seek(16);
                    break;

                case 25: // TE_STREAK_SPLASH
                    Seek(19);
                    break;

                case 27: // TE_DLIGHT
                    Seek(12);
                    break;

                case 28: // TE_ELIGHT
                    Seek(16);
                    break;

                case 29: // TE_TEXTMESSAGE
                    Seek(5);
                    Byte textParmsEffect = bitBuffer.ReadByte();
                    Seek(14);

                    if (textParmsEffect == 2)
                    {
                        Seek(2);
                    }

                    bitBuffer.ReadString(); // capped to 512 bytes (including null terminator)
                    break;

                case 30: // TE_LINE
                case 31: // TE_BOX
                    Seek(17);
                    break;

                case 99: // TE_KILLBEAM
                    Seek(2);
                    break;

                case 100: // TE_LARGEFUNNEL
                    Seek(10);
                    break;

                case 101: // TE_BLOODSTREAM
                    Seek(14);
                    break;

                case 102: // TE_SHOWLINE
                    Seek(12);
                    break;

                case 103: // TE_BLOOD
                    Seek(14);
                    break;

                case 104: // TE_DECAL
                    Seek(9);
                    break;

                case 105: // TE_FIZZ
                    Seek(5);
                    break;

                case 106: // TE_MODEL
                    // WRITE_ANGLE could be a short..
                    Seek(17);
                    break;

                case 107: // TE_EXPLODEMODEL
                    Seek(13);
                    break;

                case 108: // TE_BREAKMODEL
                    Seek(24);
                    break;

                case 109: // TE_GUNSHOTDECAL
                    Seek(9);
                    break;

                case 110: // TE_SPRITE_SPRAY
                    Seek(17);
                    break;

                case 111: // TE_ARMOR_RICOCHET
                    Seek(7);
                    break;

                case 112: // TE_PLAYERDECAL (could be a trailing short after this, apparently...)
                    Seek(10);
                    break;

                case 113: // TE_BUBBLES
                case 114: // TE_BUBBLETRAIL
                    Seek(19);
                    break;

                case 115: // TE_BLOODSPRITE
                    Seek(12);
                    break;

                case 116: // TE_WORLDDECAL
                case 117: // TE_WORLDDECALHIGH
                    Seek(7);
                    break;

                case 118: // TE_DECALHIGH
                    Seek(9);
                    break;

                case 119: // TE_PROJECTILE
                    Seek(16);
                    break;

                case 120: // TE_SPRAY
                    Seek(18);
                    break;

                case 121: // TE_PLAYERSPRITES
                    Seek(5);
                    break;

                case 122: // TE_PARTICLEBURST
                    Seek(10);
                    break;

                case 123: // TE_FIREFIELD
                    Seek(9);
                    break;

                case 124: // TE_PLAYERATTACHMENT
                    Seek(7);
                    break;

                case 125: // TE_KILLPLAYERATTACHMENTS
                    Seek(1);
                    break;

                case 126: // TE_MULTIGUNSHOT
                    Seek(18);
                    break;

                case 127: // TE_USERTRACER
                    Seek(15);
                    break;

                default:
                    throw new ApplicationException(String.Format("Unknown tempentity type \"{0}\".", type));
            }
        }

        private void MessageSetPause()
        {
            Seek(1);
        }

        private void MessageCenterPrint()
        {
            bitBuffer.ReadString();
        }

        public void MessageNewUserMsg()
        {
            Byte id = bitBuffer.ReadByte();
            SByte length = bitBuffer.ReadSByte();
            String name = bitBuffer.ReadString(16);

            AddUserMessage(id, length, name);
        }

        public void MessagePacketEntities()
        {
            bitBuffer.SeekBits(16); // num entities (not reliable at all, loop until footer - see below)

            if (demo.NetworkProtocol <= 43)
            {
                bitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            UInt32 entityNumber = 0;

            // begin entity parsing
            while (true)
            {
                UInt16 footer = bitBuffer.ReadUInt16();

                if (footer == 0)
                {
                    break;
                }

                bitBuffer.SeekBits(-16);

                Boolean entityNumberIncrement = bitBuffer.ReadBoolean();

                if (!entityNumberIncrement) // entity number isn't last entity number + 1, need to read it in
                {
                    // is the following entity number absolute, or relative from the last one?
                    Boolean absoluteEntityNumber = bitBuffer.ReadBoolean();

                    if (absoluteEntityNumber)
                    {
                        entityNumber = bitBuffer.ReadUnsignedBits(11);
                    }
                    else
                    {
                        entityNumber += bitBuffer.ReadUnsignedBits(6);
                    }
                }
                else
                {
                    entityNumber++;
                }

                if (demo.GameFolderName == "tfc")
                {
                    bitBuffer.ReadBoolean(); // unknown
                }

                Boolean custom = bitBuffer.ReadBoolean();
                Boolean useBaseline = bitBuffer.ReadBoolean();

                if (useBaseline)
                {
                    bitBuffer.SeekBits(6); // baseline index
                }

                String entityType = "entity_state_t";

                if (entityNumber > 0 && entityNumber <= demo.MaxClients)
                {
                    entityType = "entity_state_player_t";
                }
                else if (custom)
                {
                    entityType = "custom_entity_state_t";
                }

                GetDeltaStructure(entityType).ReadDelta(bitBuffer, null);
            }

            bitBuffer.SkipRemainingBits();
            bitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        public void MessageDeltaPacketEntities()
        {
            bitBuffer.SeekBits(16); // num entities (not reliable at all, loop until footer - see below)
            bitBuffer.SeekBits(8); // delta sequence number

            if (demo.NetworkProtocol <= 43)
            {
                bitBuffer.Endian = BitBuffer.EndianType.Big;

            }

            UInt32 entityNumber = 0;

            while (true)
            {
                UInt16 footer = bitBuffer.ReadUInt16();

                if (footer == 0)
                {
                    break;
                }

                bitBuffer.SeekBits(-16);

                Boolean removeEntity = bitBuffer.ReadBoolean();

                // is the following entity number absolute, or relative from the last one?
                Boolean absoluteEntityNumber = bitBuffer.ReadBoolean();

                if (absoluteEntityNumber)
                {
                    entityNumber = bitBuffer.ReadUnsignedBits(11);
                }
                else
                {
                    entityNumber += bitBuffer.ReadUnsignedBits(6);
                }

                if (!removeEntity)
                {
                    if (demo.GameFolderName == "tfc")
                    {
                        bitBuffer.ReadBoolean(); // unknown
                    }

                    Boolean custom = bitBuffer.ReadBoolean();

                    if (demo.NetworkProtocol <= 43)
                    {
                        bitBuffer.SeekBits(1); // unknown
                    }

                    String entityType = "entity_state_t";

                    if (entityNumber > 0 && entityNumber <= demo.MaxClients)
                    {
                        entityType = "entity_state_player_t";
                    }
                    else if (custom)
                    {
                        entityType = "custom_entity_state_t";
                    }

                    GetDeltaStructure(entityType).ReadDelta(bitBuffer, null);
                }
            }

            bitBuffer.SkipRemainingBits();
            bitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessageResourceList()
        {
            if (demo.NetworkProtocol <= 43)
            {
                bitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            UInt32 nEntries = bitBuffer.ReadUnsignedBits(12);

            for (Int32 i = 0; i < nEntries; i++)
            {
                bitBuffer.SeekBits(4); // entry type
                bitBuffer.ReadString(); // entry name
                bitBuffer.SeekBits(36); // index (12 bits), file size (24 bits) signed?

                UInt32 flags = bitBuffer.ReadUnsignedBits(3);


                if ((flags & 4) != 0) // md5 hash
                {
                    bitBuffer.SeekBytes(16);
                }

                if (bitBuffer.ReadBoolean())
                {
                    bitBuffer.SeekBytes(32); // reserved data
                }
            }

            // consistency list
            // indices of resources to force consistency upon?
            if (bitBuffer.ReadBoolean())
            {
                while (bitBuffer.ReadBoolean())
                {
                    Int32 nBits = (bitBuffer.ReadBoolean() ? 5 : 10);
                    bitBuffer.SeekBits(nBits); // consistency index
                }
            }

            bitBuffer.SkipRemainingBits();
            bitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessageNewMoveVars()
        {
            // TODO: see OHLDS, SV_SetMoveVars
            /*
            MSG_WriteFloat(buf, movevars.gravity);
           MSG_WriteFloat(buf, movevars.stopspeed);
           MSG_WriteFloat(buf, movevars.maxspeed);
           MSG_WriteFloat(buf, movevars.spectatormaxspeed);
           MSG_WriteFloat(buf, movevars.accelerate);
           MSG_WriteFloat(buf, movevars.airaccelerate);
           MSG_WriteFloat(buf, movevars.wateraccelerate);
           MSG_WriteFloat(buf, movevars.friction);
           MSG_WriteFloat(buf, movevars.edgefriction);
           MSG_WriteFloat(buf, movevars.waterfriction);
           MSG_WriteFloat(buf, movevars.entgravity);
           MSG_WriteFloat(buf, movevars.bounce);
           MSG_WriteFloat(buf, movevars.stepsize);
           MSG_WriteFloat(buf, movevars.maxvelocity);
           MSG_WriteFloat(buf, movevars.zmax);
           MSG_WriteFloat(buf, movevars.waveHeight);
           MSG_WriteByte(buf, (movevars.footsteps != 0)); //Sets it to 1 if nonzero, just in case someone's abusing the whole 'bool' thing.
           MSG_WriteFloat(buf, movevars.rollangle);
           MSG_WriteFloat(buf, movevars.rollspeed);
           MSG_WriteFloat(buf, movevars.skycolor_r);
           MSG_WriteFloat(buf, movevars.skycolor_g);
           MSG_WriteFloat(buf, movevars.skycolor_b);
           MSG_WriteFloat(buf, movevars.skyvec_x);
           MSG_WriteFloat(buf, movevars.skyvec_y);
           MSG_WriteFloat(buf, movevars.skyvec_z);
           MSG_WriteString(buf, movevars.skyName);
            */

            // same as gamedata header
            // 800, 75, 900, 500, 5, 10, 10, 4, 2, 1, 1, 1, 18, 2000, 6400, 0, 1, 0
            // + 24 bytes of unknown

            // different size in network protocols < 45 like gamedata frame header???

            Seek(98);
            bitBuffer.ReadString();
        }

        private void MessageCustomization()
        {
            // ???
            Seek(2);
            bitBuffer.ReadString();
            Seek(23);
        }

        private void MessageFileTransferFailed()
        {
            // string: filename
            bitBuffer.ReadString();
        }

        private void MessageHltv()
        {
            // TODO: CHECK THE SDK - MESSAGE_BEGIN( MSG_SPEC, SVC_HLTV );

            /*
            
            new:
            #define HLTV_ACTIVE				0	// tells client that he's an spectator and will get director command
            #define HLTV_STATUS				1	// send status infos about proxy 
            #define HLTV_CAMERA				2	// set the actual director camera position
            #define HLTV_EVENT				3	// informs the dircetor about ann important game event

            old:
            #define HLTV_ACTIVE				0	// tells client that he's an spectator and will get director commands
            #define HLTV_STATUS				1	// send status infos about proxy 
            #define HLTV_LISTEN				2	// tell client to listen to a multicast stream
             */

            Byte subCommand = bitBuffer.ReadByte();

            if (subCommand == 2) // HLTV_LISTEN/HLTV_CAMERA
            {
                Seek(8);
            }
            else if (subCommand != 0)
            {
                // TODO: fix this
            }
        }

        private void MessageDirector()
        {
            Byte length = bitBuffer.ReadByte();
            Seek(length);
        }

        private void MessageVoiceInit()
        {
            // string: codec name (sv_voicecodec, either voice_miles or voice_speex)
            // byte: quality (sv_voicequality, 1 to 5)

            bitBuffer.ReadString();

            if (demo.NetworkProtocol >= 47 || demo.IsBetaSteam())
            {
                Seek(1);
            }
        }

        private void MessageVoiceData()
        {
            // byte: client id/slot?
            // short: data length
            // length bytes: data

            Seek(1);
            UInt16 length = bitBuffer.ReadUInt16();
            bitBuffer.SeekBytes((Int32)length);
        }

        private void MessageSendExtraInfo()
        {
            // string: "com_clientfallback", always seems to be null
            // byte: sv_cheats

            // NOTE: had this backwards before, shouldn't matter

            bitBuffer.ReadString();
            Seek(1);
        }

        private void MessageResourceLocation()
        {
            // string: location?
            bitBuffer.ReadString();
        }

        private void MessageSendCvarValue()
        {
            bitBuffer.ReadString(); // The cvar.
        }

        private void MessageSendCvarValue2()
        {
            Seek(4); // unsigned int
            bitBuffer.ReadString(); // The cvar.
        }
        #endregion
    }
}
