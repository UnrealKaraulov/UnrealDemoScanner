﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace VolvoWrench.DemoStuff.L4D2Branch.BitStreamUtil
{
    public class DebugBitStream : IBitStream
    {
        private readonly IBitStream A, B;

        public DebugBitStream(IBitStream a, IBitStream b)
        {
            A = a;
            B = b;
        }

        public void Initialize(Stream stream)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            A.Dispose();
            B.Dispose();
        }

        public uint ReadInt(int bits)
        {
            var a = A.ReadInt(bits);
            var b = B.ReadInt(bits);
            Verify(a, b);
            return a;
        }

        public int ReadSignedInt(int bits)
        {
            var a = A.ReadSignedInt(bits);
            var b = B.ReadSignedInt(bits);
            Verify(a, b);
            return a;
        }

        public bool ReadBit()
        {
            var a = A.ReadBit();
            var b = B.ReadBit();
            Verify(a, b);
            return a;
        }

        public byte ReadByte()
        {
            var a = A.ReadByte();
            var b = B.ReadByte();
            Verify(a, b);
            return a;
        }

        public byte ReadByte(int bits)
        {
            var a = A.ReadByte(bits);
            var b = B.ReadByte(bits);
            Verify(a, b);
            return a;
        }

        public byte[] ReadBytes(int bytes)
        {
            var a = A.ReadBytes(bytes);
            var b = B.ReadBytes(bytes);
            Verify(a.SequenceEqual(b), true);
            return a;
        }

        public float ReadFloat()
        {
            var a = A.ReadFloat();
            var b = B.ReadFloat();
            Verify(a, b);
            return a;
        }

        public byte[] ReadBits(int bits)
        {
            var a = A.ReadBits(bits);
            var b = B.ReadBits(bits);
            Verify(a.SequenceEqual(b), true);
            return a;
        }

        public int ReadProtobufVarInt()
        {
            var a = A.ReadProtobufVarInt();
            var b = B.ReadProtobufVarInt();
            Verify(a, b);
            return a;
        }

        public void BeginChunk(int bits)
        {
            A.BeginChunk(bits);
            B.BeginChunk(bits);
        }

        public void EndChunk()
        {
            A.EndChunk();
            B.EndChunk();
        }

        public bool ChunkFinished
        {
            get
            {
                var a = A.ChunkFinished;
                var b = B.ChunkFinished;
                Verify(a, b);
                return a;
            }
        }

        private void Verify<T>(T a, T b)
        {
            if (!a.Equals(b))
            {
                Debug.Assert(false);
                throw new InvalidOperationException(string.Format("{0} vs {1} ({2} vs {3})",
                    a, b, A.GetType().Name, B.GetType().Name));
            }
        }

        public string ReadString()
        {
            var a = A.ReadString();
            var b = B.ReadString();
            Verify(a, b);
            return a;
        }

        public string ReadString(int size)
        {
            var a = A.ReadString(size);
            var b = B.ReadString(size);
            Verify(a, b);
            return a;
        }

        public uint ReadVarInt()
        {
            var a = A.ReadVarInt();
            var b = B.ReadVarInt();
            Verify(a, b);
            return a;
        }

        public uint ReadUBitInt()
        {
            var a = A.ReadUBitInt();
            var b = B.ReadUBitInt();
            Verify(a, b);
            return a;
        }
    }
}