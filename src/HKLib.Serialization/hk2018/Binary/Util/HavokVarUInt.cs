namespace HKLib.Serialization.hk2018.Binary.Util;

/// <summary>
///     Contains methods for VarUInt conversion and serialization
/// </summary>
public record HavokVarUInt
{
    private static readonly HavokVarUInt[] VarUIntTypes =
    {
        new(7, 1, 0),
        new(14, 2, 0b10000000),
        new(21, 3, 0b11000000),
        new(27, 4, 0b11100000),
        new(35, 5, 0b11101000),
        new(40, 6, 0b11111000),
        new(59, 8, 0b11110000),
        new(64, 9, 0b11111001)
    };

    private HavokVarUInt(int bitCount, int byteCount, byte prefix)
    {
        BitCount = bitCount;
        ByteCount = byteCount;
        Prefix = prefix;
    }

    private int BitCount { get; }
    private int ByteCount { get; }
    private byte Prefix { get; }

    /// <summary>
    ///     Gets the length of a serialized VarUInt from its first byte
    /// </summary>
    /// <param name="prefix">Byte to get the length from</param>
    /// <returns>The length of the VarUInt in bytes</returns>
    public static int GetLength(byte prefix)
    {
        foreach (HavokVarUInt varIntInfo in VarUIntTypes)
        {
            int prefixMask = ~((1 << (varIntInfo.BitCount % 8)) - 1);
            if ((prefix & prefixMask) != varIntInfo.Prefix) continue;

            return varIntInfo.ByteCount;
        }

        throw new ArgumentException("Invalid prefix.", nameof(prefix));
    }

    /// <summary>
    ///     Converts a UInt64 to its serialized VarUInt representation
    /// </summary>
    /// <param name="value">The value to convert</param>
    public static byte[] ToBytes(ulong value)
    {
        foreach (HavokVarUInt varIntInfo in VarUIntTypes)
        {
            if (value >= 1UL << varIntInfo.BitCount && varIntInfo.BitCount != 64) continue;

            byte[] bytes = BitConverter.GetBytes(value);
            Array.Resize(ref bytes, varIntInfo.ByteCount);
            Array.Reverse(bytes);
            bytes[0] |= varIntInfo.Prefix;
            return bytes;
        }

        throw new Exception("Unreachable");
    }

    /// <summary>
    ///     Converts a serialized VarUInt to a UInt64
    /// </summary>
    /// <param name="bytes">The bytes to convert</param>
    public static ulong ToUInt64(byte[] bytes)
    {
        byte prefix = bytes[0];
        foreach (HavokVarUInt varIntInfo in VarUIntTypes)
        {
            int prefixMask = ~((1 << (varIntInfo.BitCount % 8)) - 1);
            if ((prefix & prefixMask) != varIntInfo.Prefix) continue;

            if (varIntInfo.ByteCount != bytes.Length)
            {
                throw new ArgumentException(
                    $"The provided byte array is of the wrong length. Expected length: {varIntInfo.ByteCount}, Length received: {bytes.Length}");
            }

            Array.Reverse(bytes);
            Array.Resize(ref bytes, 8);

            if (varIntInfo.ByteCount < 9)
            {
                prefix &= (byte)~prefixMask;
                bytes[varIntInfo.ByteCount - 1] = prefix;
            }

            return BitConverter.ToUInt64(bytes);
        }

        throw new ArgumentException("The provided byte array is not a valid VarUInt.");
    }

    /// <summary>
    ///     Converts an Int64 to its serialized VarUInt representation
    /// </summary>
    /// <param name="value">The value to convert</param>
    public static byte[] ToBytes(long value)
    {
        if (value >= 0)
        {
            value <<= 1;
        }
        else
        {
            value = -(value + 1);
        }

        return ToBytes((ulong)value);
    }

    /// <summary>
    ///     Converts a serialized VarUInt to a Int64
    /// </summary>
    /// <param name="bytes">The bytes to convert</param>
    public static long ToInt64(byte[] bytes)
    {
        ulong unsigned = ToUInt64(bytes);
        long signed = (long)(unsigned >> 1);
        if ((unsigned & 1) == 0)
        {
            return signed;
        }

        return -signed - 1;
    }
}