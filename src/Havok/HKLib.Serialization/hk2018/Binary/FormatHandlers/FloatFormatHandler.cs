using System.Runtime.Intrinsics;
using HKLib.Reflection.hk2018;
using HKLib.Serialization.hk2018.Binary.Util;

namespace HKLib.Serialization.hk2018.Binary.FormatHandlers;

internal static class FloatFormatHandler
{
    private const int EndianMask = 0x100;

    public static object Read(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        int format = type.Format;
        bool bigEndian = (format & EndianMask) != 0;

        return (type.Size, format >> 16) switch
        {
            (2, _) => ReadHalf(reader, bigEndian),
            (4, _) => reader.ReadSingle(bigEndian),
            (8, _) => reader.ReadDouble(bigEndian),
            (16, 23) => ReadSimdFloat(reader, bigEndian),
            (16, 52) => ReadSimdDouble(reader, bigEndian),
            _ => throw new InvalidDataException("Unexpected float format")
        };
    }

    public static void Write(HavokBinaryWriter writer, HavokType type, object value,
        BinarySerializeContext context)
    {
        int format = type.Format;
        bool bigEndian = (format & EndianMask) != 0;
        switch (type.Size, format >> 16)
        {
            case (2, _):
                WriteHalf(writer, bigEndian, value);
                break;
            case (4, _):
                writer.WriteSingle((float)value, bigEndian);
                break;
            case (8, _):
                writer.WriteDouble((double)value, bigEndian);
                break;
            case (16, 23):
                WriteSimdFloat(writer, bigEndian, value);
                break;
            case (16, 52):
                WriteSimdDouble(writer, bigEndian, value);
                break;
            default:
                throw new ArgumentException("Invalid float format", nameof(type));
        }
    }

    private static object ReadHalf(HavokBinaryReader reader, bool bigEndian)
    {
        byte[] bytes = new byte[4];
        reader.ReadBytes(bytes, bigEndian ? 0 : 2, 2);
        if (bigEndian)
        {
            Array.Reverse(bytes);
        }

        return BitConverter.ToSingle(bytes);
    }

    private static object ReadSimdFloat(HavokBinaryReader reader, bool bigEndian)
    {
        float val = reader.ReadSingle(bigEndian);
        reader.ReadSingle(bigEndian);
        reader.ReadSingle(bigEndian);
        reader.ReadSingle(bigEndian);

        return val;
    }

    private static object ReadSimdDouble(HavokBinaryReader reader, bool bigEndian)
    {
        double val = reader.ReadDouble(bigEndian);
        reader.ReadDouble(bigEndian);

        return val;
    }

    private static void WriteHalf(HavokBinaryWriter writer, bool bigEndian, object value)
    {
        float floatValue = (float)value;
        Span<byte> bytes = stackalloc byte[4];
        BitConverter.TryWriteBytes(bytes, floatValue);
        if (bigEndian)
        {
            writer.WriteByte(bytes[3]);
            writer.WriteByte(bytes[2]);
        }
        else
        {
            writer.WriteByte(bytes[2]);
            writer.WriteByte(bytes[3]);
        }
    }

    private static void WriteSimdFloat(HavokBinaryWriter writer, bool bigEndian, object value)
    {
        float floatVal = (float)value;

        writer.WriteSingle(floatVal, bigEndian);
        writer.WriteSingle(floatVal, bigEndian);
        writer.WriteSingle(floatVal, bigEndian);
        writer.WriteSingle(floatVal, bigEndian);
    }

    private static void WriteSimdDouble(HavokBinaryWriter writer, bool bigEndian, object value)
    {
        double doubleVal = (double)value;
        writer.WriteDouble(doubleVal, bigEndian);
        writer.WriteDouble(doubleVal, bigEndian);
    }
}