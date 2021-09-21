using System;
using System.IO;
using DemoScanner.DemoStuff.L4D2Branch.BitStreamUtil;

namespace DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo.ST
{
    internal class StringTableParser
    {
        public void ParsePacket(IBitStream reader, DemoParser parser)
        {
            int numTables = reader.ReadByte();

            for (var i = 0; i < numTables; i++)
            {
                var tableName = reader.ReadString();

                ParseStringTable(reader, tableName, parser);
            }
        }

        public void ParseStringTable(IBitStream reader, string tableName, DemoParser parser)
        {
            var numStrings = (int) reader.ReadInt(16);

            if (tableName == "modelprecache") parser.modelprecache.Clear();

            for (var i = 0; i < numStrings; i++)
            {
                var stringName = reader.ReadString();

                if (stringName.Length >= 100) throw new Exception("Roy said I should throw this.");

                if (reader.ReadBit())
                {
                    var userDataSize = (int) reader.ReadInt(16);

                    var data = reader.ReadBytes(userDataSize);

                    if (tableName == "userinfo")
                    {
                        var info = PlayerInfo.ParseFrom(new BinaryReader(new MemoryStream(data)));

                        parser.RawPlayers[int.Parse(stringName)] = info;
                    }
                    else if (tableName == "instancebaseline")
                    {
                        var classid = int.Parse(stringName); //wtf volvo?

                        parser.instanceBaseline[classid] = data;
                    }
                    else if (tableName == "modelprecache")
                    {
                        parser.modelprecache.Add(stringName);
                    }
                }
            }

            // Client side stuff
            if (reader.ReadBit())
            {
                var numstrings = (int) reader.ReadInt(16);
                for (var i = 0; i < numstrings; i++)
                {
                    reader.ReadString(); // stringname

                    if (reader.ReadBit())
                    {
                        var userDataSize = (int) reader.ReadInt(16);

                        reader.ReadBytes(userDataSize);
                    }
                }
            }
        }
    }
}