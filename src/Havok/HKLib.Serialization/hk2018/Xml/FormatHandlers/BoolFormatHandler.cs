using System.Xml.Linq;
using HKLib.Reflection.hk2018;

namespace HKLib.Serialization.hk2018.Xml.FormatHandlers;

public static class BoolFormatHandler
{
    public static object Read(XElement element, HavokType type, XmlDeserializeContext context)
    {
        if (element.Attribute("value")?.Value is not { } stringVal)
        {
            throw new InvalidDataException("Missing \"value\" attribute for boolean value.");
        }

        if (!bool.TryParse(stringVal, out bool value))
        {
            throw new InvalidDataException($"Invalid boolean value: {element.Attribute("value")?.Value ?? "null"}");
        }

        return value;
    }

    public static void Write(XElement parentElement, HavokType type, object value, XmlSerializeContext context)
    {
        bool boolVal = (bool)value;
        parentElement.Add(new XElement("bool", new XAttribute("value", boolVal)));
    }
}