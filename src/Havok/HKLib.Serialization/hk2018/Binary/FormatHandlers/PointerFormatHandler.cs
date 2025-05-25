using HKLib.Reflection.hk2018;
using HKLib.Serialization.hk2018.Binary.Util;

namespace HKLib.Serialization.hk2018.Binary.FormatHandlers;

internal static class PointerFormatHandler
{
    public static object Read(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        ulong pointer = reader.ReadUInt64();
        return pointer == 0 ? null! : context.GetItem(pointer, reader)[0];
    }

    public static void Write(HavokBinaryWriter writer, HavokType type, object? value,
        BinarySerializeContext context)
    {
        if (value is null)
        {
            writer.WriteUInt64(0);
            return;
        }

        HavokType actualSubType = FormatHandler.GetActualType(value, context.TypeRegistry);

        ulong pointer = context.Enqueue(actualSubType, writer.Position, value);
        context.RegisterPatch(type, writer.Position);
        writer.WriteUInt64(pointer);
    }
}