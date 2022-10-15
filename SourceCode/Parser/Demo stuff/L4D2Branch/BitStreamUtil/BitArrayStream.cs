using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DemoScanner.DemoStuff.L4D2Branch.BitStreamUtil
{
    public class BitArrayStream : IBitStream
    {
        private readonly List<int> RemainingInOldChunks = new List<int>();
        private BitArray array;
        private int RemainingInCurrentChunk = -1;

        public BitArrayStream(byte[] data)
        {
            array = new BitArray(data);
            Position = 0;
        }

        public void Dispose()
        {
            array = null;
        }

        public int Position { get; private set; }

        public void Initialize(Stream stream)
        {
            using (var memstream = new MemoryStream(checked((int)stream.Length)))
            {
                stream.CopyTo(memstream);
                array = new BitArray(memstream.GetBuffer());
            }

            Position = 0;
        }

        public uint ReadInt(int numBits)
        {
            var result = PeekInt(numBits);
            Position += numBits;
            if (RemainingInCurrentChunk >= 0)
            {
                if (numBits > RemainingInCurrentChunk)
                    throw new OverflowException("Trying to read beyond a chunk boundary!");

                RemainingInCurrentChunk -= numBits;
                for (var i = 1; i < RemainingInOldChunks.Count; i++) RemainingInOldChunks[i] -= numBits;
            }

            return result;
        }

        public bool ReadBit()
        {
            return ReadInt(1) == 1;
        }

        public byte ReadByte()
        {
            return (byte)ReadInt(8);
        }

        public byte ReadByte(int numBits)
        {
            return (byte)ReadInt(numBits);
        }

        public byte[] ReadBytes(int length)
        {
            var result = new byte[length];

            for (var i = 0; i < length; i++) result[i] = ReadByte();

            return result;
        }

        public int ReadSignedInt(int numBits)
        {
            // Read the int normally and then shift it back and forth to extend the sign bit.
            return ((int)ReadInt(numBits) << (32 - numBits)) >> (32 - numBits);
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(ReadBytes(4), 0);
        }

        public byte[] ReadBits(int bits)
        {
            var result = new byte[(bits + 7) / 8];

            for (var i = 0; i < bits / 8; i++) result[i] = ReadByte();

            if (bits % 8 != 0) result[bits / 8] = ReadByte(bits % 8);

            return result;
        }

        public int ReadProtobufVarInt()
        {
            return BitStreamUtil.ReadProtobufVarIntStub(this);
        }

        public void BeginChunk(int length)
        {
            if (RemainingInCurrentChunk >= 0 && RemainingInCurrentChunk < length)
                throw new InvalidOperationException("trying to create a too big nested chunk"); // grammar much

            RemainingInOldChunks.Add(RemainingInCurrentChunk);
            RemainingInCurrentChunk = length;
        }

        public void EndChunk()
        {
            ReadBits(RemainingInCurrentChunk); // hella inefficient, but this is the BitArrayStream so no one cares
            var idx = RemainingInOldChunks.Count - 1;
            RemainingInCurrentChunk = RemainingInOldChunks[idx];
            RemainingInOldChunks.RemoveAt(idx);
        }

        public bool ChunkFinished => RemainingInCurrentChunk == 0;

        public void Seek(int pos, SeekOrigin origin)
        {
            if (RemainingInCurrentChunk >= 0) throw new NotSupportedException("Can't seek while inside a chunk");

            if (origin == SeekOrigin.Begin) Position = pos;

            if (origin == SeekOrigin.Current) Position += pos;

            if (origin == SeekOrigin.End) Position = array.Count - pos;
        }

        public uint PeekInt(int numBits)
        {
            uint result = 0;
            var intPos = 0;

            for (var i = 0; i < numBits; i++) result |= (array[i + Position] ? 1u : 0u) << intPos++;

            return result;
        }

        public string PeekBools(int length)
        {
            var buffer = new byte[length];

            var idx = 0;
            for (var i = Position; i < Math.Min(Position + length, array.Count); i++)
                if (array[i])
                    buffer[idx++] = 49;
                else
                    buffer[idx++] = 48;

            return Encoding.ASCII.GetString(buffer, 0, Math.Min(length, array.Count - Position));
        }
    }
}