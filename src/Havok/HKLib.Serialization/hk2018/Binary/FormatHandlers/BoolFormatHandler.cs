using HKLib.Reflection.hk2018;
using HKLib.Serialization.hk2018.Binary.Util;

namespace HKLib.Serialization.hk2018.Binary.FormatHandlers;

internal static class BoolFormatHandler
{
    private const int EndianMask = 0x100;

    public static object Read(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        int format = type.Format;
        bool bigEndian = (format & EndianMask) != 0;

        return (format >> 10) switch
        {
            // some booleans are neither 1 nor 0 whereby any non-zero value counts as true.
            // FileConvert always writes 1 for true and 0 for false when converting from xml so the exact value should not be important.
            8 => reader.ReadByte() != 0,
            32 => reader.ReadUInt32(bigEndian) != 0,
            _ => throw new InvalidDataException("Unexpected boolean format.")
        };
    }

    public static void Write(HavokBinaryWriter writer, HavokType type, object value,
        BinarySerializeContext context)
    {
        bool boolValue = (bool)value;
        int format = type.Format;
        bool bigEndian = (format & EndianMask) != 0;

        switch (format >> 10)
        {
            case 8:
                writer.WriteBoolean(boolValue);
                break;
            case 32:
                writer.WriteInt32(boolValue ? 1 : 0, bigEndian);
                break;
        }
    }
}