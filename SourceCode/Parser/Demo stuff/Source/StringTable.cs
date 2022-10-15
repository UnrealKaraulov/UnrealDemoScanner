using System;
using System.IO;
using System.Windows.Forms;

namespace DemoScanner.DemoStuff.Source
{
    class StringTable
    {
        public class PlayerInfo
        {
            public PlayerInfo() { }

            public PlayerInfo(BinaryReader reader)
            {
                Version = reader.ReadInt64SwapEndian();
                XUID = reader.ReadInt64SwapEndian();
                Name = reader.ReadCString(128);
                UserID = reader.ReadInt32SwapEndian();
                GUID = reader.ReadCString(33);
                FriendsID = reader.ReadInt32SwapEndian();
                FriendsName = reader.ReadCString(128);

                IsFakePlayer = reader.ReadBoolean();
                IsHLTV = reader.ReadBoolean();

                customFiles0 = reader.ReadInt32();
                customFiles1 = reader.ReadInt32();
                customFiles2 = reader.ReadInt32();
                customFiles3 = reader.ReadInt32();

                filesDownloaded = reader.ReadByte();
            }

            /// version for future compatibility
            public long Version { get; set; }

            // network xuid
            public long XUID { get; set; }
            // scoreboard information
            public string Name { get; set; } //MAX_PLAYER_NAME_LENGTH=128
                                             // local server user ID, unique while server is running
            public int UserID { get; set; }
            // global unique player identifer
            public string GUID { get; set; } //33bytes
                                             // friends identification number
            public int FriendsID { get; set; }
            // friends name
            public string FriendsName { get; set; } //128
                                                    // true, if player is a bot controlled by game.dll
            public bool IsFakePlayer { get; set; }
            // true if player is the HLTV proxy
            public bool IsHLTV { get; set; }
            // custom files CRC for this player
            public int customFiles0 { get; set; }
            public int customFiles1 { get; set; }
            public int customFiles2 { get; set; }
            public int customFiles3 { get; set; }
            private byte filesDownloaded { get; set; }
            // this counter increases each time the server downloaded a new file
            private byte FilesDownloaded { get; set; }

            public static int SizeOf => 190;

            public static PlayerInfo ParseFrom(BinaryReader reader)
            {
                return new PlayerInfo(reader);
            }
        }

        public static void ParsePacket(byte[] data, TreeNode node)
        {
            var bb = new BitBuffer(data);
            int numTables = bb.ReadByte();

            for (var i = 0; i < numTables; i++)
            {
                var tableName = bb.ReadString();
                var sub = new TreeNode(tableName);
                ParseStringTable(bb, tableName, sub);
                node.Nodes.Add(sub);
            }

        }
        //Might be interesting: https://github.com/LestaD/SourceEngine2007/blob/43a5c90a5ada1e69ca044595383be67f40b33c61/src_main/common/protocol.h
        private static void ParseStringTable(BitBuffer bb, string tableName, TreeNode node)
        {
            var numStrings = bb.ReadInt16();
            for (var i = 0; i < numStrings; i++)
            {
                var stringName = bb.ReadString();

                if (stringName.Length >= 100)
                    throw new Exception("Roy said I should throw this.");

                if (bb.ReadBoolean())
                {
                    var userDataSize = bb.ReadInt16();
                    var data = bb.ReadBytes(userDataSize);
                    if (tableName == "userinfo")
                    {
                        var info = new PlayerInfo();
                        node.Nodes.Add("Version: " + info.Version);
                        node.Nodes.Add("Network XUID: " + info.XUID);
                        node.Nodes.Add("Scoreboard info: " + info.Name);
                        node.Nodes.Add("Local user id: " + info.UserID);
                        node.Nodes.Add("Global user id: " + info.GUID);
                        node.Nodes.Add("Friends ID: " + info.FriendsID);
                        node.Nodes.Add("Friends name: " + info.FriendsName);
                        node.Nodes.Add("Bot: " + info.IsFakePlayer);
                        node.Nodes.Add("HTLV: " + info.IsHLTV);
                        node.Nodes.Add("Costumfile 1: " + info.customFiles0);
                        node.Nodes.Add("Costumfile 2: " + info.customFiles1);
                        node.Nodes.Add("Costumfile 3: " + info.customFiles2);
                        node.Nodes.Add("Costumfile 4: " + info.customFiles3);
                    }
                    else if (tableName == "soundprecache" || tableName == "decalprecache" || tableName == "modelprecache")
                    {
                        node.Nodes.Add(stringName);
                    }
                    else if (tableName == "server_query_info")
                    {
                        node.Nodes.Add(stringName + ": " + bb.ReadUInt16());
                    }
                    else if (tableName == "instancebaseline")
                    {
                        node.Nodes.Add("ID: " + int.Parse(stringName));
                    }
                    else
                    {
                        node.Nodes.Add(stringName);
                        node.Nodes.Add("Data -[" + data.Length + "]bytes");
                    }

                    //TODO: Parse data
                }
                else
                {
                    node.Nodes.Add("No extra data");
                }
            }

            // Client side stuff
            if (bb.ReadBoolean())
            {
                var numstrings = bb.ReadInt16();
                for (var i = 0; i < numstrings; i++)
                {
                    var datatype = bb.ReadString(); // stringname

                    if (bb.ReadBoolean())
                    {
                        var userDataSize = bb.ReadInt16();
                        node.Nodes.Add("Userdata: " + userDataSize + "bytes (" + datatype + ")");
                        bb.ReadBytes(userDataSize);
                    }
                    else
                    {
                        node.Nodes.Add("No user data");
                    }
                }
            }
        }
    }
}
