using DemoScanner.DemoStuff.L4D2Branch.BitStreamUtil;
using DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo.DP.Handler;
using System.Collections.Generic;
using System.IO;

namespace DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo.DP.FastNetmessages
{
    public struct GameEventList
    {
        public void Parse(IBitStream bitstream, DemoParser parser)
        {
            GameEventHandler.HandleGameEventList(ReadDescriptors(bitstream), parser);
        }

        private IEnumerable<Descriptor> ReadDescriptors(IBitStream bitstream)
        {
            while (!bitstream.ChunkFinished)
            {
                var desc = bitstream.ReadProtobufVarInt();
                var wireType = desc & 7;
                var fieldnum = desc >> 3;
                if (wireType != 2 || fieldnum != 1) throw new InvalidDataException();

                var length = bitstream.ReadProtobufVarInt();
                bitstream.BeginChunk(length * 8);
                var descriptor = new Descriptor();
                descriptor.Parse(bitstream);
                yield return descriptor;
                bitstream.EndChunk();
            }
        }

        public struct Key
        {
            public string Name;
            public int Type;

            public void Parse(IBitStream bitstream)
            {
                while (!bitstream.ChunkFinished)
                {
                    var desc = bitstream.ReadProtobufVarInt();
                    var wireType = desc & 7;
                    var fieldnum = desc >> 3;
                    if (wireType == 0 && fieldnum == 1)
                        Type = bitstream.ReadProtobufVarInt();
                    else if (wireType == 2 && fieldnum == 2)
                        Name = bitstream.ReadProtobufString();
                    else
                        throw new InvalidDataException();
                }
            }
        }

        public struct Descriptor
        {
            public int EventId;
            public Key[] Keys;
            public string Name;

            public void Parse(IBitStream bitstream)
            {
                var keys = new List<Key>();
                while (!bitstream.ChunkFinished)
                {
                    var desc = bitstream.ReadProtobufVarInt();
                    var wireType = desc & 7;
                    var fieldnum = desc >> 3;
                    if (wireType == 0 && fieldnum == 1)
                    {
                        EventId = bitstream.ReadProtobufVarInt();
                    }
                    else if (wireType == 2 && fieldnum == 2)
                    {
                        Name = bitstream.ReadProtobufString();
                    }
                    else if (wireType == 2 && fieldnum == 3)
                    {
                        var length = bitstream.ReadProtobufVarInt();
                        bitstream.BeginChunk(length * 8);
                        var key = new Key();
                        key.Parse(bitstream);
                        keys.Add(key);
                        bitstream.EndChunk();
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }

                Keys = keys.ToArray();
            }
        }
    }
}