using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using Havok.Reflection;

namespace Havok.Formats.Tagfile;

// Ported from HKLib.Serialization's hk2018/hk2019 Binary/FormatHandlers (the 2019 copies, which are a superset).
// The handlers work on Core HavokTypes and plain objects; records go through the class set's field access.

internal static class FormatHandler
{
    private const int EndianMask = 0x100;

    public static object Read(TagfileReader reader, HavokType type, TagfileReadContext context)
    {
        return type.Kind switch
        {
            HavokType.TypeKind.Bool => BoolFormat.Read(reader, type),
            HavokType.TypeKind.String => StringFormat.Read(reader, type, context),
            HavokType.TypeKind.Int => IntFormat.Read(reader, type),
            HavokType.TypeKind.Float => FloatFormat.Read(reader, type),
            HavokType.TypeKind.Pointer => PointerFormat.Read(reader, type, context),
            HavokType.TypeKind.Record => RecordFormat.Read(reader, type, context),
            HavokType.TypeKind.Array => ArrayFormat.Read(reader, type, context),
            _ => throw new NotImplementedException($"Reading {type.Kind} types ({type.Identity}) is not supported.")
        };
    }

    public static void Write(TagfileWriter writer, HavokType type, object value, TagfileWriteContext context)
    {
        switch (type.Kind)
        {
            case HavokType.TypeKind.Bool: BoolFormat.Write(writer, type, value); break;
            case HavokType.TypeKind.String: StringFormat.Write(writer, type, value, context); break;
            case HavokType.TypeKind.Int: IntFormat.Write(writer, type, value); break;
            case HavokType.TypeKind.Float: FloatFormat.Write(writer, type, value); break;
            case HavokType.TypeKind.Pointer: PointerFormat.Write(writer, type, value, context); break;
            case HavokType.TypeKind.Record: RecordFormat.Write(writer, type, value, context); break;
            case HavokType.TypeKind.Array: ArrayFormat.Write(writer, type, value, context); break;
            default: throw new KeyNotFoundException($"Writing {type.Kind} types ({type.Identity}) is not supported.");
        }
    }

    public static bool IsBigEndian(HavokType type) => (type.Format & EndianMask) != 0;

    public static HavokType GetActualType(object value, HavokTypeRegistry registry)
    {
        return registry.GetType(value.GetType()) ??
               throw new ArgumentException(
                   "There is no HavokType corresponding to the given value in the current type registry",
                   nameof(value));
    }
}

internal static class BoolFormat
{
    public static object Read(TagfileReader reader, HavokType type)
    {
        bool bigEndian = FormatHandler.IsBigEndian(type);
        return (type.Format >> 10) switch
        {
            // some booleans are neither 1 nor 0, any non-zero value counts as true
            8 => reader.ReadByte() != 0,
            32 => reader.ReadUInt32(bigEndian) != 0,
            _ => throw new InvalidDataException("Unexpected boolean format.")
        };
    }

    public static void Write(TagfileWriter writer, HavokType type, object value)
    {
        bool boolValue = (bool)value;
        switch (type.Format >> 10)
        {
            case 8:
                writer.WriteBoolean(boolValue);
                break;
            case 32:
                writer.WriteInt32(boolValue ? 1 : 0, FormatHandler.IsBigEndian(type));
                break;
        }
    }
}

internal static class IntFormat
{
    private const int SignMask = 0x200;

    public static object Read(TagfileReader reader, HavokType type)
    {
        int format = type.Format;
        bool bigEndian = FormatHandler.IsBigEndian(type);
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

    public static void Write(TagfileWriter writer, HavokType type, object boxed)
    {
        // dynamic, as in HKLib: the value may be any integer or enum type
        dynamic value = boxed is Color color ? color.ToArgb() : boxed;

        int format = type.Format;
        bool bigEndian = FormatHandler.IsBigEndian(type);
        bool isSigned = (format & SignMask) != 0;
        switch ((format >> 10) / 8, isSigned)
        {
            case (1, true): writer.WriteSByte((sbyte)value); break;
            case (1, false): writer.WriteByte((byte)value); break;
            case (2, true): writer.WriteInt16((short)value, bigEndian); break;
            case (2, false): writer.WriteUInt16((ushort)value, bigEndian); break;
            case (4, true): writer.WriteInt32((int)value, bigEndian); break;
            case (4, false): writer.WriteUInt32((uint)value, bigEndian); break;
            case (8, true): writer.WriteInt64((long)value, bigEndian); break;
            case (8, false): writer.WriteUInt64((ulong)value, bigEndian); break;
            default: throw new InvalidDataException("Unexpected int format");
        }
    }
}

internal static class FloatFormat
{
    public static object Read(TagfileReader reader, HavokType type)
    {
        bool bigEndian = FormatHandler.IsBigEndian(type);
        return (type.Size, type.Format >> 16) switch
        {
            (2, _) => ReadHalf(reader, bigEndian),
            (4, _) => reader.ReadSingle(bigEndian),
            (8, _) => reader.ReadDouble(bigEndian),
            (16, 23) => ReadSimdFloat(reader, bigEndian),
            (16, 52) => ReadSimdDouble(reader, bigEndian),
            _ => throw new InvalidDataException("Unexpected float format")
        };
    }

    public static void Write(TagfileWriter writer, HavokType type, object value)
    {
        bool bigEndian = FormatHandler.IsBigEndian(type);
        switch (type.Size, type.Format >> 16)
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
                for (int i = 0; i < 4; i++) writer.WriteSingle((float)value, bigEndian);
                break;
            case (16, 52):
                for (int i = 0; i < 2; i++) writer.WriteDouble((double)value, bigEndian);
                break;
            default:
                throw new ArgumentException("Invalid float format", nameof(type));
        }
    }

    // hkHalf is the top 16 bits of a float
    private static object ReadHalf(TagfileReader reader, bool bigEndian)
    {
        byte[] bytes = new byte[4];
        reader.ReadBytes(bytes, bigEndian ? 0 : 2, 2);
        if (bigEndian) Array.Reverse(bytes);
        return BitConverter.ToSingle(bytes);
    }

    private static void WriteHalf(TagfileWriter writer, bool bigEndian, object value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BitConverter.TryWriteBytes(bytes, (float)value);
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

    private static object ReadSimdFloat(TagfileReader reader, bool bigEndian)
    {
        float value = reader.ReadSingle(bigEndian);
        reader.ReadSingle(bigEndian);
        reader.ReadSingle(bigEndian);
        reader.ReadSingle(bigEndian);
        return value;
    }

    private static object ReadSimdDouble(TagfileReader reader, bool bigEndian)
    {
        double value = reader.ReadDouble(bigEndian);
        reader.ReadDouble(bigEndian);
        return value;
    }
}

internal static class StringFormat
{
    private const int FixedMask = 0x10;

    public static object Read(TagfileReader reader, HavokType type, TagfileReadContext context)
    {
        int format = type.Format;
        if ((format & FixedMask) != 0)
        {
            return reader.ReadASCII(format >> 16);
        }

        ulong pointer = reader.ReadUInt64();
        if (pointer == 0) return null!;
        if (context.Strings.TryGetValue(pointer, out string? cached)) return cached;

        object[] chars = context.GetItem(pointer, reader);
        var builder = new System.Text.StringBuilder(chars.Length);
        for (int i = 0; i < chars.Length - 1; i++)
        {
            builder.Append((char)(byte)chars[i]);
        }

        string value = builder.ToString();
        context.Strings[pointer] = value;
        return value;
    }

    public static void Write(TagfileWriter writer, HavokType type, object? value, TagfileWriteContext context)
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

        chars[stringValue.Length] = (byte)0;

        HavokType charType = context.Registry.GetType("char") ??
                             throw new InvalidOperationException("Char type was not found in the current type registry");

        ulong pointer = context.Enqueue(charType, writer.Position, chars, stringValue);
        context.RegisterPatch(type, writer.Position);
        writer.WriteUInt64(pointer);
    }
}

internal static class PointerFormat
{
    public static object Read(TagfileReader reader, HavokType type, TagfileReadContext context)
    {
        ulong pointer = reader.ReadUInt64();
        return pointer == 0 ? null! : context.GetItem(pointer, reader)[0];
    }

    public static void Write(TagfileWriter writer, HavokType type, object? value, TagfileWriteContext context)
    {
        if (value is null)
        {
            writer.WriteUInt64(0);
            return;
        }

        HavokType actualSubType = FormatHandler.GetActualType(value, context.Registry);
        ulong pointer = context.Enqueue(actualSubType, writer.Position, value);
        context.RegisterPatch(type, writer.Position);
        writer.WriteUInt64(pointer);
    }
}

internal static class RecordFormat
{
    public static object Read(TagfileReader reader, HavokType type, TagfileReadContext context)
    {
        IHavokFieldAccess data = context.ClassSet.Instantiate(type);

        long objectOffset = reader.Position;
        if (type.Alignment != 0 && objectOffset % type.Alignment != 0)
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
                    $"Unable to set field \"{field.Name}\" of type \"{field.Type.Identity}\" in object of type \"{type.Identity}\"");
            }
        }

        while (reader.Position - objectOffset < type.Size)
        {
            reader.AssertByte(0);
        }

        if (reader.Position - objectOffset > type.Size)
            throw new InvalidOperationException("Read past the end of the object");

        return data.Object;
    }

    public static void Write(TagfileWriter writer, HavokType type, object? value, TagfileWriteContext context)
    {
        // non-nullable nested structs which were not instantiated are instantiated here
        value ??= type.Instantiate();

        if (!context.ClassSet.IsHavokObject(value))
        {
            throw new ArgumentException($"Value of type {value.GetType()} is not a Havok object of {context.ClassSet.Name}",
                nameof(value));
        }

        IHavokFieldAccess data = context.ClassSet.Access(value, type);

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
                    $"Unable to get field \"{field.Name}\" of type \"{field.Type.Identity}\" in object of type \"{type.Identity}\"");
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

internal abstract class ArrayFormat
{
    // Files are read in parallel, so the handler of an element type may be looked up by several threads at once
    private static readonly ConcurrentDictionary<Type, ArrayFormat> Handlers = new();

    private static ArrayFormat GetHandler(Type elementType)
    {
        return Handlers.GetOrAdd(elementType, static type =>
            (ArrayFormat)Activator.CreateInstance(typeof(ArrayFormat<>).MakeGenericType(type))!);
    }

    public static object Read(TagfileReader reader, HavokType type, TagfileReadContext context)
    {
        if (type.Identity == "hkPropertyBag")
        {
            // the only field is non-serialized
            reader.AssertUInt64(0);
            return type.Instantiate();
        }

        if (type.Type == typeof(object)) return ReadHkReflectAny(reader, context);
        if (type.Type == typeof(Vector4)) return ReadVector4(reader, type, context);
        if (type.Type == typeof(Quaternion))
        {
            Vector4 vector = ReadVector4(reader, type, context);
            return new Quaternion(vector.X, vector.Y, vector.Z, vector.W);
        }

        if (type.Type == typeof(Matrix4x4)) return ReadMatrix(reader, type, context, 4);
        if (type.Type is { } matrix3 && matrix3 == context.ClassSet.Matrix3x3Type)
        {
            return context.ClassSet.ToMatrix3x3(ReadMatrix(reader, type, context, 3));
        }

        Type elementType = type.SubType!.Name == "T[N]" ? type.SubType!.SubType!.Type! : type.SubType!.Type!;
        return GetHandler(elementType).ReadValue(reader, type, context);
    }

    public static void Write(TagfileWriter writer, HavokType type, object value, TagfileWriteContext context)
    {
        if (type.Identity == "hkPropertyBag")
        {
            writer.WriteUInt64(0);
            return;
        }

        if (type.Type == typeof(object))
        {
            WriteHkReflectAny(writer, type, value, context);
            return;
        }

        if (type.Type == typeof(Vector4))
        {
            WriteVector4(writer, type, (Vector4)value, context);
            return;
        }

        if (type.Type == typeof(Quaternion))
        {
            Quaternion q = (Quaternion)value;
            WriteVector4(writer, type, new Vector4(q.X, q.Y, q.Z, q.W), context);
            return;
        }

        if (type.Type == typeof(Matrix4x4))
        {
            WriteMatrix(writer, type, (Matrix4x4)value, context, 4);
            return;
        }

        if (type.Type is { } matrix3 && matrix3 == context.ClassSet.Matrix3x3Type)
        {
            WriteMatrix(writer, type, context.ClassSet.FromMatrix3x3(value), context, 3);
            return;
        }

        Type elementType = type.SubType!.Name == "T[N]" ? type.SubType!.SubType!.Type! : type.SubType!.Type!;
        GetHandler(elementType).WriteValue(writer, type, value, context);
    }

    protected abstract object ReadValue(TagfileReader reader, HavokType type, TagfileReadContext context);

    protected abstract void WriteValue(TagfileWriter writer, HavokType type, object value, TagfileWriteContext context);

    private static object ReadHkReflectAny(TagfileReader reader, TagfileReadContext context)
    {
        ulong pointer = reader.ReadUInt64();
        if (pointer == 0) return null!;
        object[] objects = context.GetItem(pointer, reader);
        if (objects.Length > 1) throw new NotImplementedException();
        return objects[0];
    }

    private static void WriteHkReflectAny(TagfileWriter writer, HavokType type, object? value, TagfileWriteContext context)
    {
        if (value is null)
        {
            writer.WriteUInt64(0);
            return;
        }

        HavokType actualSubType = FormatHandler.GetActualType(value, context.Registry);
        ulong pointer = context.Enqueue(actualSubType, writer.Position, value);
        context.RegisterPatch(type, writer.Position);
        writer.WriteUInt64(pointer);
    }

    private static Vector4 ReadVector4(TagfileReader reader, HavokType type, TagfileReadContext context)
    {
        HavokType subType = GetSubType(type);
        float x = (float)FormatHandler.Read(reader, subType, context);
        float y = (float)FormatHandler.Read(reader, subType, context);
        float z = (float)FormatHandler.Read(reader, subType, context);
        float w = (float)FormatHandler.Read(reader, subType, context);
        return new Vector4(x, y, z, w);
    }

    private static void WriteVector4(TagfileWriter writer, HavokType type, Vector4 value, TagfileWriteContext context)
    {
        HavokType subType = GetSubType(type);
        FormatHandler.Write(writer, subType, value.X, context);
        FormatHandler.Write(writer, subType, value.Y, context);
        FormatHandler.Write(writer, subType, value.Z, context);
        FormatHandler.Write(writer, subType, value.W, context);
    }

    /// <summary>
    /// Matrices are rows of 4 floats: 4 rows for hkMatrix4, 3 for hkMatrix3.
    /// </summary>
    private static Matrix4x4 ReadMatrix(TagfileReader reader, HavokType type, TagfileReadContext context, int rows)
    {
        HavokType subType = GetSubType(type);
        Matrix4x4 matrix = new();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                matrix[i, j] = (float)FormatHandler.Read(reader, subType, context);
            }
        }

        return matrix;
    }

    private static void WriteMatrix(TagfileWriter writer, HavokType type, Matrix4x4 matrix, TagfileWriteContext context,
        int rows)
    {
        HavokType subType = GetSubType(type);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                FormatHandler.Write(writer, subType, matrix[i, j], context);
            }
        }
    }

    private static HavokType GetSubType(HavokType type)
    {
        while (type.SubType is null && type.Parent is not null)
        {
            type = type.Parent;
        }

        return type.SubType!;
    }
}

internal sealed class ArrayFormat<T> : ArrayFormat
{
    protected override object ReadValue(TagfileReader reader, HavokType type, TagfileReadContext context)
    {
        return type.Name switch
        {
            "hkArray" => ReadHkArray(reader, context),
            "hkRelArray" => ReadHkRelArray(reader, context),
            "hkRelArray32" => ReadHkRelArray32(reader, context),
            "T[N]" => ReadFixedArray(reader, type, context),
            _ => throw new NotImplementedException($"Reading arrays of type {type.Name} is not supported.")
        };
    }

    protected override void WriteValue(TagfileWriter writer, HavokType type, object value, TagfileWriteContext context)
    {
        switch (type.Name)
        {
            case "hkArray": WriteHkArray(writer, type, value, context); break;
            case "hkRelArray": WriteHkRelArray(writer, type, value, context); break;
            case "hkRelArray32": WriteHkRelArray32(writer, type, value, context); break;
            case "T[N]": WriteFixedArray(writer, type, value, context); break;
            default: throw new NotImplementedException($"Writing arrays of type {type.Name} is not supported.");
        }
    }

    private static void PopulateList(TagfileReader reader, List<T> list, ulong pointer, TagfileReadContext context)
    {
        foreach (object element in context.GetItem(pointer, reader))
        {
            if (element is not (null or T))
                throw new InvalidOperationException(
                    $"Invalid element type. Expected: {typeof(T)} | Received: {element.GetType()}");
            list.Add((T)element!);
        }
    }

    private static object ReadHkArray(TagfileReader reader, TagfileReadContext context)
    {
        List<T> list = new();
        ulong pointer = reader.ReadUInt64();
        reader.AssertUInt64(0); // size and flags
        if (pointer != 0) PopulateList(reader, list, pointer, context);
        return list;
    }

    private static void WriteHkArray(TagfileWriter writer, HavokType type, object? value, TagfileWriteContext context)
    {
        if (value is not List<T> { Count: > 0 } list)
        {
            writer.WriteUInt64(0);
            writer.WriteUInt64(0);
            return;
        }

        ulong pointer = EnqueueList(writer, type, context, list, 8);
        context.RegisterPatch(type, writer.Position);
        writer.WriteUInt64(pointer);
        writer.WriteUInt64(0);
    }

    // serialized as a single 32-bit item pointer
    private static object ReadHkRelArray(TagfileReader reader, TagfileReadContext context)
    {
        List<T> list = new();
        ulong pointer = reader.ReadUInt32();
        if (pointer != 0) PopulateList(reader, list, pointer, context);
        return list;
    }

    private static void WriteHkRelArray(TagfileWriter writer, HavokType type, object? value, TagfileWriteContext context)
    {
        if (value is not List<T> { Count: > 0 } list)
        {
            writer.WriteUInt32(0);
            return;
        }

        ulong pointer = EnqueueList(writer, type, context, list, 4);
        if (context.Dialect.PatchRelArrays) context.RegisterPatch(type, writer.Position);
        writer.WriteUInt32((uint)pointer);
    }

    /// <summary>
    /// The 32-bit offset and size hkRelArray added in 2019: an item pointer followed by padding. Unlike hkRelArray,
    /// its pointer is patched.
    /// </summary>
    private static object ReadHkRelArray32(TagfileReader reader, TagfileReadContext context)
    {
        List<T> list = new();
        ulong pointer = reader.ReadUInt32();
        reader.AssertUInt32(0);
        if (pointer != 0) PopulateList(reader, list, pointer, context);
        return list;
    }

    private static void WriteHkRelArray32(TagfileWriter writer, HavokType type, object? value, TagfileWriteContext context)
    {
        if (value is not List<T> { Count: > 0 } list)
        {
            writer.WriteUInt64(0);
            return;
        }

        ulong pointer = EnqueueList(writer, type, context, list, 4);
        context.RegisterPatch(type, writer.Position);
        writer.WriteUInt32((uint)pointer);
        writer.WriteUInt32(0);
    }

    private static ulong EnqueueList(TagfileWriter writer, HavokType type, TagfileWriteContext context, List<T> list,
        int pointerSize)
    {
        object[] objects = new object[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            objects[i] = list[i]!;
        }

        return context.Enqueue(type.SubType!, writer.Position, objects, list, pointerSize);
    }

    private static object ReadFixedArray(TagfileReader reader, HavokType type, TagfileReadContext context)
    {
        if (type.SubType!.Name == "T[N]")
        {
            int length1 = type.Format >> 8;
            int length2 = type.SubType!.Format >> 8;
            T[,] array2 = new T[length1, length2];
            for (int i = 0; i < length1; i++)
            {
                for (int j = 0; j < length2; j++)
                {
                    array2[i, j] = (T)FormatHandler.Read(reader, type.SubType!.SubType!, context);
                }
            }

            return array2;
        }

        int size = type.Format >> 8;
        T[] array = new T[size];
        for (int i = 0; i < size; i++)
        {
            // dynamic, as in HKLib: the element may need a numeric conversion to T
            dynamic element = FormatHandler.Read(reader, type.SubType!, context);
            array[i] = (T)element;
        }

        return array;
    }

    private static void WriteFixedArray(TagfileWriter writer, HavokType type, object value, TagfileWriteContext context)
    {
        if (type.SubType!.Name == "T[N]")
        {
            foreach (T element in (T[,])value)
            {
                FormatHandler.Write(writer, type.SubType!.SubType!, element!, context);
            }

            return;
        }

        foreach (T element in (T[])value)
        {
            FormatHandler.Write(writer, type.SubType!, element!, context);
        }
    }
}
