using System.Text;
using HKLib.Reflection.hk2018;
using HKLib.Serialization.hk2018.Binary.Util;

namespace HKLib.Serialization.hk2018.Binary.FormatHandlers;

internal static class StringFormatHandler
{
    private const int FixedMask = 0x10;

    private static readonly HavokType SubType =
        new HavokTypeBuilder()
            .WithName("char")
            .WithOptionals(HavokType.Optional.Format | HavokType.Optional.SizeAlign)
            .WithFormat(8196)
            .WithSizeAlignment(1, 1)
            .Build();

    public static object Read(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        int format = type.Format;
        if ((format & FixedMask) != 0)
        {
            return reader.ReadASCII(format >> 16);
        }

        ulong pointer = reader.ReadUInt64();
        if (pointer == 0) return null!;
        object[] chars = context.GetItem(pointer, reader);
        StringBuilder stringBuilder = new();
        for (int i = 0; i < chars.Length - 1; i++)
        {
            char character = (char)(byte)chars[i];
            stringBuilder.Append(character);
        }

        return stringBuilder.ToString();
    }

    public static void Write(HavokBinaryWriter writer, HavokType type, object? value,
        BinarySerializeContext context)
    {
        string? stringValue = (string?)value;

        int format = type.Format;
        if ((format & FixedMask) != 0)
        {
            if (stringValue is null || stringValue.Length != format >> 16)
            {
                throw new ArgumentException("Invalid string length.", nameof(value));
            }

            writer.WriteASCII(stringValue);
            return;
        }

        if (stringValue is null)
        {
            writer.WriteUInt64(0);
            return;
        }

        object[] chars = new object[stringValue.Length + 1];
        for (int i = 0; i < stringValue.Length; i++)
        {
            chars[i] = (byte)stringValue[i];
        }

        // null terminated
        chars[stringValue.Length] = (byte)0;

        HavokType? charType = context.TypeRegistry.GetType("char");
        if (charType is null)
            throw new InvalidOperationException("Char type was not found in the current type registry");

        ulong pointer = context.Enqueue(charType, writer.Position, chars, stringValue);
        context.RegisterPatch(type, writer.Position);
        writer.WriteUInt64(pointer);
    }
}