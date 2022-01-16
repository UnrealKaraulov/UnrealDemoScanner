using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

namespace compLexity_Demo_Player
{
    public class HalfLifeDelta
    {
        private class Entry
        {
            public String Name;
            public Object Value;
        }

        private List<Entry> entryList;

        public HalfLifeDelta(Int32 nEntries)
        {
            entryList = new List<Entry>(nEntries);
        }

        public void AddEntry(String name)
        {
            Entry e = new Entry();
            e.Name = name;
            e.Value = null;

            entryList.Add(e);
        }

        public Object FindEntryValue(String name)
        {
            Entry e = FindEntry(name);

            if (e == null)
            {
                return null;
            }

            return e.Value;
        }

        public void SetEntryValue(String name, Object value)
        {
            Entry e = FindEntry(name);

            if (e == null)
            {
                throw new ApplicationException(String.Format("Delta entry {0} not found.", name));
            }

            e.Value = value;
        }

        public void SetEntryValue(Int32 index, Object value)
        {
            entryList[index].Value = value;
        }

        private Entry FindEntry(String name)
        {
            foreach (Entry e in entryList)
            {
                if (e.Name == name)
                {
                    return e;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Stores delta structure entry parameters, as well as handling the creation and decoding of delta compressed data.
    /// </summary>
    public class HalfLifeDeltaStructure
    {
        public enum EntryFlags
        {
            Byte = (1 << 0),
            Short = (1 << 1),
            Float = (1 << 2),
            Integer = (1 << 3),
            Angle = (1 << 4),
            TimeWindow8 = (1 << 5),
            TimeWindowBig = (1 << 6),
            String = (1 << 7),
            Signed = (1 << 31)
        }

        public class Entry
        {
            public String Name;
            public UInt32 nBits;
            public Single Divisor;
            public EntryFlags Flags;
            public Single PreMultiplier;
        }

        private String name;
        private List<Entry> entryList;

        public String Name
        {
            get
            {
                return name;
            }
        }

        public HalfLifeDeltaStructure(String name)
        {
            this.name = name;
            entryList = new List<Entry>();
        }

        /// <summary>
        /// Adds an entry. Delta is assumed to be delta_description_t. Should only need to be called when parsing svc_deltadescription.
        /// </summary>
        /// <param name="delta"></param>
        public void AddEntry(HalfLifeDelta delta)
        {
            String name = (String)delta.FindEntryValue("name");
            UInt32 nBits = (UInt32)delta.FindEntryValue("nBits");
            Single divisor = (Single)delta.FindEntryValue("divisor");
            EntryFlags flags = (EntryFlags)((UInt32)delta.FindEntryValue("flags"));
            //Single preMultiplier = (Single)delta.FindEntryValue("preMultiplier");

            AddEntry(name, nBits, divisor, flags);
        }

        /// <summary>
        /// Adds an entry manually. Should only need to be called when creating a delta_description_t structure.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nBits"></param>
        /// <param name="divisor"></param>
        /// <param name="flags"></param>
        public void AddEntry(String name, UInt32 nBits, Single divisor, EntryFlags flags)
        {
            Entry entry = new Entry();

            entry.Name = name;
            entry.nBits = nBits;
            entry.Divisor = divisor;
            entry.Flags = flags;
            entry.PreMultiplier = 1.0f;

            entryList.Add(entry);
        }

        public HalfLifeDelta CreateDelta()
        {
            HalfLifeDelta delta = new HalfLifeDelta(entryList.Count);

            // create delta structure with the same entries as the delta decoder, but no data
            foreach (Entry e in entryList)
            {
                delta.AddEntry(e.Name);
            }

            return delta;
        }

        public void ReadDelta(BitBuffer bitBuffer, HalfLifeDelta delta)
        {
            Byte[] bitmaskBytes;

            ReadDelta(bitBuffer, delta, out bitmaskBytes);
        }

        public void ReadDelta(BitBuffer bitBuffer, HalfLifeDelta delta, out Byte[] bitmaskBytes)
        {
            // read bitmask
            UInt32 nBitmaskBytes = bitBuffer.ReadUnsignedBits(3);
            // TODO: error check nBitmaskBytes against nEntries

            if (nBitmaskBytes == 0)
            {
                bitmaskBytes = null;
                return;
            }

            bitmaskBytes = new Byte[nBitmaskBytes];

            for (Int32 i = 0; i < nBitmaskBytes; i++)
            {
                bitmaskBytes[i] = bitBuffer.ReadByte();
            }

            for (Int32 i = 0; i < nBitmaskBytes; i++)
            {
                for (Int32 j = 0; j < 8; j++)
                {
                    Int32 index = j + i * 8;

                    if (index == entryList.Count)
                    {
                        return;
                    }

                    if ((bitmaskBytes[i] & (1 << j)) != 0)
                    {
                        Object value = ParseEntry(bitBuffer, entryList[index]);

                        if (delta != null)
                        {
                            delta.SetEntryValue(index, value);
                        }
                    }
                }
            }
        }

        public Byte[] CreateDeltaBitmask(HalfLifeDelta delta)
        {
            UInt32 nBitmaskBytes = (UInt32)((entryList.Count / 8) + (entryList.Count % 8 > 0 ? 1 : 0));
            Byte[] bitmaskBytes = new Byte[nBitmaskBytes];

            for (Int32 i = 0; i < bitmaskBytes.Length; i++)
            {
                for (Int32 j = 0; j < 8; j++)
                {
                    Int32 index = j + i * 8;

                    if (index >= entryList.Count)
                    {
                        break;
                    }

                    if (delta.FindEntryValue(entryList[index].Name) != null)
                    {
                        bitmaskBytes[i] |= (Byte)(1 << j);
                    }
                }
            }

            return bitmaskBytes;
        }

        public void WriteDelta(BitWriter bitWriter, HalfLifeDelta delta, Byte[] bitmaskBytes)
        {
            if (bitmaskBytes == null) // no bitmask bytes
            {
                bitWriter.WriteUnsignedBits(0, 3);
                return;
            }

            bitWriter.WriteUnsignedBits((UInt32)bitmaskBytes.Length, 3);

            for (Int32 i = 0; i < bitmaskBytes.Length; i++)
            {
                bitWriter.WriteByte(bitmaskBytes[i]);
            }

            for (Int32 i = 0; i < bitmaskBytes.Length; i++)
            {
                for (Int32 j = 0; j < 8; j++)
                {
                    Int32 index = j + i * 8;

                    if (index == entryList.Count)
                    {
                        return;
                    }

                    if ((bitmaskBytes[i] & (1 << j)) != 0)
                    {
                        WriteEntry(delta, bitWriter, entryList[index]);
                    }
                }
            }
        }

        private Object ParseEntry(BitBuffer bitBuffer, Entry e)
        {
            Boolean signed = ((e.Flags & EntryFlags.Signed) != 0);

            if ((e.Flags & EntryFlags.Byte) != 0)
            {
                if (signed)
                {
                    return (SByte)ParseInt(bitBuffer, e);
                }
                else
                {
                    return (Byte)ParseUnsignedInt(bitBuffer, e);
                }
            }

            if ((e.Flags & EntryFlags.Short) != 0)
            {
                if (signed)
                {
                    return (Int16)ParseInt(bitBuffer, e);
                }
                else
                {
                    return (UInt16)ParseUnsignedInt(bitBuffer, e);
                }
            }

            if ((e.Flags & EntryFlags.Integer) != 0)
            {
                if (signed)
                {
                    return (Int32)ParseInt(bitBuffer, e);
                }
                else
                {
                    return (UInt32)ParseUnsignedInt(bitBuffer, e);
                }
            }

            if ((e.Flags & EntryFlags.Float) != 0 || (e.Flags & EntryFlags.TimeWindow8) != 0 || (e.Flags & EntryFlags.TimeWindowBig) != 0)
            {
                Boolean negative = false;
                Int32 bitsToRead = (Int32)e.nBits;

                if (signed)
                {
                    negative = bitBuffer.ReadBoolean();
                    bitsToRead--;
                }

                return (Single)bitBuffer.ReadUnsignedBits(bitsToRead) / e.Divisor * (negative ? -1.0f : 1.0f);
            }

            if ((e.Flags & EntryFlags.Angle) != 0)
            {
                return (Single)(bitBuffer.ReadUnsignedBits((Int32)e.nBits) * (360.0f / (Single)(1 << (Int32)e.nBits)));
            }

            if ((e.Flags & EntryFlags.String) != 0)
            {
                return bitBuffer.ReadString();
            }

            throw new ApplicationException(String.Format("Unknown delta entry type {0}.", e.Flags));
        }

        private Int32 ParseInt(BitBuffer bitBuffer, Entry e)
        {
            Boolean negative = bitBuffer.ReadBoolean();
            return (Int32)bitBuffer.ReadUnsignedBits((Int32)e.nBits - 1) / (Int32)e.Divisor * (negative ? -1 : 1);
        }

        private UInt32 ParseUnsignedInt(BitBuffer bitBuffer, Entry e)
        {
            return bitBuffer.ReadUnsignedBits((Int32)e.nBits) / (UInt32)e.Divisor;
        }

        private void WriteEntry(HalfLifeDelta delta, BitWriter bitWriter, Entry e)
        {
            Boolean signed = ((e.Flags & EntryFlags.Signed) != 0);
            Object value = delta.FindEntryValue(e.Name);

            if ((e.Flags & EntryFlags.Byte) != 0)
            {
                if (signed)
                {
                    SByte writeValue = (SByte)value;
                    WriteInt(bitWriter, e, (Int32)writeValue);
                }
                else
                {
                    Byte writeValue = (Byte)value;
                    WriteUnsignedInt(bitWriter, e, (UInt32)writeValue);
                }
            }
            else if ((e.Flags & EntryFlags.Short) != 0)
            {
                if (signed)
                {
                    Int16 writeValue = (Int16)value;
                    WriteInt(bitWriter, e, (Int32)writeValue);
                }
                else
                {
                    UInt16 writeValue = (UInt16)value;
                    WriteUnsignedInt(bitWriter, e, (UInt32)writeValue);
                }
            }
            else if ((e.Flags & EntryFlags.Integer) != 0)
            {
                if (signed)
                {
                    WriteInt(bitWriter, e, (Int32)value);
                }
                else
                {
                    WriteUnsignedInt(bitWriter, e, (UInt32)value);
                }
            }
            else if ((e.Flags & EntryFlags.Angle) != 0)
            {
                bitWriter.WriteUnsignedBits((UInt32)((Single)value / (360.0f / (Single)(1 << (Int32)e.nBits))), (Int32)e.nBits);
            }
            else if ((e.Flags & EntryFlags.String) != 0)
            {
                bitWriter.WriteString((String)value);
            }
            else if ((e.Flags & EntryFlags.Float) != 0 || (e.Flags & EntryFlags.TimeWindow8) != 0 || (e.Flags & EntryFlags.TimeWindowBig) != 0)
            {
                Single writeValue = (Single)value;
                Int32 bitsToWrite = (Int32)e.nBits;

                if (signed)
                {
                    bitWriter.WriteBoolean(writeValue < 0);
                    bitsToWrite--;
                }

                bitWriter.WriteUnsignedBits((UInt32)(Math.Abs(writeValue) * e.Divisor), bitsToWrite);
            }
            else
            {
                throw new ApplicationException(String.Format("Unknown delta entry type {0}.", e.Flags));
            }
        }

        private void WriteInt(BitWriter bitWriter, Entry e, Int32 value)
        {
            Int32 writeValue = value * (Int32)e.Divisor;

            bitWriter.WriteBoolean(writeValue < 0);
            bitWriter.WriteUnsignedBits((UInt32)Math.Abs(writeValue), (Int32)e.nBits - 1);
        }

        private void WriteUnsignedInt(BitWriter bitWriter, Entry e, UInt32 value)
        {
            UInt32 writeValue = value * (UInt32)e.Divisor;
            bitWriter.WriteUnsignedBits((UInt32)Math.Abs(writeValue), (Int32)e.nBits);
        }
    }
}
