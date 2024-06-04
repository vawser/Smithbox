using System.Numerics;
using System.Xml.Linq;
using HKLib.Reflection.hk2018;
using HKLib.ValueTypes;

namespace HKLib.Serialization.hk2018.Xml.FormatHandlers;

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

    public static object Read(XElement element, HavokType type, XmlDeserializeContext context)
    {
        if (type.Identity == "hkPropertyBag")
        {
            return ReadHkPropertyBag(element, type, context);
        }

        if (type.Type == typeof(object))
        {
            return ReadHkReflectAny(element, type, context);
        }

        if (type.Type == typeof(Vector4))
        {
            return ReadVector4(element, type, context);
        }

        if (type.Type == typeof(Quaternion))
        {
            return ReadQuaternion(element, type, context);
        }

        if (type.Type == typeof(Matrix4x4))
        {
            return ReadMatrix4X4(element, type, context);
        }

        if (type.Type == typeof(Matrix3x3))
        {
            return ReadMatrix3X3(element, type, context);
        }

        Type elementType = type.SubType!.Name == "T[N]" ? type.SubType!.SubType!.Type! : type.SubType!.Type!;
        return GetFormatHandler(elementType).ReadValueImpl(element, type, context);
    }

    public static void Write(XElement parentElement, HavokType type, object value, XmlSerializeContext context)
    {
        if (type.Identity == "hkPropertyBag")
        {
            WriteHkPropertyBag(parentElement, type, value, context);
            return;
        }

        if (type.Type == typeof(object))
        {
            WriteHkReflectAny(parentElement, type, value, context);
            return;
        }

        if (type.Type == typeof(Vector4))
        {
            WriteVector4(parentElement, type, value, context);
            return;
        }

        if (type.Type == typeof(Quaternion))
        {
            WriteQuaternion(parentElement, type, value, context);
            return;
        }

        if (type.Type == typeof(Matrix4x4))
        {
            WriteMatrix4X4(parentElement, type, value, context);
            return;
        }

        if (type.Type == typeof(Matrix3x3))
        {
            WriteMatrix3X3(parentElement, type, value, context);
            return;
        }

        Type elementType = type.SubType!.Name == "T[N]" ? type.SubType!.SubType!.Type! : type.SubType!.Type!;
        GetFormatHandler(elementType).WriteValueImpl(parentElement, type, value, context);
    }

    protected abstract object ReadValueImpl(XElement element, HavokType type, XmlDeserializeContext context);

    protected abstract void WriteValueImpl(XElement parentElement, HavokType type, object value,
        XmlSerializeContext context);

    private static object ReadHkPropertyBag(XElement element, HavokType type, XmlDeserializeContext context)
    {
        return type.Instantiate();
    }

    private static void WriteHkPropertyBag(XElement parentElement, HavokType type, object value,
        XmlSerializeContext context)
    {
        HavokType subType = GetSubType(type);
        parentElement.Add(CreateElement(subType, 0, context));
    }

    private static object ReadHkReflectAny(XElement element, HavokType type, XmlDeserializeContext context)
    {
        XElement valueElement = element.Elements().Single();
        if (valueElement.Attribute("elementtypeid")?.Value is not { } elementTypeId)
        {
            throw new InvalidDataException("Missing \"elementtypeid\" attribute on array value.");
        }

        HavokType elementType = context.GetType(elementTypeId);
        return FormatHandler.Read(valueElement, elementType, context);
    }

    private static void WriteHkReflectAny(XElement parentElement, HavokType type, object value,
        XmlSerializeContext context)
    {
        HavokType subType = FormatHandler.GetActualType(value, context.TypeRegistry);
        XElement element = CreateElement(subType, 1, context);
        FormatHandler.Write(element, subType, value, context);
        parentElement.Add(element);
    }

    private static Vector4 ReadVector4(XElement element, HavokType type, XmlDeserializeContext context)
    {
        HavokType subType = GetSubType(type);
        XElement[] components = element.Elements().ToArray();
        float x = (float)FormatHandler.Read(components[0], subType, context);
        float y = (float)FormatHandler.Read(components[1], subType, context);
        float z = (float)FormatHandler.Read(components[2], subType, context);
        float w = (float)FormatHandler.Read(components[3], subType, context);

        return new Vector4(x, y, z, w);
    }

    private static void WriteVector4(XElement parentElement, HavokType type, object value, XmlSerializeContext context)
    {
        Vector4 vectorValue = (Vector4)value;
        HavokType subType = GetSubType(type);
        XElement element = CreateElement(subType, 4, context);

        FormatHandler.Write(element, subType, vectorValue.X, context);
        FormatHandler.Write(element, subType, vectorValue.Y, context);
        FormatHandler.Write(element, subType, vectorValue.Z, context);
        FormatHandler.Write(element, subType, vectorValue.W, context);
        parentElement.Add(element);
    }

    private static object ReadQuaternion(XElement element, HavokType type, XmlDeserializeContext context)
    {
        Vector4 vector = ReadVector4(element, type, context);
        return new Quaternion(vector.X, vector.Y, vector.Z, vector.W);
    }

    private static void WriteQuaternion(XElement parentElement, HavokType type, object value,
        XmlSerializeContext context)
    {
        Quaternion quaternionValue = (Quaternion)value;
        Vector4 vector = new(quaternionValue.X, quaternionValue.Y, quaternionValue.Z, quaternionValue.W);
        WriteVector4(parentElement, type, vector, context);
    }

    private static object ReadMatrix4X4(XElement element, HavokType type, XmlDeserializeContext context)
    {
        HavokType subType = GetSubType(type);
        List<XElement> values = element.Elements().ToList();

        Matrix4x4 matrix = new();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                matrix[i, j] = (float)FormatHandler.Read(values[i * 4 + j], subType, context);
            }
        }

        return matrix;
    }

    private static void WriteMatrix4X4(XElement parentElement, HavokType type, object value,
        XmlSerializeContext context)
    {
        Matrix4x4 matrixValue = (Matrix4x4)value;
        HavokType subType = GetSubType(type);
        XElement element = CreateElement(subType, 16, context);

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                FormatHandler.Write(element, subType, matrixValue[i, j], context);
            }
        }

        parentElement.Add(element);
    }

    private static object ReadMatrix3X3(XElement element, HavokType type, XmlDeserializeContext context)
    {
        HavokType subType = GetSubType(type);
        List<XElement> values = element.Elements().ToList();

        Matrix4x4 matrix = new();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                matrix[i, j] = (float)FormatHandler.Read(values[i * 4 + j], subType, context);
            }
        }

        return (Matrix3x3)matrix;
    }

    private static void WriteMatrix3X3(XElement parentElement, HavokType type, object value,
        XmlSerializeContext context)
    {
        Matrix4x4 matrixValue = (Matrix3x3)value;
        HavokType subType = GetSubType(type);
        XElement element = CreateElement(subType, 12, context);

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                FormatHandler.Write(element, subType, matrixValue[i, j], context);
            }
        }

        parentElement.Add(element);
    }

    private static HavokType GetSubType(HavokType type)
    {
        while (type.SubType is null && type.Parent is not null)
        {
            type = type.Parent;
        }

        return type.SubType!;
    }

    protected static XElement CreateElement(HavokType elementType, int count, XmlSerializeContext context)
    {
        string elementTypeId = context.GetTypeId(elementType);
        string subTypeName = FormatHandler.GetXmlName(elementType);
        return new XElement("array",
            new XAttribute("count", count),
            new XAttribute("elementtypeid", elementTypeId),
            new XComment($" ArrayOf {subTypeName} "));
    }
}

internal class ArrayFormatHandler<T> : ArrayFormatHandler
{
    protected override object ReadValueImpl(XElement element, HavokType type, XmlDeserializeContext context)
    {
        return type.Name switch
        {
            "hkArray" or "hkRelArray" => ReadList(element, type, context),
            "T[N]" => ReadArray(element, type, context),
            _ => throw new NotImplementedException()
        };
    }

    protected override void WriteValueImpl(XElement parentElement, HavokType type, object value,
        XmlSerializeContext context)
    {
        switch (type.Name)
        {
            case "hkArray":
            case "hkRelArray":
                WriteList(parentElement, type, value, context);
                break;
            case "T[N]":
                WriteArray(parentElement, type, value, context);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private static object ReadList(XElement element, HavokType type, XmlDeserializeContext context)
    {
        List<T> list = new();
        foreach (XElement item in element.Elements())
        {
            list.Add((T)FormatHandler.Read(item, type.SubType!, context));
        }

        return list;
    }

    private static void WriteList(XElement parentElement, HavokType type, object? value,
        XmlSerializeContext context)
    {
        HavokType subType = type.SubType!;
        if (value is null)
        {
            parentElement.Add(CreateElement(subType, 0, context));
            return;
        }

        List<T> list = (List<T>)value;
        XElement element = CreateElement(subType, list.Count, context);
        foreach (T item in list)
        {
            FormatHandler.Write(element, subType, item!, context);
        }

        parentElement.Add(element);
    }

    private static object ReadArray(XElement element, HavokType type, XmlDeserializeContext context)
    {
        HavokType subType = type.SubType!;
        if (subType.Name == "T[N]")
        {
            return Read2dArray(element, type, context);
        }

        XElement[] items = element.Elements().ToArray();

        int size = type.Format >> 8;
        T[] array = new T[size];
        for (int i = 0; i < size; i++)
        {
            T obj = (T)FormatHandler.Read(items[i], subType, context);
            array[i] = obj;
        }

        return array;
    }

    private static void WriteArray(XElement parentElement, HavokType type, object value, XmlSerializeContext context)
    {
        HavokType subType = type.SubType!;
        if (subType.Name == "T[N]")
        {
            Write2dArray(parentElement, type, value, context);
            return;
        }

        T[] array = (T[])value;
        XElement element = CreateElement(subType, array.Length, context);
        foreach (T item in array)
        {
            FormatHandler.Write(element, subType, item!, context);
        }

        parentElement.Add(element);
    }

    private static object Read2dArray(XElement element, HavokType type, XmlDeserializeContext context)
    {
        XElement[] items = element.Elements().ToArray();

        int length1 = type.Format >> 8;
        int length2 = type.SubType!.Format >> 8;
        T[,] array = new T[length1, length2];
        for (int i = 0; i < length1; i++)
        {
            for (int j = 0; j < length2; j++)
            {
                int index = i * length2 + j;
                array[i, j] = (T)FormatHandler.Read(items[index], type.SubType!.SubType!, context);
            }
        }

        return array;
    }

    private static void Write2dArray(XElement parentElement, HavokType type, object value, XmlSerializeContext context)
    {
        HavokType subType = type.SubType!.SubType!;
        T[,] array = (T[,])value;
        XElement element = CreateElement(subType, array.Length, context);

        foreach (T item in array)
        {
            FormatHandler.Write(element, subType, item!, context);
        }

        parentElement.Add(element);
    }
}