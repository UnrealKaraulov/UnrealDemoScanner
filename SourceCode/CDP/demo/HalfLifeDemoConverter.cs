using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace compLexity_Demo_Player
{
    public class NoFreeUserMessageException : Exception
    {
        public NoFreeUserMessageException()
        {
        }
    }

    public class HalfLifeDemoConverter : IHalfLifeDemoWriter
    {
        // a svc_resourcelist entry
        private class Resource
        {
            public UInt32 type;
            public String name;
            public UInt32 index;
            public Int32 fileSize;
            public UInt32 flags;
            public Byte[] md5Hash;
            public Boolean hasReservedData;
            public Byte[] reservedData;
        }

        private HalfLifeDemo demo;
        private HalfLifeDemoParser parser;
        private Byte firstFreeUserMessage;
        private Boolean haveParsedDirectorMessage = false;

        public HalfLifeDemoConverter(HalfLifeDemo demo)
        {
            this.demo = demo;

            // Find the first free user message.
            if (demo.Game != null && demo.Game.UserMessages != null)
            {
                Byte highestUserMessage = 0;

                foreach (KeyValuePair<String, Byte> userMessage in demo.Game.UserMessages)
                {
                    if (userMessage.Value > highestUserMessage)
                    {
                        highestUserMessage = userMessage.Value;
                    }
                }

                if (highestUserMessage == 255)
                {
                    throw new NoFreeUserMessageException();
                }

                firstFreeUserMessage = (Byte)(highestUserMessage + 1);
            }
        }

        #region Interface
        public void AddMessageHandlers(HalfLifeDemoParser parser)
        {
            this.parser = parser;

            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_event, MessageEvent);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_sound, MessageSound);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_serverinfo, MessageServerInfo);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_updateuserinfo, MessageUpdateUserInfo);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_deltadescription, MessageDeltaDescription);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_clientdata, MessageClientData);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_pings, MessagePings);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_spawnbaseline, MessageSpawnBaseline);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_newusermsg, MessageNewUserMsg);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_packetentities, MessagePacketEntities);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_deltapacketentities, MessageDeltaPacketEntities);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_resourcelist, MessageResourceList);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_hltv, MessageHltv);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_director, MessageDirector);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_voiceinit, MessageVoiceInit);
            parser.AddMessageHandler((Byte)HalfLifeDemoParser.MessageId.svc_timescale, MessageTimeScale);
            parser.AddUserMessageHandler("ClCorpse", MessageClCorpse);
            parser.AddUserMessageHandler("ScreenFade", MessageScreenFade);
            parser.AddUserMessageHandler("SendAudio", MessageSendAudio);
            parser.AddUserMessageHandler("TextMsg", MessageTextMsg);

            Procedure<String> removeMessage = (s) =>
            {
                Int32 startOffset = parser.BitBuffer.CurrentByte;
                Int32 messageLength = parser.FindUserMessageLength(s);
                Int32 endOffset = parser.BitBuffer.CurrentByte + messageLength;
                parser.Seek(startOffset - 1, SeekOrigin.Begin);
                parser.BitBuffer.RemoveBytes(endOffset - startOffset + 1);
            };

            parser.AddUserMessageHandler("CDChallenge", () =>
            {
                removeMessage("CDChallenge");
            });

            parser.AddUserMessageHandler("CDSalt", () =>
            {
                removeMessage("CDSalt");
            });
        }

        public void ProcessHeader(ref Byte[] header)
        {
            if (demo.ConvertToCurrentNetworkProtocol())
            {
                header[12] = HalfLifeDemo.CurrentNetworkProtocol;
            }
            else if (demo.ConvertNetworkProtocol())
            {
                header[12] = 47;
            }
        }

        public void ProcessFirstGameDataFrame(ref Byte[] frameData)
        {
            // A svc_director message preceeds svc_spawnbaseline in newer HLTV demos. The director message is used to initialise and reset the HUD. Since old HLTV demos omit this message, it can be added here (and set the perspective to first-person too).
            if (demo.Perspective != Demo.Perspectives.Hltv || haveParsedDirectorMessage)
            {
                return;
            }

            const Byte DRC_CMD_START = 1;
            const Byte DRC_CMD_MODE = 3;
            const Byte OBS_IN_EYE = 4;

            // Create a svc_director message to initialise the HUD.
            BitWriter directorMessage = new BitWriter();
            directorMessage.WriteByte((Byte)HalfLifeDemoParser.MessageId.svc_director);
            directorMessage.WriteByte(1); // length
            directorMessage.WriteByte(DRC_CMD_START);

            // Create a svc_director message to set the perspective to first-person.
            directorMessage.WriteByte((Byte)HalfLifeDemoParser.MessageId.svc_director);
            directorMessage.WriteByte(2); // length
            directorMessage.WriteByte(DRC_CMD_MODE);
            directorMessage.WriteByte(OBS_IN_EYE);

            // Insert the new messages.
            BitBuffer bitBuffer = new BitBuffer(frameData);
            bitBuffer.InsertBytes(directorMessage.Data);

            frameData = bitBuffer.Data;
        }

        public Boolean ShouldParseGameDataMessages(Byte frameType)
        {
            return (demo.ConvertNetworkProtocol() || frameType == 0 || (GameManager.CanRemoveFadeToBlack(demo) && demo.Perspective == Demo.Perspectives.Pov && Config.Settings.PlaybackRemoveFtb) || (GameManager.CanRemoveHltvAds(demo) && Config.Settings.PlaybackRemoveHltvAds) || (GameManager.CanRemoveHltvSlowMotion(demo) && Config.Settings.PlaybackRemoveHltvSlowMotion));
        }

        public Boolean ShouldWriteClientCommand(String command)
        {
            if (Config.Settings.PlaybackRemoveShowscores && (command == "+showscores" || command == "-showscores"))
            {
                return false;
            }

            if (command.StartsWith("adjust_crosshair"))
            {
                return false;
            }

            return true;
        }

        public Byte GetNewUserMessageId(Byte messageId)
        {
            // beta steam demos, or any other network protocol conversion
            if (!demo.ConvertNetworkProtocol())
            {
                return messageId;
            }

            if (demo.Game == null || demo.Game.UserMessages == null)
            {
                return messageId;
            }

            String name = parser.FindMessageIdString(messageId);

            if (!demo.Game.UserMessages.ContainsKey(name))
            {
                // shouldn't happen, must be a bad message
                // let the parser handle it
                return messageId;
            }

            return demo.Game.UserMessages[name];
        }

        public void WriteDemoInfo(Byte[] demoInfo, MemoryStream ms)
        {
            if (demo.ConvertNetworkProtocol() && demo.NetworkProtocol <= 43)
            {
                // zero out some data
                for (Int32 i = 28; i < 436; i++)
                {
                    demoInfo[i] = (Byte)0;
                }

                // move view model info
                for (Int32 i = 0; i < 15; i++)
                {
                    demoInfo[421 + i] = demoInfo[517 + i];
                }

                // copy only what we need (to match the length of the current network protocol)
                ms.Write(demoInfo, 0, 436);
            }
            else
            {
                ms.Write(demoInfo, 0, demoInfo.Length);
            }
        }
        #endregion

        private void ReWriteMessage(Int32 messageStartOffset, Byte[] data)
        {
            // remove old message
            Int32 messageEndOffset = parser.BitBuffer.CurrentByte;
            parser.Seek(messageStartOffset, SeekOrigin.Begin);
            parser.BitBuffer.RemoveBytes(messageEndOffset - messageStartOffset);

            // insert new message
            parser.BitBuffer.InsertBytes(data);
        }

        #region Message Handlers
        private void MessageEvent()
        {
            if (!demo.ConvertNetworkProtocol())
            {
                parser.MessageEvent();
                return;
            }

            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;

            // read message
            if (demo.NetworkProtocol <= 43)
            {
                parser.BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            BitWriter bitWriter = new BitWriter();
            HalfLifeDeltaStructure eventStructure = parser.GetDeltaStructure("event_t");

            UInt32 nEvents = parser.BitBuffer.ReadUnsignedBits(5);
            bitWriter.WriteUnsignedBits(nEvents, 5);

            for (Int32 i = 0; i < nEvents; i++)
            {
                UInt32 eventIndex = parser.BitBuffer.ReadUnsignedBits(10);
                bitWriter.WriteUnsignedBits(eventIndex, 10); // event index

                Boolean packetIndexBit = parser.BitBuffer.ReadBoolean();
                bitWriter.WriteBoolean(packetIndexBit);

                if (packetIndexBit)
                {
                    bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(11), 11); // packet index

                    Boolean deltaBit = parser.BitBuffer.ReadBoolean();
                    bitWriter.WriteBoolean(deltaBit);

                    if (deltaBit)
                    {
                        HalfLifeDelta delta = eventStructure.CreateDelta();
                        Byte[] bitmaskBytes;
                        eventStructure.ReadDelta(parser.BitBuffer, delta, out bitmaskBytes);

                        if (demo.Game != null)
                        {
                            demo.Game.ConvertEventCallback(demo, delta, eventIndex);
                        }

                        eventStructure.WriteDelta(bitWriter, delta, bitmaskBytes);
                    }
                }

                Boolean fireTimeBit = parser.BitBuffer.ReadBoolean();
                bitWriter.WriteBoolean(fireTimeBit);

                if (fireTimeBit)
                {
                    bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(16), 16); // fire time
                }
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageSound()
        {
            if (!demo.ConvertNetworkProtocol() || demo.NetworkProtocol > 43)
            {
                parser.MessageSound();
                return;
            }

            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;

            // read message
            parser.BitBuffer.Endian = BitBuffer.EndianType.Big;

            BitWriter bitWriter = new BitWriter();

            UInt32 flags = parser.BitBuffer.ReadUnsignedBits(9);
            bitWriter.WriteUnsignedBits(flags, 9);

            if ((flags & (1 << 0)) != 0) // volume
            {
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(8), 8);
            }

            if ((flags & (1 << 1)) != 0) // attenuation * 64
            {
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(8), 8);
            }

            bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(3), 3); // channel
            bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(11), 11); // edict number

            if ((flags & (1 << 2)) != 0) // sound index (short)
            {
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(16), 16);
            }
            else // sound index (byte)
            {
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(8), 8);
            }

            bitWriter.WriteVectorCoord(true, parser.BitBuffer.ReadVectorCoord(true)); // position

            if ((flags & (1 << 3)) != 0) // pitch
            {
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(8), 8);
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageServerInfo()
        {
            if (demo.ConvertToCurrentNetworkProtocol())
            {
                parser.BitBuffer.RemoveBytes(4);
                parser.BitBuffer.InsertBytes(new Byte[] { (Byte)HalfLifeDemo.CurrentNetworkProtocol, 0, 0, 0 });
            }
            else if (demo.ConvertNetworkProtocol())
            {
                parser.BitBuffer.RemoveBytes(4);
                parser.BitBuffer.InsertBytes(new Byte[] { (Byte)47, 0, 0, 0 });
            }
            else
            {
                parser.Seek(4); // network protocol
            }

            parser.Seek(4); // process count
            parser.Seek(4); // munged map checksum
            parser.Seek(16); // client.dll checksum
            parser.Seek(1); // max clients
            parser.Seek(1); // recorder slot
            parser.Seek(1); // deathmatch/coop flag
            parser.BitBuffer.ReadString(); // game folder

            // server name
            if (demo.NetworkProtocol > 43)
            {
                parser.BitBuffer.ReadString();
            }
            else
            {
                if (demo.ConvertNetworkProtocol())
                {
                    Byte[] serverNameBytes = Encoding.ASCII.GetBytes(demo.ServerName);
                    parser.BitBuffer.InsertBytes(serverNameBytes);
                    parser.BitBuffer.InsertBytes(new Byte[] { (Byte)0 }); // null terminator
                }
            }

            // skip map
            parser.BitBuffer.ReadString();

            if (demo.NetworkProtocol == 45)
            {
                Byte extraInfo = parser.BitBuffer.ReadByte();
                parser.Seek(-1);

                if (extraInfo == (Byte)HalfLifeDemoParser.MessageId.svc_sendextrainfo)
                {
                    goto InsertMapCycle;
                }
            }

            parser.BitBuffer.ReadString(); // skip mapcycle

            if (demo.NetworkProtocol <= 43)
            {
                goto InsertExtraFlag;
            }

            if (parser.BitBuffer.ReadByte() > 0)
            {
                parser.Seek(-1);
                parser.BitBuffer.RemoveBytes(demo.NetworkProtocol == 45 ? 34 : 22);
                goto InsertExtraFlag;
            }
            else
            {
                return;
            }

            InsertMapCycle:
                parser.BitBuffer.InsertBytes(new Byte[] { 0 });

            InsertExtraFlag:
                parser.BitBuffer.InsertBytes(new Byte[] { 0 });
        }

        private void MessageUpdateUserInfo()
        {
            parser.MessageUpdateUserInfo();

            // insert empty string hash
            if (demo.NetworkProtocol <= 43 && demo.ConvertNetworkProtocol())
            {
                parser.BitBuffer.InsertBytes(new Byte[16]);
            }
        }

        private void MessageDeltaDescription()
        {
            if (!demo.ConvertNetworkProtocol() || demo.NetworkProtocol > 43)
            {
                parser.MessageDeltaDescription();
                return;
            }

            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;
            parser.BitBuffer.Endian = BitBuffer.EndianType.Big;
            BitWriter bitWriter = new BitWriter();

            // read/write message
            String structureName = parser.BitBuffer.ReadString();
            bitWriter.WriteString(structureName);

            UInt32 nEntries = parser.BitBuffer.ReadUnsignedBits(16);
            bitWriter.WriteUnsignedBits(nEntries, 16);

            HalfLifeDeltaStructure newDeltaStructure = new HalfLifeDeltaStructure(structureName);
            parser.AddDeltaStructure(newDeltaStructure);

            HalfLifeDeltaStructure deltaDescription = parser.GetDeltaStructure("delta_description_t");

            for (UInt16 i = 0; i < nEntries; i++)
            {
                HalfLifeDelta delta = deltaDescription.CreateDelta();
                Byte[] bitmaskBytes;
                deltaDescription.ReadDelta(parser.BitBuffer, delta, out bitmaskBytes);

                if (demo.Game != null)
                {
                    demo.Game.ConvertDeltaDescriptionCallback(demo.GameVersion, structureName, delta);
                }

                deltaDescription.WriteDelta(bitWriter, delta, bitmaskBytes);
                newDeltaStructure.AddEntry(delta);
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageClientData()
        {
            if (demo.Perspective == Demo.Perspectives.Hltv)
            {
                return;
            }

            if (!demo.ConvertNetworkProtocol() || demo.IsBetaSteam())
            {
                parser.MessageClientData();
                return;
            }

            // read message
            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;

            if (demo.NetworkProtocol <= 43)
            {
                parser.BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            BitWriter bitWriter = new BitWriter();
            HalfLifeDeltaStructure clientDataStructure = parser.GetDeltaStructure("clientdata_t");
            HalfLifeDeltaStructure weaponDataStructure = parser.GetDeltaStructure("weapon_data_t");

            Boolean deltaSequence = parser.BitBuffer.ReadBoolean();
            bitWriter.WriteBoolean(deltaSequence);

            UInt32 deltaSequenceNumber = 0;

            if (deltaSequence)
            {
                deltaSequenceNumber = parser.BitBuffer.ReadUnsignedBits(8);
                bitWriter.WriteUnsignedBits(deltaSequenceNumber, 8);
            }

            HalfLifeDelta clientData = clientDataStructure.CreateDelta();
            Byte[] clientDataBitmaskBytes;
            clientDataStructure.ReadDelta(parser.BitBuffer, clientData, out clientDataBitmaskBytes);
            clientDataStructure.WriteDelta(bitWriter, clientData, clientDataBitmaskBytes);

            while (parser.BitBuffer.ReadBoolean())
            {
                bitWriter.WriteBoolean(true);

                if (demo.NetworkProtocol < 47 && !demo.IsBetaSteam())
                {
                    bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(5), 6);
                }
                else
                {
                    bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(6), 6);
                }

                HalfLifeDelta weaponData = weaponDataStructure.CreateDelta();
                Byte[] bitmaskBytes;
                weaponDataStructure.ReadDelta(parser.BitBuffer, weaponData, out bitmaskBytes);
                weaponDataStructure.WriteDelta(bitWriter, weaponData, bitmaskBytes);
            }

            bitWriter.WriteBoolean(false);

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessagePings()
        {
            if (!demo.ConvertNetworkProtocol() || demo.NetworkProtocol > 43)
            {
                parser.MessagePings();
                return;
            }

            // read into new message
            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;
            BitWriter bitWriter = new BitWriter();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Big;

            while (parser.BitBuffer.ReadBoolean())
            {
                bitWriter.WriteBoolean(true);
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(5), 5); // slot
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(12), 12); // ping
                bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(7), 7); // loss
            }

            bitWriter.WriteBoolean(false);

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageSpawnBaseline()
        {
            if (!demo.ConvertNetworkProtocol())
            {
                parser.MessageSpawnBaseline();
                return;
            }

            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;
            BitWriter bitWriter = new BitWriter();

            // read message into new message
            if (demo.NetworkProtocol <= 43)
            {
                parser.BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            while (true)
            {
                UInt32 entityIndex = parser.BitBuffer.ReadUnsignedBits(11);
                bitWriter.WriteUnsignedBits(entityIndex, 11);

                if (entityIndex == (1 << 11) - 1) // all 1's
                {
                    break;
                }

                UInt32 entityType = parser.BitBuffer.ReadUnsignedBits(2);
                bitWriter.WriteUnsignedBits(entityType, 2);

                String entityTypeString;

                if ((entityType & 1) != 0)
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

                HalfLifeDeltaStructure deltaStructure = parser.GetDeltaStructure(entityTypeString);
                HalfLifeDelta delta = deltaStructure.CreateDelta();
                Byte[] bitmaskBytes;
                deltaStructure.ReadDelta(parser.BitBuffer, delta, out bitmaskBytes);

                if (demo.Game != null)
                {
                    demo.Game.ConvertPacketEntititiesCallback(delta, entityTypeString, demo.GameVersion);
                }

                deltaStructure.WriteDelta(bitWriter, delta, bitmaskBytes);
            }

            UInt32 footer = parser.BitBuffer.ReadUnsignedBits(5); // should be all 1's
            bitWriter.WriteUnsignedBits(footer, 5);

            if (footer != (1 << 5) - 1)
            {
                throw new ApplicationException("Bad svc_spawnbaseline footer.");
            }

            UInt32 nExtraData = parser.BitBuffer.ReadUnsignedBits(6);
            bitWriter.WriteUnsignedBits(nExtraData, 6);

            HalfLifeDeltaStructure entityStateStructure = parser.GetDeltaStructure("entity_state_t");

            for (Int32 i = 0; i < nExtraData; i++)
            {
                HalfLifeDelta delta = entityStateStructure.CreateDelta();
                Byte[] bitmaskBytes;
                entityStateStructure.ReadDelta(parser.BitBuffer, delta, out bitmaskBytes);
                entityStateStructure.WriteDelta(bitWriter, delta, bitmaskBytes);
            }

            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;
            parser.BitBuffer.SkipRemainingBits();

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageNewUserMsg()
        {
            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;

            // read message
            Byte id = parser.BitBuffer.ReadByte();
            SByte length = parser.BitBuffer.ReadSByte();
            String name = parser.BitBuffer.ReadString(16);

            parser.AddUserMessage(id, length, name);

            // FIXME: clean this up
            if (!demo.ConvertNetworkProtocol() && name != "CDChallenge" && name != "CDSalt")
            {
                return;
            }

            if (demo.Game == null || demo.Game.UserMessages == null)
            {
                return;
            }

            Byte newId;

            if (demo.Game.UserMessages.ContainsKey(name))
            {
                newId = demo.Game.UserMessages[name];
            }
            else
            {
                // cheating death
                // TODO: probably should have a list of "bad" user messages to remove...
                // TODO: should remove these messages even when not converting network protocols
                if (name == "CDChallenge" || name == "CDSalt")
                {
                    // remove message
                    Int32 messageFinishOffset = parser.BitBuffer.CurrentByte;
                    parser.BitBuffer.SeekBytes(messageStartOffset - 1, SeekOrigin.Begin);
                    parser.BitBuffer.RemoveBytes(messageFinishOffset - messageStartOffset + 1); // +1 for message id
                    return;
                }

                // user message doesn't exist in CS 1.6. shouldn't happen, but meh...
                // TODO: use an id unused by compatibleUserMessageTable
                //newId = (Byte?)id;
                newId = firstFreeUserMessage;

                if (firstFreeUserMessage == 255)
                {
                    throw new NoFreeUserMessageException();
                }

                firstFreeUserMessage++;

                demo.Game.UserMessages.Add(name, newId);
            }

            BitWriter bitWriter = new BitWriter();
            bitWriter.WriteByte((Byte)newId);
            bitWriter.WriteSByte(length);
            bitWriter.WriteString(name, 16);

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessagePacketEntities()
        {
            if (!demo.ConvertNetworkProtocol())
            {
                parser.MessagePacketEntities();
                return;
            }

            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;
            BitWriter bitWriter = new BitWriter();

            // read message into new message
            bitWriter.WriteUInt16(parser.BitBuffer.ReadUInt16()); // nEntities/maxEntities

            if (demo.NetworkProtocol <= 43)
            {
                parser.BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            UInt32 entityNumber = 0;

            while (true)
            {
                UInt16 footer = parser.BitBuffer.ReadUInt16();

                if (footer == 0)
                {
                    bitWriter.WriteUInt16(footer);
                    break;
                }
                else
                {
                    parser.BitBuffer.SeekBits(-16);
                }

                if (!parser.BitBuffer.ReadBoolean()) // entity number isn't last entity number + 1, need to read it in
                {
                    bitWriter.WriteBoolean(false);

                    // is the following entity number absolute, or relative from the last one?
                    if (parser.BitBuffer.ReadBoolean())
                    {
                        bitWriter.WriteBoolean(true);
                        entityNumber = parser.BitBuffer.ReadUnsignedBits(11);
                        bitWriter.WriteUnsignedBits(entityNumber, 11);
                    }
                    else
                    {
                        bitWriter.WriteBoolean(false);
                        UInt32 entityNumberDelta = parser.BitBuffer.ReadUnsignedBits(6);
                        bitWriter.WriteUnsignedBits(entityNumberDelta, 6);
                        entityNumber += entityNumberDelta;
                    }
                }
                else
                {
                    bitWriter.WriteBoolean(true);
                    entityNumber++;
                }

                Boolean custom = parser.BitBuffer.ReadBoolean();
                bitWriter.WriteBoolean(custom);
                Boolean baseline = parser.BitBuffer.ReadBoolean();
                bitWriter.WriteBoolean(baseline);

                if (baseline)
                {
                    bitWriter.WriteUnsignedBits(parser.BitBuffer.ReadUnsignedBits(6), 6); // baseline index
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

                HalfLifeDeltaStructure entityStateStructure = parser.GetDeltaStructure(entityType);
                HalfLifeDelta delta = entityStateStructure.CreateDelta();
                Byte[] bitmaskBytes;
                entityStateStructure.ReadDelta(parser.BitBuffer, delta, out bitmaskBytes);

                if (demo.Game != null)
                {
                    demo.Game.ConvertPacketEntititiesCallback(delta, entityType, demo.GameVersion);
                }

                entityStateStructure.WriteDelta(bitWriter, delta, bitmaskBytes);
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageDeltaPacketEntities()
        {
            if (!demo.ConvertNetworkProtocol() || demo.IsBetaSteam())
            {
                parser.MessageDeltaPacketEntities();
                return;
            }

            Int32 messageStartOffset = parser.BitBuffer.CurrentByte;
            BitWriter bitWriter = new BitWriter();

            // read message
            bitWriter.WriteUInt16(parser.BitBuffer.ReadUInt16()); // nEntities/maxEntities

            if (demo.NetworkProtocol <= 43)
            {
                parser.BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            bitWriter.WriteByte(parser.BitBuffer.ReadByte()); // delta sequence number

            UInt32 entityNumber = 0;

            while (true)
            {
                // check for footer
                UInt16 footer = parser.BitBuffer.ReadUInt16();

                if (footer == 0)
                {
                    bitWriter.WriteUInt16(footer);
                    break;
                }

                parser.BitBuffer.SeekBits(-16);

                // option bits
                Boolean removeEntity = parser.BitBuffer.ReadBoolean();
                bitWriter.WriteBoolean(removeEntity);
                Boolean absoluteEntityNumber = parser.BitBuffer.ReadBoolean();
                bitWriter.WriteBoolean(absoluteEntityNumber);

                // entity number
                if (absoluteEntityNumber)
                {
                    entityNumber = parser.BitBuffer.ReadUnsignedBits(11);
                    bitWriter.WriteUnsignedBits(entityNumber, 11);
                }
                else
                {
                    UInt32 deltaEntityNumber = parser.BitBuffer.ReadUnsignedBits(6);
                    bitWriter.WriteUnsignedBits(deltaEntityNumber, 6);
                    entityNumber += deltaEntityNumber;
                }

                if (!removeEntity)
                {
                    // entity type
                    Boolean custom = parser.BitBuffer.ReadBoolean();
                    bitWriter.WriteBoolean(custom);

                    if (demo.NetworkProtocol <= 43)
                    {
                        parser.BitBuffer.SeekBits(1); // unknown, always 0
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

                    // delta compressed data
                    Byte[] bitmaskBytes;
                    HalfLifeDeltaStructure deltaDecoder = parser.GetDeltaStructure(entityType);
                    HalfLifeDelta deltaEntity = deltaDecoder.CreateDelta();
                    deltaDecoder.ReadDelta(parser.BitBuffer, deltaEntity, out bitmaskBytes);

                    if (demo.Game != null)
                    {
                        demo.Game.ConvertPacketEntititiesCallback(deltaEntity, entityType, demo.GameVersion);
                    }

                    deltaDecoder.WriteDelta(bitWriter, deltaEntity, bitmaskBytes);
                }
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // insert new message
            ReWriteMessage(messageStartOffset, bitWriter.Data);
        }

        private void MessageResourceList()
        {
            Int32 startByteIndex = parser.BitBuffer.CurrentByte;

            // read message
            if (demo.NetworkProtocol <= 43)
            {
                parser.BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            UInt32 nEntries = parser.BitBuffer.ReadUnsignedBits(12);
            List<Resource> resourceList = new List<Resource>((Int32)nEntries);

            for (Int32 i = 0; i < nEntries; i++)
            {
                Resource r = new Resource();

                r.type = parser.BitBuffer.ReadUnsignedBits(4);
                r.name = parser.BitBuffer.ReadString();
                r.index = parser.BitBuffer.ReadUnsignedBits(12);
                r.fileSize = parser.BitBuffer.ReadBits(24); // signed?
                r.flags = parser.BitBuffer.ReadUnsignedBits(3);

                if ((r.flags & 4) != 0) // md5 hash (RES_CUSTOM?)
                {
                    r.md5Hash = parser.BitBuffer.ReadBytes(16);
                }

                r.hasReservedData = parser.BitBuffer.ReadBoolean();

                if (r.hasReservedData)
                {
                    r.reservedData = parser.BitBuffer.ReadBytes(32);
                }

                if (demo.Game == null || demo.Game.ConvertResourceListCallback(demo, r.type, r.index, ref r.name))
                {
                    resourceList.Add(r);
                }
            }

            // consistency list
            // indices of resources to force consistency upon?
            if (parser.BitBuffer.ReadBoolean())
            {
                while (parser.BitBuffer.ReadBoolean())
                {
                    Int32 nBits = (parser.BitBuffer.ReadBoolean() ? 5 : 10);
                    parser.BitBuffer.SeekBits(nBits);
                }
            }

            parser.BitBuffer.SkipRemainingBits();
            parser.BitBuffer.Endian = BitBuffer.EndianType.Little;

            // stop now if we're not converting network protocols
            if (!demo.ConvertNetworkProtocol())
            {
                return;
            }

            // create new message
            BitWriter bitWriter = new BitWriter();

            bitWriter.WriteUnsignedBits((UInt32)resourceList.Count, 12);

            foreach (Resource r in resourceList)
            {
                bitWriter.WriteUnsignedBits(r.type, 4);
                bitWriter.WriteString(r.name);
                bitWriter.WriteUnsignedBits(r.index, 12);
                bitWriter.WriteBits(r.fileSize, 24);
                bitWriter.WriteUnsignedBits(r.flags, 3);

                if ((r.flags & 4) != 0) // md5 hash
                {
                    bitWriter.WriteBytes(r.md5Hash);
                }

                bitWriter.WriteBoolean(r.hasReservedData);

                if (r.hasReservedData)
                {
                    bitWriter.WriteBytes(r.reservedData);
                }
            }

            bitWriter.WriteBoolean(false); // consistency list

            // remove old message
            Int32 endByteIndex = parser.BitBuffer.CurrentByte;
            parser.Seek(startByteIndex, SeekOrigin.Begin);
            parser.BitBuffer.RemoveBytes(endByteIndex - startByteIndex);

            // insert new message into bitbuffer
            parser.BitBuffer.InsertBytes(bitWriter.Data);
        }

        // TODO: this needs work
        private void MessageHltv()
        {
            //perspective = PerspectiveEnum.Hltv;

            /*
            #define HLTV_ACTIVE				0	// tells client that he's an spectator and will get director commands
            #define HLTV_STATUS				1	// send status infos about proxy 
            #define HLTV_LISTEN				2	// tell client to listen to a multicast stream
             */

            Byte subCommand = parser.BitBuffer.ReadByte();

            if (subCommand == 2) // HLTV_LISTEN
            {
                // remove entire message
                parser.Seek(-2);
                parser.BitBuffer.RemoveBytes(10);
                //bitBuffer.InsertBytes(bitBuffer.GetNumBytesRead(), new Byte[] { 0 });
                //parser.Seek(8);
            }
            else if (subCommand == 1)
            {
                // TODO: fix this
                //MessageBox.Show("HLTV_STATUS");
            }
        }

        private void MessageDirector()
        {
            haveParsedDirectorMessage = true;

            // see HL SDK common/hltv.h
            const int DRC_CMD_MESSAGE = 6;

            Int32 startByteIndex = parser.BitBuffer.CurrentByte;
            Byte length = parser.BitBuffer.ReadByte();
            Byte type = parser.BitBuffer.ReadByte();

            if (Config.Settings.PlaybackRemoveHltvAds && type == DRC_CMD_MESSAGE)
            {
                parser.Seek(startByteIndex - 1, SeekOrigin.Begin);
                parser.BitBuffer.RemoveBytes(1 + 1 + length);
            }
            else
            {
                parser.Seek(startByteIndex + 1 + length, SeekOrigin.Begin);
            }
        }

        private void MessageVoiceInit()
        {
            parser.BitBuffer.ReadString();

            if (!demo.ConvertNetworkProtocol() || demo.IsBetaSteam())
            {
                if (demo.NetworkProtocol >= 47 || demo.IsBetaSteam())
                {
                    parser.Seek(1);
                }
            }
            else
            {
                parser.BitBuffer.InsertBytes(new Byte[] { 5 });
            }
        }

        private void MessageTimeScale()
        {
            float value = parser.BitBuffer.ReadSingle();

            if (Config.Settings.PlaybackRemoveHltvSlowMotion && value < 1.0f)
            {
                parser.Seek(-5);
                parser.BitBuffer.RemoveBytes(5);
            }
        }

        private void MessageTextMsg()
        {
            Byte length = parser.BitBuffer.ReadByte();
            Int32 messageDataOffset = parser.BitBuffer.CurrentByte;

            parser.Seek(1); // slot
            string s = parser.BitBuffer.ReadString();

            // Remove "* No Time Limit *" and "Time Remaining: x".
            if (s.StartsWith("#Game_timelimit") || s.StartsWith("#Game_no_timelimit"))
            {
                // Remove the entire message.
                parser.Seek(messageDataOffset - 2, SeekOrigin.Begin);
                parser.BitBuffer.RemoveBytes(2 + length);
            }
            else
            {
                parser.Seek(messageDataOffset + length, SeekOrigin.Begin);
            }
        }

        private void MessageClCorpse()
        {
            Byte length = parser.BitBuffer.ReadByte();
            Int32 messageDataOffset = parser.BitBuffer.CurrentByte;

            if (demo.ConvertNetworkProtocol() && demo.Game != null)
            {
                // Get the message data and create a BitBuffer for it.
                byte[] messageData = parser.BitBuffer.ReadBytes(length);
                BitBuffer messageBitBuffer = new BitBuffer(messageData);

                // Have the game handler convert the message.
                demo.Game.ConvertClCorpseMessageCallback(demo.GameVersion, messageBitBuffer);

                // Remove the old message data and insert the new converted message data.
                parser.Seek(messageDataOffset, SeekOrigin.Begin);
                parser.BitBuffer.RemoveBytes(length);
                parser.BitBuffer.InsertBytes(messageBitBuffer.Data);
            }
            else
            {
                parser.Seek(length);
            }
        }

        private void MessageScreenFade()
        {
            Int32 startByteIndex = parser.BitBuffer.CurrentByte;

            UInt16 duration = parser.BitBuffer.ReadUInt16();
            UInt16 holdTime = parser.BitBuffer.ReadUInt16();
            UInt16 flags = parser.BitBuffer.ReadUInt16();
            UInt32 colour = parser.BitBuffer.ReadUInt32();

            if (!Config.Settings.PlaybackRemoveFtb)
            {
                return;
            }

            // see if it's fade to black
            // flags: FFADE_OUT | FFADE_STAYOUT
            // could probably just check flags and colour...
            if (duration == 0x3000 && holdTime == 0x3000 && flags == 0x05 && colour == 0xFF000000)
            {
                // remove the entire message
                parser.Seek(startByteIndex - 1, SeekOrigin.Begin);
                parser.BitBuffer.RemoveBytes(11);
            }
        }

        private void MessageSendAudio()
        {
            Byte length = parser.BitBuffer.ReadByte();
            Int32 startOffset = parser.BitBuffer.CurrentByte;

            Byte slot = parser.BitBuffer.ReadByte();
            //String name = parser.BitBuffer.ReadString();

            parser.Seek(startOffset, SeekOrigin.Begin);

            if (demo.ConvertNetworkProtocol())
            {
                // add 2 to length
                parser.Seek(-1);
                parser.BitBuffer.RemoveBytes(1);
                parser.BitBuffer.InsertBytes(new Byte[] { (Byte)(length + 2) });

                // append pitch (short, 100)
                parser.Seek(length);
                parser.BitBuffer.InsertBytes(new Byte[] { (Byte)100, (Byte)0 });
            }
            else
            {
                parser.Seek(length);
            }
        }
        #endregion
    }
}
