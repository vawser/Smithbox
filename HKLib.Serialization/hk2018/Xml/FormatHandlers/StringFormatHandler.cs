using System.Xml.Linq;
using HKLib.Reflection.hk2018;

namespace HKLib.Serialization.hk2018.Xml.FormatHandlers;

public static class StringFormatHandler
{
    public static object Read(XElement element, HavokType type, XmlDeserializeContext context)
    {
        return element.Attribute("value")?.Value ?? string.Empty;
    }

    public static void Write(XElement parentElement, HavokType type, object? value, XmlSerializeContext context)
    {
        string stringVal = (string?)value ?? string.Empty;
        parentElement.Add(new XElement("string", new XAttribute("value", stringVal)));
    }
}