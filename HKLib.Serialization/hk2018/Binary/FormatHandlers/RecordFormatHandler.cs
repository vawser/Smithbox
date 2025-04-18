using HKLib.hk2018;
using HKLib.Reflection.hk2018;
using HKLib.Serialization.hk2018.Binary.Util;

namespace HKLib.Serialization.hk2018.Binary.FormatHandlers;

internal static class RecordFormatHandler
{
    public static object Read(HavokBinaryReader reader, HavokType type,
        BinaryDeserializeContext context)
    {
        HavokData data = HavokData.Instantiate(type) ??
                         throw new ArgumentException($"Failed to instantiate type {type.Identity}", nameof(type));

        long objectOffset = reader.Position;
        if (objectOffset % type.Alignment != 0)
        {
            throw new InvalidOperationException(
                $"Attempted unaligned read. Position: {objectOffset} | Alignment: {type.Alignment}");
        }

        foreach (HavokType.Member field in type.Fields)
        {
            if (reader.Position - objectOffset > field.Offset)
            {
                throw new InvalidOperationException("Read past end of field");
            }

            while (reader.Position - objectOffset < field.Offset)
            {
                reader.AssertByte(0);
            }

            if (field.NonSerializable)
            {
                for (int i = 0; i < field.Type.Size; i++)
                {
                    reader.AssertByte(0);
                }

                continue;
            }

            object fieldValue = FormatHandler.Read(reader, field.Type, context);
            if (!data.TrySetField(field.Name, fieldValue))
            {
                throw new InvalidOperationException(
                    $"Unable to set field \"{field.Name}\" of type \"{field.Type.Identity}\" in object of type \"{data.Type.Identity}\"");
            }
        }

        while (reader.Position - objectOffset < type.Size)
        {
            reader.AssertByte(0);
        }

        if (reader.Position - objectOffset > type.Size)
            throw new InvalidOperationException("Read past the end of the object");

        return data.GetObject<IHavokObject>()!;
    }

    public static void Write(HavokBinaryWriter writer, HavokType type, object? value,
        BinarySerializeContext context)
    {
        // non-nullable nested-structs which were not instantiated are instantiated here
        value ??= type.Instantiate();

        if (value is not IHavokObject havokObject)
        {
            throw new ArgumentException($"Value of type {value.GetType()} does not implement {nameof(IHavokObject)}",
                nameof(value));
        }

        HavokData data = HavokData.Of(havokObject, type)
                         ?? throw new ArgumentException(
                             $"Failed to get a HavokData representation for object of type {value.GetType()}",
                             nameof(value));

        long objectOffset = writer.Position;
        foreach (HavokType.Member field in type.Fields)
        {
            if (writer.Position - objectOffset > field.Offset)
            {
                throw new InvalidOperationException("Wrote past end of field.");
            }

            while (writer.Position - objectOffset < field.Offset)
            {
                writer.WriteByte(0);
            }

            if (field.NonSerializable)
            {
                for (int i = 0; i < field.Type.Size; i++)
                {
                    writer.WriteByte(0);
                }

                continue;
            }


            if (!data.TryGetField(field.Name, out object? fieldValue))
            {
                throw new InvalidOperationException(
                    $"Unable to get field \"{field.Name}\" of type \"{field.Type.Identity}\" in object of type \"{data.Type.Identity}\"");
            }

            FormatHandler.Write(writer, field.Type, fieldValue!, context);
        }

        if (writer.Position - objectOffset > type.Size)
            throw new InvalidOperationException("Wrote past end of object");

        while (writer.Position - objectOffset < type.Size)
        {
            writer.WriteByte(0);
        }
    }
}