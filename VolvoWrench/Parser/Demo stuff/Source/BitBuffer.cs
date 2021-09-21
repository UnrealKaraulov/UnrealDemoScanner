using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DemoScanner.DemoStuff.Source
{
    [Serializable]
    public class BitBufferOutOfRangeException : Exception
    {
    }

    public class BitBuffer
    {
        public enum EndianType
        {
            Little,
            Big
        }

        private readonly List<byte> _data;

        public BitBuffer(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), @"Value cannot be null.");
            }

            _data = new List<byte>(data);
            Endian = EndianType.Little;
        }

        public void SeekBits(int count)
        {
            SeekBits(count, SeekOrigin.Current);
        }

        public void SeekBits(int offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Current:
                    CurrentBit += offset;
                    break;
                case SeekOrigin.Begin:
                    CurrentBit = offset;
                    break;
                case SeekOrigin.End:
                    CurrentBit = (_data.Count*8) - offset;
                    break;
            }

            if (CurrentBit < 0 || CurrentBit > _data.Count*8)
            {
                throw new BitBufferOutOfRangeException();
            }
        }

        public void SeekBytes(int count)
        {
            SeekBits(count * 8);
        }

        public void SeekBytes(int offset, SeekOrigin origin)
        {
            SeekBits(offset * 8, origin);
        }

        /// <summary>
        ///     Seeks past the remaining bits in the current byte.
        /// </summary>
        public void SkipRemainingBits()
        {
            var bitOffset = CurrentBit % 8;

            if (bitOffset != 0)
            {
                SeekBits(8 - bitOffset);
            }
        }

        // HL 1.1.0.6 bit reading (big endian byte and bit order)
        private uint ReadUnsignedBitsBigEndian(int nBits)
        {
            if (nBits <= 0 || nBits > 32)
            {
                throw new ArgumentException(@"Value must be a positive integer between 1 and 32 inclusive.", nameof(nBits));
            }

            // check for overflow
            if (CurrentBit + nBits > _data.Count*8)
            {
                throw new BitBufferOutOfRangeException();
            }

            var currentByte = CurrentBit / 8;
            var bitOffset = CurrentBit - (currentByte*8);
            var nBytesToRead = (bitOffset + nBits) / 8;

            if ((bitOffset + nBits)%8 != 0)
            {
                nBytesToRead++;
            }

            // get bytes we need
            ulong currentValue = 0;
            for (var i = 0; i < nBytesToRead; i++)
            {
                var b = _data[currentByte + (nBytesToRead - 1) - i];
                currentValue += (ulong) b << (i * 8);
            }

            // get bits we need from bytes
            currentValue >>= ((nBytesToRead*8 - bitOffset) - nBits);
            currentValue &= (uint) (((long) 1 << nBits) - 1);

            // increment current bit
            CurrentBit += nBits;

            return (uint) currentValue;
        }

        private uint ReadUnsignedBitsLittleEndian(int nBits)
        {
            nBits = Math.Abs(nBits);
            if (nBits <= 0 || nBits > 32)
            {
                throw new ArgumentException(@"Value must be a positive integer between 1 and 32 inclusive.", nameof(nBits));
            }

            // check for overflow
            if (CurrentBit + nBits > _data.Count*8)
            {
                throw new BitBufferOutOfRangeException();
            }

            var currentByte = CurrentBit / 8;
            var bitOffset = CurrentBit - (currentByte*8);
            var nBytesToRead = (bitOffset + nBits) / 8;

            if ((bitOffset + nBits)%8 != 0)
            {
                nBytesToRead++;
            }

            // get bytes we need
            ulong currentValue = 0;
            for (var i = 0; i < nBytesToRead; i++)
            {
                var b = _data[currentByte + i];
                currentValue += (ulong) b << (i * 8);
            }

            // get bits we need from bytes
            currentValue >>= bitOffset;
            currentValue &= (uint) (((long) 1 << nBits) - 1);

            // increment current bit
            CurrentBit += nBits;

            return (uint) currentValue;
        }

        public uint ReadUnsignedBits(int nBits)
        {
            if (Endian == EndianType.Little)
            {
                return ReadUnsignedBitsLittleEndian(nBits);
            }
            return ReadUnsignedBitsBigEndian(nBits);
        }

        public int ReadBits(int nBits)
        {
            var result = (int) ReadUnsignedBits(nBits - 1);
            var sign = (ReadBoolean() ? 1 : 0);

            if (sign == 1)
            {
                result = -((1 << (nBits - 1)) - result);
            }

            return result;
        }

        public bool ReadBoolean()
        {
            // check for overflow
            if (CurrentBit + 1 > _data.Count*8)
            {
                throw new BitBufferOutOfRangeException();
            }

            var result = (_data[CurrentBit/8] & ((Endian == EndianType.Little ? 1 << CurrentBit%8 : 128 >> CurrentBit%8))) != 0;
            CurrentBit++;
            return result;
        }

        public byte ReadByte()
        {
            return (byte) ReadUnsignedBits(8);
        }

        public sbyte ReadSByte()
        {
            return (sbyte) ReadBits(8);
        }

        public byte[] ReadBytes(int nBytes)
        {
            var result = new byte[nBytes];

            for (var i = 0; i < nBytes; i++)
            {
                result[i] = ReadByte();
            }

            return result;
        }

        public char[] ReadChars(int nChars)
        {
            var result = new char[nChars];

            for (var i = 0; i < nChars; i++)
            {
                result[i] = (char) ReadByte(); // not unicode
            }

            return result;
        }

        public short ReadInt16()
        {
            return (short) ReadBits(16);
        }

        public ushort ReadUInt16()
        {
            return (ushort) ReadUnsignedBits(16);
        }

        public int ReadInt32()
        {
            return ReadBits(32);
        }

        public uint ReadUInt32()
        {
            return ReadUnsignedBits(32);
        }

        public float ReadSingle()
        {
            return BitConverter.ToSingle(ReadBytes(4), 0);
        }

        /// <summary>
        ///     Read a null-terminated string, then skip any remaining bytes to make up length bytes.
        /// </summary>
        /// <param name="length">The total number of bytes to read.</param>
        /// <returns></returns>
        public string ReadString(int length)
        {
            var startBit = CurrentBit;
            var s = ReadString();
            SeekBits(length * 8 - (CurrentBit - startBit));
            return s;
        }

        public string ReadString()
        {
            var bytes = new List<byte>();

            while (true)
            {
                var b = ReadByte();

                if (b == 0x00)
                {
                    break;
                }

                bytes.Add(b);
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public float[] ReadVectorCoord()
        {
            return ReadVectorCoord(false);
        }

        public float[] ReadVectorCoord(bool goldSrc)
        {
            var xFlag = ReadBoolean();
            var yFlag = ReadBoolean();
            var zFlag = ReadBoolean();

            var result = new float[3];

            if (xFlag)
            {
                result[0] = ReadCoord(goldSrc);
            }

            if (yFlag)
            {
                result[1] = ReadCoord(goldSrc);
            }

            if (zFlag)
            {
                result[2] = ReadCoord(goldSrc);
            }

            return result;
        }

        public float ReadCoord()
        {
            return ReadCoord(false);
        }

        public float ReadCoord(bool goldSrc)
        {
            var intFlag = ReadBoolean();
            var fractionFlag = ReadBoolean();

            var value = 0.0f;

            if (!intFlag && !fractionFlag)
            {
                return value;
            }

            var sign = ReadBoolean();
            uint intValue = 0;
            uint fractionValue = 0;

            if (intFlag)
            {
                if (goldSrc)
                {
                    intValue = ReadUnsignedBits(12);
                }
                else
                {
                    intValue = ReadUnsignedBits(14) + 1;
            }
            }

            if (fractionFlag)
            {
                fractionValue = ReadUnsignedBits(goldSrc ? 3 : 5);
            }

            value = intValue + (fractionValue*1.0f/32.0f);

            if (sign)
            {
                value = -value;
            }

            return value;
        }

        /// <summary>
        ///     Sets all bits to zero, starting with the current bit and up to nBits.
        ///     Used for Fade to Black removal.
        /// </summary>
        /// <param name="nBits"></param>
        public void ZeroOutBits(int nBits)
        {
            for (var i = 0; i < nBits; i++)
            {
                var currentByte = CurrentBit / 8;
                var bitOffset = CurrentBit - (currentByte*8);

                var temp = _data[currentByte];
                temp -= (byte) (_data[currentByte] & (1 << bitOffset));
                _data[currentByte] = temp;

                CurrentBit++;
            }
        }

        public string PrintBits(int nBits)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < nBits; i++)
            {
                sb.AppendFormat("{0}", (ReadBoolean() ? 1 : 0));
            }

            return (sb + "\n");
        }

        public void InsertBytes(byte[] insertData)
        {
            if (insertData.Length == 0)
            {
                return;
            }

            if (CurrentBit % 8 != 0)
            {
                throw new ApplicationException(
                    "InsertBytes can only be called if the current bit is aligned to byte boundaries.");
            }

            _data.InsertRange(CurrentByte, insertData);
            CurrentBit += insertData.Length * 8;
        }

        public void RemoveBytes(int count)
        {
            if (count == 0)
            {
                return;
            }

            if (CurrentBit % 8 != 0)
            {
                throw new ApplicationException(
                    "RemoveBytes can only be called if the current bit is aligned to byte boundaries.");
            }

            if (CurrentByte + count > Length)
            {
                throw new BitBufferOutOfRangeException();
            }

            _data.RemoveRange(CurrentByte, count);
        }

        #region Properties

        /// <summary>
        ///     Data length in bytes.
        /// </summary>
        public int Length => _data.Count;

        /// <summary>
        ///     The current bit we are on
        /// </summary>
        public int CurrentBit { get; private set; }

        /// <summary>
        ///     The byte we are reading the bits of currently
        /// </summary>
        public int CurrentByte => (CurrentBit - (CurrentBit%8))/8;

        /// <summary>
        ///     Bits left from the buffer
        /// </summary>
        public int BitsLeft => (_data.Count*8) - CurrentBit;

        /// <summary>
        ///     Bytes left from the buffer
        /// </summary>
        public int BytesLeft => _data.Count - CurrentByte;

        /// <summary>
        ///     The data of the buffer
        /// </summary>
        public byte[] Data => _data.ToArray();

        /// <summary>
        ///     Byte sorting
        /// </summary>
        public EndianType Endian { get; set; }

        #endregion
    }

    /// <summary>
    ///     Methods to write bits to the buffer
    /// </summary>
    public class BitWriter
    {
        private readonly List<byte> _data;
        private int _currentBit;

        /// <summary>
        ///     Constructor of the bitwriter it initializes an empty buffer
        /// </summary>
        public BitWriter()
        {
            _data = new List<byte>();
        }

        /// <summary>
        ///     The data in buffer
        /// </summary>
        public byte[] Data => _data.ToArray();

        public void WriteUnsignedBits(uint value, int nBits)
        {
            var currentByte = _currentBit / 8;
            var bitOffset = _currentBit - (currentByte*8);

            // calculate how many bits need to be written to the current byte
            var bitsToWriteToCurrentByte = 8 - bitOffset;
            if (bitsToWriteToCurrentByte > nBits)
            {
                bitsToWriteToCurrentByte = nBits;
            }

            // calculate how many bytes need to be added to the list
            var bytesToAdd = 0;

            if (nBits > bitsToWriteToCurrentByte)
            {
                var temp = nBits - bitsToWriteToCurrentByte;
                bytesToAdd = temp / 8;

                if ((temp%8) != 0)
                {
                    bytesToAdd++;
                }
            }

            if (bitOffset == 0)
            {
                bytesToAdd++;
            }

            // add new bytes if needed
            for (var i = 0; i < bytesToAdd; i++)
            {
                _data.Add(new byte());
            }

            var nBitsWritten = 0;

            // write bits to the current byte
            var b = (byte) (value & ((1 << bitsToWriteToCurrentByte) - 1));
            b <<= bitOffset;
            b += _data[currentByte];
            _data[currentByte] = b;

            nBitsWritten += bitsToWriteToCurrentByte;
            currentByte++;

            // write bits to all the newly added bytes
            while (nBitsWritten < nBits)
            {
                bitsToWriteToCurrentByte = nBits - nBitsWritten;
                if (bitsToWriteToCurrentByte > 8)
                {
                    bitsToWriteToCurrentByte = 8;
                }

                b = (byte) ((value >> nBitsWritten) & ((1 << bitsToWriteToCurrentByte) - 1));
                _data[currentByte] = b;

                nBitsWritten += bitsToWriteToCurrentByte;
                currentByte++;
            }

            // set new current bit
            _currentBit += nBits;
        }

        public void WriteBits(int value, int nBits)
        {
            WriteUnsignedBits((uint) value, nBits - 1);

            var sign = (value < 0 ? 1u : 0u);
            WriteUnsignedBits(sign, 1);
        }

        public void WriteBoolean(bool value)
        {
            var currentByte = _currentBit / 8;

            if (currentByte > _data.Count - 1)
            {
                _data.Add(new byte());
            }

            if (value)
            {
                _data[currentByte] += (byte) (1 << _currentBit%8);
            }

            _currentBit++;
        }

        public void WriteByte(byte value)
        {
            WriteUnsignedBits(value, 8);
        }

        public void WriteSByte(sbyte value)
        {
            WriteBits(value, 8);
        }

        public void WriteBytes(byte[] values)
        {
            foreach (byte t in values)
            {
                WriteByte(t);
            }
        }

        public void WriteChars(char[] values)
        {
            foreach (char t in values)
            {
                WriteByte((byte) t);
            }
        }

        public void WriteInt16(short value)
        {
            WriteBits(value, 16);
        }

        public void WriteUInt16(ushort value)
        {
            WriteUnsignedBits(value, 16);
        }

        public void WriteInt32(int value)
        {
            WriteBits(value, 32);
        }

        public void WriteUInt32(uint value)
        {
            WriteUnsignedBits(value, 32);
        }

        public void WriteString(string value)
        {
            foreach (char t in value)
            {
                WriteByte((byte) t);
            }

            // null terminator
            WriteByte(0);
        }

        public void WriteString(string value, int length)
        {
            if (length < value.Length + 1)
            {
                throw new ApplicationException("String length longer than specified length.");
            }

            WriteString(value);

            // write padding 0's
            for (var i = 0; i < length - (value.Length + 1); i++)
            {
                WriteByte(0);
            }
        }

        public void WriteVectorCoord(bool goldSrc, float[] coord)
        {
            WriteBoolean(true);
            WriteBoolean(true);
            WriteBoolean(true);
            WriteCoord(goldSrc, coord[0]);
            WriteCoord(goldSrc, coord[1]);
            WriteCoord(goldSrc, coord[2]);
        }

        public void WriteCoord(bool goldSrc, float value)
        {
            WriteBoolean(true); // int flag
            WriteBoolean(true); // fraction flag

            // sign
            WriteBoolean(value < 0.0f);

            var intValue = (uint) value;

            if (goldSrc)
            {
                WriteUnsignedBits(intValue, 12);
                WriteUnsignedBits(0, 3); // Todo: fix
            }
            else
            {
                WriteUnsignedBits(intValue - 1, 14);
                WriteUnsignedBits(0, 5); // Todo: fix
            }
        }
    }
}