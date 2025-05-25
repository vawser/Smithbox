using System.Globalization;
using System.Runtime.Intrinsics;
using System.Xml.Linq;
using HKLib.Reflection.hk2018;

namespace HKLib.Serialization.hk2018.Xml.FormatHandlers;

public static class FloatFormatHandler
{
    private const int EndianMask = 0x100;

    public static object Read(XElement element, HavokType type, XmlDeserializeContext context)
    {
        if (element.Attribute("hex")?.Value is not { } stringVal)
        {
            throw new InvalidDataException("Missing \"hex\" attribute for real value.");
        }

        if (!ulong.TryParse(stringVal.AsSpan()[1..], NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out ulong ulongVal))
        {
            throw new InvalidDataException($"Invalid double value: {stringVal}");
        }

        Span<byte> bytes = stackalloc byte[8];
        BitConverter.TryWriteBytes(bytes, ulongVal);

        int format = type.Format;
        bool bigEndian = (format & EndianMask) != 0;
        if (bigEndian)
        {
            bytes.Reverse();
        }

        double doubleVal = BitConverter.ToDouble(bytes);
        return (type.Size, format >> 16) switch
        {
            (2, _) => (float)doubleVal,
            (4, _) => (float)doubleVal,
            (8, _) => doubleVal,
            (16, 23) => Vector128.Create((float)doubleVal),
            (16, 52) => Vector128.Create(doubleVal),
            _ => throw new InvalidDataException("Unexpected float format")
        };
    }

    public static void Write(XElement parentElement, HavokType type, object value, XmlSerializeContext context)
    {
        int format = type.Format;
        double doubleVal = (type.Size, format >> 16) switch
        {
            (2, _) => (float)value,
            (4, _) => (float)value,
            (8, _) => (double)value,
            (16, 23) => ((Vector128<float>)value)[0],
            (16, 52) => ((Vector128<double>)value)[0],
            _ => throw new ArgumentException("Invalid float format", nameof(type))
        };
        Span<byte> bytes = stackalloc byte[8];
        BitConverter.TryWriteBytes(bytes, doubleVal);

        bool bigEndian = (format & EndianMask) != 0;
        if (bigEndian)
        {
            bytes.Reverse();
        }

        ulong hexVal = BitConverter.ToUInt64(bytes);
        parentElement.Add(new XElement("real", new XAttribute("dec", doubleVal.ToString("g6")),
            new XAttribute("hex", '#' + hexVal.ToString("x"))));
    }
}