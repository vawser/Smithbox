using System.Numerics;
using HKLib.Serialization.hk2018.Binary.Util;
using HKLib.ValueTypes;
using HavokType = HKLib.Reflection.hk2018.HavokType;

namespace HKLib.Serialization.hk2018.Binary.FormatHandlers;

internal abstract class ArrayFormatHandler
{
    private static readonly Dictionary<Type, ArrayFormatHandler> ArrayFormatHandlers = new();

    private static ArrayFormatHandler GetFormatHandler(Type subType)
    {
        if (ArrayFormatHandlers.TryGetValue(subType, out ArrayFormatHandler? arrayFormatHandler))
            return arrayFormatHandler;

        Type formatHandlerType = typeof(ArrayFormatHandler<>).MakeGenericType(subType);
        ArrayFormatHandler formatHandler = (ArrayFormatHandler)Activator.CreateInstance(formatHandlerType)!;
        ArrayFormatHandlers.Add(subType, formatHandler);
        return formatHandler;
    }

    public static object Read(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        if (type.Identity == "hkPropertyBag")
        {
            return ReadHkPropertyBag(reader, type, context);
        }

        if (type.Type == typeof(object))
        {
            return ReadHkReflectAny(reader, type, context);
        }


        if (type.Type == typeof(Vector4))
        {
            return ReadVector4(reader, type, context);
        }

        if (type.Type == typeof(Quaternion))
        {
            return ReadQuaternion(reader, type, context);
        }

        if (type.Type == typeof(Matrix4x4))
        {
            return ReadMatrix4X4(reader, type, context);
        }

        if (type.Type == typeof(Matrix3x3))
        {
            return ReadMatrix3X3(reader, type, context);
        }

        Type elementType = type.SubType!.Name == "T[N]" ? type.SubType!.SubType!.Type! : type.SubType!.Type!;
        return GetFormatHandler(elementType).ReadValueImpl(reader, type, context);
    }

    public static void Write(HavokBinaryWriter writer, HavokType type, object value,
        BinarySerializeContext context)
    {
        if (type.Identity == "hkPropertyBag")
        {
            WriteHkPropertyBag(writer, type, value, context);
            return;
        }

        if (type.Type == typeof(object))
        {
            WriteHkReflectAny(writer, type, value, context);
            return;
        }

        if (type.Type == typeof(Vector4))
        {
            WriteVector4(writer, type, value, context);
            return;
        }

        if (type.Type == typeof(Quaternion))
        {
            WriteQuaternion(writer, type, value, context);
            return;
        }

        if (type.Type == typeof(Matrix4x4))
        {
            WriteMatrix4X4(writer, type, value, context);
            return;
        }

        if (type.Type == typeof(Matrix3x3))
        {
            WriteMatrix3X3(writer, type, value, context);
            return;
        }

        Type elementType = type.SubType!.Name == "T[N]" ? type.SubType!.SubType!.Type! : type.SubType!.Type!;
        GetFormatHandler(elementType).WriteValueImpl(writer, type, value, context);
    }

    protected abstract object ReadValueImpl(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context);

    protected abstract void WriteValueImpl(HavokBinaryWriter writer, HavokType type, object value,
        BinarySerializeContext context);

    private static object ReadHkPropertyBag(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        // only field is non-serialized
        reader.AssertUInt64(0);
        return type.Instantiate();
    }

    private static void WriteHkPropertyBag(HavokBinaryWriter writer, HavokType type, object value,
        BinarySerializeContext context)
    {
        // only field is non-serialized
        writer.WriteUInt64(0);
    }

    private static object ReadHkReflectAny(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        ulong pointer = reader.ReadUInt64();
        if (pointer == 0) return null!;
        object[] objects = context.GetItem(pointer, reader);
        if (objects.Length > 1) throw new NotImplementedException();

        return objects[0];
    }

    private static void WriteHkReflectAny(HavokBinaryWriter writer, HavokType type, object? value,
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

    private static Vector4 ReadVector4(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        HavokType subType = GetSubType(type);

        float x = (float)FormatHandler.Read(reader, subType, context);
        float y = (float)FormatHandler.Read(reader, subType, context);
        float z = (float)FormatHandler.Read(reader, subType, context);
        float w = (float)FormatHandler.Read(reader, subType, context);

        return new Vector4(x, y, z, w);
    }

    private static void WriteVector4(HavokBinaryWriter writer, HavokType type, object value,
        BinarySerializeContext context)
    {
        Vector4 vectorValue = (Vector4)value;
        HavokType subType = GetSubType(type);

        FormatHandler.Write(writer, subType, vectorValue.X, context);
        FormatHandler.Write(writer, subType, vectorValue.Y, context);
        FormatHandler.Write(writer, subType, vectorValue.Z, context);
        FormatHandler.Write(writer, subType, vectorValue.W, context);
    }

    private static object ReadQuaternion(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        Vector4 vector = ReadVector4(reader, type, context);
        return new Quaternion(vector.X, vector.Y, vector.Z, vector.W);
    }

    private static void WriteQuaternion(HavokBinaryWriter writer, HavokType type, object value,
        BinarySerializeContext context)
    {
        Quaternion quaternionValue = (Quaternion)value;
        Vector4 vector = new(quaternionValue.X, quaternionValue.Y, quaternionValue.Z, quaternionValue.W);
        WriteVector4(writer, type, vector, context);
    }

    private static object ReadMatrix4X4(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        HavokType subType = GetSubType(type);

        Matrix4x4 matrix = new();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                matrix[i, j] = (float)FormatHandler.Read(reader, subType, context);
            }
        }

        return matrix;
    }

    private static void WriteMatrix4X4(HavokBinaryWriter writer, HavokType type, object value,
        BinarySerializeContext context)
    {
        Matrix4x4 matrixValue = (Matrix4x4)value;
        HavokType subType = GetSubType(type);

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                // Havok matrices are column-major
                FormatHandler.Write(writer, subType, matrixValue[i, j], context);
            }
        }
    }

    private static object ReadMatrix3X3(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        HavokType subType = GetSubType(type);

        Matrix4x4 matrix = new();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                matrix[i, j] = (float)FormatHandler.Read(reader, subType, context);
            }
        }

        return (Matrix3x3)matrix;
    }

    private static void WriteMatrix3X3(HavokBinaryWriter writer, HavokType type, object value,
        BinarySerializeContext context)
    {
        Matrix4x4 matrixValue = (Matrix3x3)value;
        HavokType subType = GetSubType(type);

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                FormatHandler.Write(writer, subType, matrixValue[i, j], context);
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

internal class ArrayFormatHandler<T> : ArrayFormatHandler
{
    protected override object ReadValueImpl(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        return type.Name switch
        {
            "hkArray" => ReadHkArray(reader, type, context),
            "hkRelArray" => ReadHkRelArray(reader, type, context),
            "T[N]" => ReadArray(reader, type, context),
            _ => throw new NotImplementedException()
        };
    }

    protected override void WriteValueImpl(HavokBinaryWriter writer, HavokType type, object value,
        BinarySerializeContext context)
    {
        switch (type.Name)
        {
            case "hkArray":
                WriteHkArray(writer, type, value, context);
                break;
            case "hkRelArray":
                WriteHkRelArray(writer, type, value, context);
                break;
            case "T[N]":
                WriteArray(writer, type, value, context);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private static void PopulateListFromPointer(HavokBinaryReader reader, List<T> list, ulong pointer,
        BinaryDeserializeContext context)
    {
        object[] elements = context.GetItem(pointer, reader);
        foreach (object element in elements)
        {
            if (element is not (null or T))
                throw new InvalidOperationException(
                    $"Invalid element type. Expected: {typeof(T)} | Received: {element.GetType()}");
            list.Add((T)element!);
        }
    }

    private static object ReadHkRelArray(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        List<T> list = new();
        // serialized as a single item pointer
        ulong pointer = reader.ReadUInt32();
        if (pointer != 0)
        {
            PopulateListFromPointer(reader, list, pointer, context);
        }

        return list;
    }

    private static void WriteHkRelArray(HavokBinaryWriter writer, HavokType type, object? value,
        BinarySerializeContext context)
    {
        if (value is null)
        {
            writer.WriteUInt32(0);
            return;
        }

        List<T> list = (List<T>)value;
        if (list.Count == 0)
        {
            writer.WriteUInt32(0);
            return;
        }

        ulong pointer = EnqueueList(writer, type, context, list);
        writer.WriteUInt32((uint)pointer);
    }

    private static object ReadHkArray(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        List<T> list = new();

        ulong pointer = reader.ReadUInt64();
        // sizeAndFlags
        reader.AssertUInt64(0);

        if (pointer != 0)
        {
            PopulateListFromPointer(reader, list, pointer, context);
        }

        return list;
    }

    private static void WriteHkArray(HavokBinaryWriter writer, HavokType type, object? value,
        BinarySerializeContext context)
    {
        if (value is null)
        {
            writer.WriteUInt64(0);
            writer.WriteUInt64(0);
            return;
        }

        List<T> list = (List<T>)value;
        if (list.Count == 0)
        {
            writer.WriteUInt64(0);
            writer.WriteUInt64(0);
            return;
        }

        ulong pointer = EnqueueList(writer, type, context, list);

        context.RegisterPatch(type, writer.Position);
        writer.WriteUInt64(pointer);
        writer.WriteUInt64(0);
    }

    private static ulong EnqueueList(HavokBinaryWriter writer, HavokType type, BinarySerializeContext context,
        List<T> list)
    {
        object[] objects = new object[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            objects[i] = list[i]!;
        }

        ulong pointer = context.Enqueue(type.SubType!, writer.Position, objects, list);
        return pointer;
    }

    private static object ReadArray(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        if (type.SubType!.Name == "T[N]")
        {
            return Read2dArray(reader, type, context);
        }

        int size = type.Format >> 8;
        T[] array = new T[size];
        for (int i = 0; i < size; i++)
        {
            dynamic d = FormatHandler.Read(reader, type.SubType!, context);
            T obj = (T)d;
            array[i] = obj;
        }

        return array;
    }

    private static void WriteArray(HavokBinaryWriter writer, HavokType type, object value,
        BinarySerializeContext context)
    {
        if (type.SubType!.Name == "T[N]")
        {
            Write2dArray(writer, type, value, context);
            return;
        }

        T[] array = (T[])value;

        foreach (T element in array)
        {
            FormatHandler.Write(writer, type.SubType!, element!, context);
        }
    }

    private static object Read2dArray(HavokBinaryReader reader, HavokType type, BinaryDeserializeContext context)
    {
        int length1 = type.Format >> 8;
        int length2 = type.SubType!.Format >> 8;
        T[,] array = new T[length1, length2];
        for (int i = 0; i < length1; i++)
        {
            for (int j = 0; j < length2; j++)
            {
                T obj = (T)FormatHandler.Read(reader, type.SubType!.SubType!, context);
                array[i, j] = obj;
            }
        }

        return array;
    }

    private static void Write2dArray(HavokBinaryWriter writer, HavokType type, object value,
        BinarySerializeContext context)
    {
        T[,] array = (T[,])value;

        foreach (T element in array)
        {
            FormatHandler.Write(writer, type.SubType!.SubType!, element!, context);
        }
    }
}