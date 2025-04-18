using System.Xml.Linq;
using HKLib.Reflection.hk2018;

namespace HKLib.Serialization.hk2018.Xml.FormatHandlers;

public static class IntFormatHandler
{
    private const int SignMask = 0x200;

    public static object Read(XElement element, HavokType type, XmlDeserializeContext context)
    {
        int format = type.Format;
        bool isSigned = (format & SignMask) != 0;


        if (element.Attribute("value")?.Value is not { } stringVal)
        {
            throw new InvalidDataException("Missing \"value\" attribute for integer value.");
        }

        object obj;
        if (isSigned)
        {
            if (!long.TryParse(stringVal, out long longVal))
            {
                throw new InvalidDataException($"Invalid signed integer value: {stringVal}");
            }

            obj = ((format >> 10) / 8) switch
            {
                1 => (sbyte)longVal,
                2 => (short)longVal,
                4 => (int)longVal,
                8 => (object)longVal,
                _ => throw new InvalidDataException("Unexpected int format")
            };
        }
        else
        {
            if (!ulong.TryParse(stringVal, out ulong ulongVal))
            {
                throw new InvalidDataException($"Invalid unsigned integer value: {stringVal}");
            }

            obj = ((format >> 10) / 8) switch
            {
                1 => (byte)ulongVal,
                2 => (ushort)ulongVal,
                4 => (uint)ulongVal,
                8 => (object)ulongVal,
                _ => throw new InvalidDataException("Unexpected int format")
            };
        }

        return obj;
    }

    public static void Write(XElement parentElement, HavokType type, dynamic value, XmlSerializeContext context)
    {
        bool isSigned = (type.Format & SignMask) != 0;
        object intVal = isSigned ? (long)value : (ulong)value;
        parentElement.Add(new XElement("integer", new XAttribute("value", intVal)));

        if (type.Presets.Count > 0)
        {
            parentElement.Add(new XComment($" {value} "));
        }
    }
}