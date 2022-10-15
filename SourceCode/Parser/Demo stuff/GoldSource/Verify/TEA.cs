using System;
using System.Linq;

namespace DemoScanner.DemoStuff.GoldSource.Verify
{
    /// <summary>
    ///     Tea encryption class
    /// </summary>
    public class Tea
    {
        /// <summary>
        ///     Key used by bxt for TEA encryption.
        /// </summary>
        public static uint[] BxtKey = { 0x1337FACE, 0x12345678, 0xDEADBEEF, 0xFEEDABCD };

        /// <summary>
        ///     Decrypts an array of Uin32s encrypted with BXT's TEA key.
        /// </summary>
        /// <param name="data">The data to decrypt</param>
        public static uint[] Decrypt(byte[] data)
        {
            if (data.Length != 8) throw new Exception("Invalid data! (Invalid number of bytes supplied)");

            uint v0 = BitConverter.ToUInt32(data.Take(4).ToArray(), 0),
                v1 = BitConverter.ToUInt32(data.Skip(4).ToArray(), 0),
                sum = 0xC6EF3720;
            for (var i = 0; i < 32; i++)
            {
                v1 -= ((v0 << 4) + BxtKey[2]) ^ (v0 + sum) ^ ((v0 >> 5) + BxtKey[3]);
                v0 -= ((v1 << 4) + BxtKey[0]) ^ (v1 + sum) ^ ((v1 >> 5) + BxtKey[1]);
                sum -= 0x9e3779b9;
            }

            return new[] { v0, v1 };
        }

        /// <summary>
        ///     Trims null bytes from the end of a byte array.
        /// </summary>
        /// <param name="Bytes">The bytes to trail the null bytes from.</param>
        /// <returns>The trimmed array.</returns>
        public static byte[] TrimBytes(byte[] Bytes)
        {
            var i = Bytes.Length - 1;
            while (Bytes[i] == 0) --i;
            var temp = new byte[i + 1];
            Array.Copy(Bytes, temp, i + 1);
            return temp;
        }
    }
}