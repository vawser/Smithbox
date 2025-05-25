using System.Xml.Linq;
using HKLib.Reflection.hk2018;

namespace HKLib.Serialization.hk2018.Xml.FormatHandlers;

public static class FormatHandler
{
    private static readonly Dictionary<HavokType.TypeKind, (ReadFormat Read, WriteFormat Write)> FormatHandlers = new()
    {
        { HavokType.TypeKind.Bool, (BoolFormatHandler.Read, BoolFormatHandler.Write) },
        { HavokType.TypeKind.String, (StringFormatHandler.Read, StringFormatHandler.Write) },
        { HavokType.TypeKind.Int, (IntFormatHandler.Read, IntFormatHandler.Write) },
        { HavokType.TypeKind.Float, (FloatFormatHandler.Read, FloatFormatHandler.Write) },
        { HavokType.TypeKind.Pointer, (PointerFormatHandler.Read, PointerFormatHandler.Write) },
        { HavokType.TypeKind.Record, (RecordFormatHandler.Read, RecordFormatHandler.Write) },
        { HavokType.TypeKind.Array, (ArrayFormatHandler.Read, ArrayFormatHandler.Write) },
    };

    public static object Read(XElement element, HavokType type, XmlDeserializeContext context)
    {
        if (type.Kind is HavokType.TypeKind.Void or HavokType.TypeKind.Opaque) throw new NotImplementedException();
        return FormatHandlers[type.Kind].Read(element, type, context);
    }

    public static void Write(XElement parentElement, HavokType type, object value,
        XmlSerializeContext context)
    {
        FormatHandlers[type.Kind].Write(parentElement, type, value, context);
    }

    public static HavokType GetActualType(object value, HavokTypeRegistry typeRegistry)
    {
        HavokType? actualType = typeRegistry.GetType(value.GetType());
        if (actualType is null)
        {
            throw new ArgumentException(
                "There is no HavokType corresponding to the given value in the current type registry",
                nameof(value));
        }

        return actualType;
    }

    public static string GetXmlName(HavokType type)
    {
        return type.Identity.Replace("<", "< ").Replace(">", " >");
    }

    private delegate object ReadFormat(XElement element, HavokType type, XmlDeserializeContext context);

    private delegate void WriteFormat(XElement parentElement, HavokType type, object value,
        XmlSerializeContext context);
}