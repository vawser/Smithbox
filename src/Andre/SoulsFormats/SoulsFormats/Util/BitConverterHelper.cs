
using System;

namespace SoulsFormats
{
    internal static class BitConverterHelper
    {
        internal static short ToInt16BigEndian(byte[] bytes, int offset)
        {
            if (offset + 2 > bytes.Length)
            {
                throw new ArgumentOutOfRangeException($"{nameof(offset)} went of out range.", nameof(offset));
            }

            byte[] readBytes = new byte[2];
            readBytes[0] = bytes[offset];
            readBytes[1] = bytes[offset + 1];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(readBytes);
            }

            return BitConverter.ToInt16(readBytes, 0);
        }

        internal static ushort ToUInt16BigEndian(byte[] bytes, int offset)
        {
            if (offset + 2 > bytes.Length)
            {
                throw new ArgumentOutOfRangeException($"{nameof(offset)} went of out range.", nameof(offset));
            }

            byte[] readBytes = new byte[2];
            readBytes[0] = bytes[offset];
            readBytes[1] = bytes[offset + 1];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(readBytes);
            }

            return BitConverter.ToUInt16(readBytes, 0);
        }

        internal static int ToInt32BigEndian(byte[] bytes, int offset)
        {
            if (offset + 4 > bytes.Length)
            {
                throw new ArgumentOutOfRangeException($"{nameof(offset)} went of out range.", nameof(offset));
            }

            byte[] readBytes = new byte[4];
            readBytes[0] = bytes[offset];
            readBytes[1] = bytes[offset + 1];
            readBytes[2] = bytes[offset + 2];
            readBytes[3] = bytes[offset + 3];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(readBytes);
            }

            return BitConverter.ToInt32(readBytes, 0);
        }

        internal static uint ToUInt32BigEndian(byte[] bytes, int offset)
        {
            if (offset + 4 > bytes.Length)
            {
                throw new ArgumentOutOfRangeException($"{nameof(offset)} went of out range.", nameof(offset));
            }

            byte[] readBytes = new byte[4];
            readBytes[0] = bytes[offset];
            readBytes[1] = bytes[offset + 1];
            readBytes[2] = bytes[offset + 2];
            readBytes[3] = bytes[offset + 3];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(readBytes);
            }

            return BitConverter.ToUInt32(readBytes, 0);
        }

        internal static long ToInt64BigEndian(byte[] bytes, int offset)
        {
            if (offset + 8 > bytes.Length)
            {
                throw new ArgumentOutOfRangeException($"{nameof(offset)} went of out range.", nameof(offset));
            }

            byte[] readBytes = new byte[8];
            readBytes[0] = bytes[offset];
            readBytes[1] = bytes[offset + 1];
            readBytes[2] = bytes[offset + 2];
            readBytes[3] = bytes[offset + 3];
            readBytes[4] = bytes[offset + 4];
            readBytes[5] = bytes[offset + 5];
            readBytes[6] = bytes[offset + 6];
            readBytes[7] = bytes[offset + 7];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(readBytes);
            }

            return BitConverter.ToInt64(readBytes, 0);
        }

        internal static ulong ToUInt64BigEndian(byte[] bytes, int offset)
        {
            if (offset + 8 > bytes.Length)
            {
                throw new ArgumentOutOfRangeException($"{nameof(offset)} went of out range.", nameof(offset));
            }

            byte[] readBytes = new byte[8];
            readBytes[0] = bytes[offset];
            readBytes[1] = bytes[offset + 1];
            readBytes[2] = bytes[offset + 2];
            readBytes[3] = bytes[offset + 3];
            readBytes[4] = bytes[offset + 4];
            readBytes[5] = bytes[offset + 5];
            readBytes[6] = bytes[offset + 6];
            readBytes[7] = bytes[offset + 7];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(readBytes);
            }

            return BitConverter.ToUInt64(readBytes, 0);
        }
    }
}
