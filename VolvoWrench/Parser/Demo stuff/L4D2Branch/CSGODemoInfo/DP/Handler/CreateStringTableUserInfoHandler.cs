using System;
using System.Collections.Generic;
using System.IO;
using DemoScanner.DemoStuff.L4D2Branch.BitStreamUtil;
using DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo.DP.FastNetmessages;

namespace DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo.DP.Handler
{
    public static class CreateStringTableUserInfoHandler
    {
        public static void Apply(CreateStringTable table, IBitStream reader, DemoParser parser)
        {
            if (table.Name == "modelprecache")
                while (parser.modelprecache.Count < table.MaxEntries)
                    parser.modelprecache.Add(null);

            if (reader.ReadBit()) throw new NotImplementedException("Encoded with dictionaries, unable to decode");

            var nTemp = table.MaxEntries;
            var nEntryBits = 0;
            while ((nTemp >>= 1) != 0) ++nEntryBits;

            var history = new List<string>();

            var lastEntry = -1;

            for (var i = 0; i < table.NumEntries; i++)
            {
                var entryIndex = lastEntry + 1;
                // d in the entity-index
                if (!reader.ReadBit()) entryIndex = (int) reader.ReadInt(nEntryBits);

                lastEntry = entryIndex;

                // Read the name of the string into entry.
                var entry = "";
                if (entryIndex < 0 || entryIndex >= table.MaxEntries)
                    throw new InvalidDataException("bogus string index");

                if (reader.ReadBit())
                {
                    var substringcheck = reader.ReadBit();

                    if (substringcheck)
                    {
                        var index = (int) reader.ReadInt(5);
                        var bytestocopy = (int) reader.ReadInt(5);

                        entry = history[index].Substring(0, bytestocopy);

                        entry += reader.ReadString(1024);
                    }
                    else
                    {
                        entry = reader.ReadString(1024);
                    }
                }

                if (entry == null) entry = "";

                if (history.Count > 31) history.RemoveAt(0);

                history.Add(entry);

                // Read in the user data.
                var userdata = new byte[0];
                if (reader.ReadBit())
                {
                    if (table.UserDataFixedSize)
                    {
                        userdata = reader.ReadBits(table.UserDataSizeBits);
                    }
                    else
                    {
                        var bytesToRead = (int) reader.ReadInt(14);

                        userdata = reader.ReadBytes(bytesToRead);
                    }
                }

                if (userdata.Length == 0) break;

                if (table.Name == "userinfo")
                {
                    // Now we'll parse the players out of it.
                    var playerReader = new BinaryReader(new MemoryStream(userdata));
                    var info = PlayerInfo.ParseFrom(playerReader);

                    parser.RawPlayers[entryIndex] = info;
                }
                else if (table.Name == "instancebaseline")
                {
                    var classid = int.Parse(entry); //wtf volvo?

                    parser.instanceBaseline[classid] = userdata;
                }
                else if (table.Name == "modelprecache")
                {
                    parser.modelprecache[entryIndex] = entry;
                }
            }

            parser.stringTables.Add(table);
        }
    }
}