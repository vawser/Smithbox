using System.Drawing;
using HKLib.Reflection.hk2018;
using HKLib.Serialization.hk2018.Binary.Util;

namespace HKLib.Serialization.hk2018.Binary.FormatHandlers;

internal static class IntFormatHandler
{
    private const int EndianMask = 0x100;
    private const int SignMask = 0x200;

    public static object Read(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        int format = type.Format;
        bool bigEndian = (format & EndianMask) != 0;

        bool isSigned = (format & SignMask) != 0;
        object obj = ((format >> 10) / 8, isSigned) switch
        {
            (1, true) => reader.ReadSByte(),
            (1, false) => reader.ReadByte(),
            (2, true) => reader.ReadInt16(bigEndian),
            (2, false) => reader.ReadUInt16(bigEndian),
            (4, true) => reader.ReadInt32(bigEndian),
            (4, false) => reader.ReadUInt32(bigEndian),
            (8, true) => reader.ReadInt64(bigEndian),
            (8, false) => reader.ReadUInt64(bigEndian),
            _ => throw new InvalidDataException("Unexpected int format")
        };

        return type.Type == typeof(Color) ? Color.FromArgb(unchecked((int)(uint)obj)) : obj;
    }

    public static void Write(HavokBinaryWriter writer, HavokType type, dynamic value,
        BinarySerializeContext context)
    {
        if (value is Color color) value = color.ToArgb();

        int format = type.Format;
        bool bigEndian = (format & EndianMask) != 0;

        bool isSigned = (format & SignMask) != 0;
        switch ((format >> 10) / 8, isSigned)
        {
            case (1, true):
                writer.WriteSByte((sbyte)value);
                break;
            case (1, false):
                writer.WriteByte((byte)value);
                break;
            case (2, true):
                writer.WriteInt16((short)value, bigEndian);
                break;
            case (2, false):
                writer.WriteUInt16((ushort)value, bigEndian);
                break;
            case (4, true):
                writer.WriteInt32((int)value, bigEndian);
                break;
            case (4, false):
                writer.WriteUInt32((uint)value, bigEndian);
                break;
            case (8, true):
                writer.WriteInt64((long)value, bigEndian);
                break;
            case (8, false):
                writer.WriteUInt64((ulong)value, bigEndian);
                break;
            default:
                throw new InvalidDataException("Unexpected int format");
        }
    }
}