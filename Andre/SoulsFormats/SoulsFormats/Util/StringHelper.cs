using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoulsFormats
{
    internal static class StringHelper
    {
        internal static string ToBinary(this byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }

        internal static byte[] FromBinary(this string binary, int bitCount)
        {
            if (string.IsNullOrWhiteSpace(binary))
            {
                return Array.Empty<byte>();
            }

            var strs = binary.SplitBy(bitCount);
            var values = new List<byte>();
            foreach (string str in strs)
            {
                values.Add(Convert.ToByte(str, 2));
            }
            return values.ToArray();
        }

        internal static ushort[] FromBinaryToUInt16(this string binary, int bitCount)
        {
            if (string.IsNullOrWhiteSpace(binary))
            {
                return Array.Empty<ushort>();
            }

            var strs = binary.SplitBy(bitCount);
            var values = new List<ushort>();
            foreach (string str in strs)
            {
                values.Add(Convert.ToUInt16(str, 2));
            }
            return values.ToArray();
        }

        internal static IEnumerable<string> SplitBy(this string str, int chunkSize)
        {
            int remainder = str.Length % chunkSize;
            if (remainder != 0)
            {
                str = str.Substring(0, str.Length - remainder);
            }

            return Enumerable.Range(0, str.Length / chunkSize).Select(i => str.Substring(i * chunkSize, chunkSize));
        }
    }
}
