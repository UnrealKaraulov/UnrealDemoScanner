using DemoScanner.DemoStuff.L4D2Branch.BitStreamUtil;
using DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo.DT;
using System;
using System.Text;

namespace DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo.DP.Handler
{
    internal static class PropDecoder
    {
        public static object DecodeProp(FlattenedPropEntry prop, IBitStream stream)
        {
            var sendProp = prop.Prop;
            switch (sendProp.Type)
            {
                case SendPropertyType.Int:
                    return DecodeInt(sendProp, stream);
                case SendPropertyType.Float:
                    return DecodeFloat(sendProp, stream);
                case SendPropertyType.Vector:
                    return DecodeVector(sendProp, stream);
                case SendPropertyType.Array:
                    var test = DecodeArray(prop, stream);
                    return test;
                case SendPropertyType.String:
                    return DecodeString(sendProp, stream);
                case SendPropertyType.VectorXY:
                    return DecodeVectorXY(sendProp, stream);
                default:
                    throw new NotImplementedException("Could not read property. Abort! ABORT!");
            }
        }

        public static int DecodeInt(SendTableProperty prop, IBitStream reader)
        {
            if (prop.Flags.HasFlagFast(SendPropertyFlags.VarInt))
            {
                if (prop.Flags.HasFlagFast(SendPropertyFlags.Unsigned)) return (int)reader.ReadVarInt();
                return (int)reader.ReadSignedVarInt();
            }

            if (prop.Flags.HasFlagFast(SendPropertyFlags.Unsigned)) return (int)reader.ReadInt(prop.NumberOfBits);
            return reader.ReadSignedInt(prop.NumberOfBits);
        }

        public static float DecodeFloat(SendTableProperty prop, IBitStream reader)
        {
            ulong dwInterp;

            if (DecodeSpecialFloat(prop, reader, out var fVal)) return fVal;


            //Encoding: The range between lowVal and highVal is splitted into the same steps.
            //Read an int, fit it into the range. 
            dwInterp = reader.ReadInt(prop.NumberOfBits);
            fVal = (float)dwInterp / ((1 << prop.NumberOfBits) - 1);
            fVal = prop.LowValue + (prop.HighValue - prop.LowValue) * fVal;

            return fVal;
        }

        public static Vector DecodeVector(SendTableProperty prop, IBitStream reader)
        {
            if (prop.Flags.HasFlagFast(SendPropertyFlags.Normal))
            {
            }

            var v = new Vector
            {
                X = DecodeFloat(prop, reader),
                Y = DecodeFloat(prop, reader)
            };

            if (!prop.Flags.HasFlagFast(SendPropertyFlags.Normal))
            {
                v.Z = DecodeFloat(prop, reader);
            }
            else
            {
                var isNegative = reader.ReadBit();

                //v0v0v1v1 in original instead of margin. 
                var absolute = v.X * v.X + v.Y * v.Y;
                if (absolute < 1.0f)
                    v.Z = (float)Math.Sqrt(1 - absolute);
                else
                    v.Z = 0f;

                if (isNegative) v.Z *= -1;
            }

            return v;
        }

        public static object[] DecodeArray(FlattenedPropEntry flattenedProp, IBitStream reader)
        {
            var numElements = flattenedProp.Prop.NumberOfElements;
            var maxElements = numElements;

            var numBits = 1;

            while ((maxElements >>= 1) != 0) numBits++;

            var nElements = (int)reader.ReadInt(numBits);

            var result = new object[nElements];

            var temp = new FlattenedPropEntry("", flattenedProp.ArrayElementProp, null);
            for (var i = 0; i < nElements; i++) result[i] = DecodeProp(temp, reader);

            return result;
        }

        public static string DecodeString(SendTableProperty prop, IBitStream reader)
        {
            return Encoding.Default.GetString(reader.ReadBytes((int)reader.ReadInt(9)));
        }

        public static Vector DecodeVectorXY(SendTableProperty prop, IBitStream reader)
        {
            var v = new Vector
            {
                X = DecodeFloat(prop, reader),
                Y = DecodeFloat(prop, reader)
            };

            return v;
        }

        #region Float-Stuff

        private static bool DecodeSpecialFloat(SendTableProperty prop, IBitStream reader, out float result)
        {
            if (prop.Flags.HasFlagFast(SendPropertyFlags.Coord))
            {
                result = ReadBitCoord(reader);
                return true;
            }

            if (prop.Flags.HasFlagFast(SendPropertyFlags.CoordMp))
            {
                result = ReadBitCoordMP(reader, false, false);
                return true;
            }

            if (prop.Flags.HasFlagFast(SendPropertyFlags.CoordMpLowPrecision))
            {
                result = ReadBitCoordMP(reader, false, true);
                return true;
            }

            if (prop.Flags.HasFlagFast(SendPropertyFlags.CoordMpIntegral))
            {
                result = ReadBitCoordMP(reader, true, false);
                return true;
            }

            if (prop.Flags.HasFlagFast(SendPropertyFlags.NoScale))
            {
                result = reader.ReadFloat();
                return true;
            }

            if (prop.Flags.HasFlagFast(SendPropertyFlags.Normal))
            {
                result = ReadBitNormal(reader);
                return true;
            }

            if (prop.Flags.HasFlagFast(SendPropertyFlags.CellCoord))
            {
                result = ReadBitCellCoord(reader, prop.NumberOfBits, false, false);
                return true;
            }

            if (prop.Flags.HasFlagFast(SendPropertyFlags.CellCoordLowPrecision))
            {
                result = ReadBitCellCoord(reader, prop.NumberOfBits, true, false);
                return true;
            }

            if (prop.Flags.HasFlagFast(SendPropertyFlags.CellCoordIntegral))
            {
                result = ReadBitCellCoord(reader, prop.NumberOfBits, false, true);
                return true;
            }

            result = 0;

            return false;
        }

        private static readonly int COORD_FRACTIONAL_BITS = 5;
        private static readonly int COORD_DENOMINATOR = 1 << COORD_FRACTIONAL_BITS;
        private static readonly float COORD_RESOLUTION = 1.0f / COORD_DENOMINATOR;

        private static readonly int COORD_FRACTIONAL_BITS_MP_LOWPRECISION = 3;
        private static readonly float COORD_DENOMINATOR_LOWPRECISION = 1 << COORD_FRACTIONAL_BITS_MP_LOWPRECISION;
        private static readonly float COORD_RESOLUTION_LOWPRECISION = 1.0f / COORD_DENOMINATOR_LOWPRECISION;

        private static float ReadBitCoord(IBitStream reader)
        {
            int intVal, fractVal;
            float value = 0;

            var isNegative = false;

            // Read the required integer and fraction flags
            intVal = (int)reader.ReadInt(1);
            fractVal = (int)reader.ReadInt(1);

            // If we got either parse them, otherwise it's a zero.
            if ((intVal | fractVal) != 0)
            {
                // Read the sign bit
                isNegative = reader.ReadBit();

                // If there's an integer, read it in
                if (intVal == 1)
                    // Adjust the integers from [0..MAX_COORD_VALUE-1] to [1..MAX_COORD_VALUE]
                    intVal = (int)reader.ReadInt(14) + 1; //14 --> Coord int bits

                //If there's a fraction, read it in
                if (fractVal == 1) fractVal = (int)reader.ReadInt(COORD_FRACTIONAL_BITS);

                value = intVal + fractVal * COORD_RESOLUTION;
            }

            if (isNegative) value *= -1;

            return value;
        }

        private static float ReadBitCoordMP(IBitStream reader, bool isIntegral, bool isLowPrecision)
        {
            int intval = 0, fractval = 0;
            var value = 0.0f;
            var isNegative = false;

            var inBounds = reader.ReadBit();

            if (isIntegral)
            {
                // Read the required integer and fraction flags
                intval = reader.ReadBit() ? 1 : 0;

                // If we got either parse them, otherwise it's a zero.
                if (intval == 1)
                {
                    // Read the sign bit
                    isNegative = reader.ReadBit();

                    // If there's an integer, read it in
                    // Adjust the integers from [0..MAX_COORD_VALUE-1] to [1..MAX_COORD_VALUE]
                    if (inBounds)
                        value = reader.ReadInt(11) + 1;
                    else
                        value = reader.ReadInt(14) + 1;
                }
            }
            else
            {
                // Read the required integer and fraction flags
                intval = reader.ReadBit() ? 1 : 0;

                // Read the sign bit
                isNegative = reader.ReadBit();

                // If we got either parse them, otherwise it's a zero.
                if (intval == 1)
                {
                    // If there's an integer, read it in
                    // Adjust the integers from [0..MAX_COORD_VALUE-1] to [1..MAX_COORD_VALUE]
                    if (inBounds)
                        value = reader.ReadInt(11) + 1;
                    else
                        value = reader.ReadInt(14) + 1;
                }

                // If there's a fraction, read it in
                fractval = (int)reader.ReadInt(isLowPrecision ? 3 : 5);

                // Calculate the correct floating point value
                value = intval + fractval * (isLowPrecision ? COORD_RESOLUTION_LOWPRECISION : COORD_RESOLUTION);
            }

            if (isNegative) value = -value;

            return value;
        }

        private static float ReadBitCellCoord(IBitStream reader, int bits, bool lowPrecision, bool integral)
        {
            int intval = 0, fractval = 0;
            var value = 0.0f;

            if (integral)
            {
                value = reader.ReadInt(bits);
            }
            else
            {
                intval = (int)reader.ReadInt(bits);
                fractval =
                    (int)reader.ReadInt(lowPrecision ? COORD_FRACTIONAL_BITS_MP_LOWPRECISION : COORD_FRACTIONAL_BITS);


                value = intval + fractval * (lowPrecision ? COORD_RESOLUTION_LOWPRECISION : COORD_RESOLUTION);
            }

            return value;
        }

        private static readonly int NORMAL_FRACTIONAL_BITS = 11;
        private static readonly int NORMAL_DENOMINATOR = (1 << NORMAL_FRACTIONAL_BITS) - 1;
        private static readonly float NORMAL_RESOLUTION = 1.0f / NORMAL_DENOMINATOR;

        private static float ReadBitNormal(IBitStream reader)
        {
            var isNegative = reader.ReadBit();

            var fractVal = reader.ReadInt(NORMAL_FRACTIONAL_BITS);

            var value = fractVal * NORMAL_RESOLUTION;

            if (isNegative) value *= -1;

            return value;
        }

        #endregion
    }
}