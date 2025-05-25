using System.Xml.Linq;
using HKLib.hk2018;
using HKLib.Reflection.hk2018;

namespace HKLib.Serialization.hk2018.Xml.FormatHandlers;

public static class PointerFormatHandler
{
    public static object Read(XElement element, HavokType type, XmlDeserializeContext context)
    {
        if (element.Attribute("id")?.Value is not { } id)
        {
            throw new InvalidDataException("Missing \"id\" attribute on pointer element.");
        }

        return id == "object0" ? null! : context.GetObject(id);
    }

    public static void Write(XElement parentElement, HavokType type, object? value, XmlSerializeContext context)
    {
        string pointer;
        if (value is null)
        {
            pointer = "object0";
        }
        else if (value is not IHavokObject havokObject)
        {
            throw new ArgumentException("Invalid pointer type. Only pointers to IHavokObjects are supported",
                nameof(value));
        }
        else
        {
            HavokType actualSubType = FormatHandler.GetActualType(value, context.TypeRegistry);
            pointer = context.Enqueue(actualSubType, havokObject);
        }

        parentElement.Add(new XElement("pointer", new XAttribute("id", pointer)));
    }
}