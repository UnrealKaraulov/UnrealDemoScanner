using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DemoScanner.DemoStuff
{
    internal static class BinaryReaderExtension
    {
        public static string ReadCString(this BinaryReader reader, int length)
        {
            return ReadCString(reader, length, Encoding.UTF8);
        }

        public static int ReadInt32SwapEndian(this BinaryReader reader)
        {
            return BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
        }

        public static long ReadInt64SwapEndian(this BinaryReader reader)
        {
            return BitConverter.ToInt64(reader.ReadBytes(8).Reverse().ToArray(), 0);
        }

        public static string Reverse(this string s)
        {
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string ReadCString(this BinaryReader reader, int length, Encoding encoding)
        {
            return encoding.GetString(reader.ReadBytes(length)).Split(new[] { '\0' }, 2)[0];
        }

        public static int ReadVarInt32(this BinaryReader reader)
        {
            int b = 0, count = 0, result = 0;

            do
            {
                if (count > 5) throw new InvalidDataException("VarInt32 out of range");

                b = reader.ReadByte();

                result |= (b & 0x7F) << (7 * count);

                count++;
            } while ((b & 0x80) != 0);

            return result;
        }

        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            return ReadNullTerminatedString(reader, Encoding.Default);
        }

        public static string ReadNullTerminatedString(this BinaryReader reader, Encoding encoding)
        {
            return ReadNullTerminatedString(reader, encoding, 512);
        }


        public static string ReadNullTerminatedString(this BinaryReader reader, Encoding encoding,
            int initialBufferSize)
        {
            var result = new List<byte>(initialBufferSize);

            while (true)
            {
                var b = reader.ReadByte();

                if (b == 0) break;

                result.Add(b);
            }

            return Encoding.Default.GetString(result.ToArray());
        }


    }
    public static class Crc32
    {
        private static readonly uint[] Table;

        static Crc32()
        {
            uint poly = 0xedb88320;
            Table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint r = i;
                for (int j = 0; j < 8; j++)
                    r = (r & 1) != 0 ? (r >> 1) ^ poly : r >> 1;
                Table[i] = r;
            }
        }

        public static uint Compute(byte[] bytes)
        {
            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; i++)
            {
                crc = (crc >> 8) ^ Table[(crc ^ bytes[i]) & 0xff];
            }
            return crc ^ 0xffffffff;
        }
    }
}