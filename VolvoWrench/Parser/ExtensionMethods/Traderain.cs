using System.IO;
using System.Text;

namespace VolvoWrench.ExtensionMethods
{
    internal static class Traderain
    {
        public static string ReadString(this BinaryReader br, int length)
        {
            return Encoding.ASCII.GetString(br.ReadBytes(length))
                .Trim('\0')
                .Replace("\0", string.Empty);
            ;
        }
    }
}