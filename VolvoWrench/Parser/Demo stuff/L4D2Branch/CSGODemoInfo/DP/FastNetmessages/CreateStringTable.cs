using System;
using System.IO;
using DemoScanner.DemoStuff.L4D2Branch.BitStreamUtil;
using DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo.DP.Handler;

namespace DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo.DP.FastNetmessages
{
    public struct CreateStringTable
    {
        private int _UserDataFixedSize;
        public int Flags;
        public int MaxEntries;
        public string Name;
        public int NumEntries;
        public int UserDataSize;
        public int UserDataSizeBits;

        public bool UserDataFixedSize => _UserDataFixedSize != 0;

        public void Parse(IBitStream bitstream, DemoParser parser)
        {
            while (!bitstream.ChunkFinished)
            {
                var desc = bitstream.ReadProtobufVarInt();
                var wireType = desc & 7;
                var fieldnum = desc >> 3;

                if (wireType == 2)
                {
                    if (fieldnum == 1)
                    {
                        Name = bitstream.ReadProtobufString();
                        continue;
                    }

                    if (fieldnum == 8)
                    {
                        // String data is special.
                        // We'll simply hope that gaben is nice and sends
                        // string_data last, just like he should.
                        var len = bitstream.ReadProtobufVarInt();
                        bitstream.BeginChunk(len * 8);
                        CreateStringTableUserInfoHandler.Apply(this, bitstream, parser);
                        bitstream.EndChunk();
                        if (!bitstream.ChunkFinished)
                            throw new NotImplementedException("Lord Gaben wasn't nice to us :/");

                        break;
                    }

                    throw new InvalidDataException("yes I know we should drop this but we" +
                                                   "probably want to know that they added a new big field");
                }

                if (wireType != 0) throw new InvalidDataException();

                var val = bitstream.ReadProtobufVarInt();

                switch (fieldnum)
                {
                    case 2:
                        MaxEntries = val;
                        break;
                    case 3:
                        NumEntries = val;
                        break;
                    case 4:
                        _UserDataFixedSize = val;
                        break;
                    case 5:
                        UserDataSize = val;
                        break;
                    case 6:
                        UserDataSizeBits = val;
                        break;
                    case 7:
                        Flags = val;
                        break;
                }
            }
        }
    }
}