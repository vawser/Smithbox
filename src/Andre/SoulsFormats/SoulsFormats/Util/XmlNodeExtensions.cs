using System;
using System.IO;
using System.Xml;

namespace SoulsFormats.XmlExtensions
{
    public static class XmlNodeExtensions
    {
        private static T ReadT<T>(XmlNode node, string xpath, Func<string, T> parse)
        {
            XmlNode child = node.SelectSingleNode(xpath);
            if (child == null)
                throw new InvalidDataException($"Missing element: {xpath}");

            return parse(child.InnerText);
        }

        private static T ReadT<T>(XmlNode node, string xpath, IFormatProvider provider, Func<string, IFormatProvider, T> parse)
        {
            XmlNode child = node.SelectSingleNode(xpath);
            if (child == null)
                throw new InvalidDataException($"Missing element: {xpath}");

            return parse(child.InnerText, provider);
        }

        private static T ReadTAttribute<T>(XmlNode node, string xpath, Func<string, T> parse)
        {
            XmlAttribute child = node.Attributes[xpath];
            if (child == null)
                throw new InvalidDataException($"Missing element: {xpath}");

            return parse(child.InnerText);
        }

        private static T? ReadTIfExist<T>(XmlNode node, string xpath, Func<string, T> parse) where T : struct
        {
            XmlNode child = node.SelectSingleNode(xpath);
            return child == null ? null : (T?)parse(child.InnerText);
        }

        private static T? ReadTIfExist<T>(XmlNode node, string xpath, IFormatProvider provider, Func<string, IFormatProvider, T> parse) where T : struct
        {
            XmlNode child = node.SelectSingleNode(xpath);
            return child == null ? null : (T?)parse(child.InnerText, provider);
        }

        private static T ReadTOrDefault<T>(this XmlNode node, string xpath, T def, Func<string, T> parse)
        {
            XmlNode child = node.SelectSingleNode(xpath);
            return child == null ? def : parse(child.InnerText);
        }

        private static T ReadTOrDefault<T>(this XmlNode node, string xpath, T def, IFormatProvider provider, Func<string, IFormatProvider, T> parse)
        {
            XmlNode child = node.SelectSingleNode(xpath);
            return child == null ? def : parse(child.InnerText, provider);
        }


        public static bool ReadBoolean(this XmlNode node, string xpath)
            => ReadT(node, xpath, bool.Parse);

        public static bool? ReadBooleanIfExist(this XmlNode node, string xpath)
            => ReadTIfExist(node, xpath, bool.Parse);

        public static bool ReadBooleanOrDefault(this XmlNode node, string xpath, bool def)
            => ReadTOrDefault(node, xpath, def, bool.Parse);


        public static sbyte ReadSByte(this XmlNode node, string xpath)
            => ReadT(node, xpath, sbyte.Parse);

        public static sbyte? ReadSByteIfExist(this XmlNode node, string xpath)
            => ReadTIfExist(node, xpath, sbyte.Parse);

        public static sbyte ReadSByteOrDefault(this XmlNode node, string xpath, sbyte def)
            => ReadTOrDefault(node, xpath, def, sbyte.Parse);


        public static byte ReadByte(this XmlNode node, string xpath)
            => ReadT(node, xpath, byte.Parse);

        public static byte? ReadByteIfExist(this XmlNode node, string xpath)
            => ReadTIfExist(node, xpath, byte.Parse);

        public static byte ReadByteOrDefault(this XmlNode node, string xpath, byte def)
            => ReadTOrDefault(node, xpath, def, byte.Parse);


        public static short ReadInt16(this XmlNode node, string xpath)
            => ReadT(node, xpath, short.Parse);

        public static short? ReadInt16IfExist(this XmlNode node, string xpath)
            => ReadTIfExist(node, xpath, short.Parse);

        public static short ReadInt16OrDefault(this XmlNode node, string xpath, short def)
            => ReadTOrDefault(node, xpath, def, short.Parse);


        public static ushort ReadUInt16(this XmlNode node, string xpath)
            => ReadT(node, xpath, ushort.Parse);

        public static ushort? ReadUInt16IfExist(this XmlNode node, string xpath)
            => ReadTIfExist(node, xpath, ushort.Parse);

        public static ushort ReadUInt16OrDefault(this XmlNode node, string xpath, ushort def)
            => ReadTOrDefault(node, xpath, def, ushort.Parse);


        public static int ReadInt32(this XmlNode node, string xpath)
            => ReadT(node, xpath, int.Parse);

        public static int ReadInt32Attribute(this XmlNode node, string xpath)
            => ReadTAttribute(node, xpath, int.Parse);

        public static int? ReadInt32IfExist(this XmlNode node, string xpath)
            => ReadTIfExist(node, xpath, int.Parse);

        public static int ReadInt32OrDefault(this XmlNode node, string xpath, int def)
            => ReadTOrDefault(node, xpath, def, int.Parse);


        public static uint ReadUInt32(this XmlNode node, string xpath)
            => ReadT(node, xpath, uint.Parse);

        public static uint ReadUInt32Attribute(this XmlNode node, string xpath)
            => ReadTAttribute(node, xpath, uint.Parse);

        public static uint ReadUInt32HexAttribute(this XmlNode node, string xpath)
            => ReadTAttribute(node, xpath, (x) => Convert.ToUInt32(x, 16));

        public static uint? ReadUInt32IfExist(this XmlNode node, string xpath)
            => ReadTIfExist(node, xpath, uint.Parse);

        public static uint ReadUInt32OrDefault(this XmlNode node, string xpath, uint def)
            => ReadTOrDefault(node, xpath, def, uint.Parse);


        public static long ReadInt64(this XmlNode node, string xpath)
            => ReadT(node, xpath, long.Parse);

        public static long? ReadInt64IfExist(this XmlNode node, string xpath)
            => ReadTIfExist(node, xpath, long.Parse);

        public static long ReadInt64OrDefault(this XmlNode node, string xpath, long def)
            => ReadTOrDefault(node, xpath, def, long.Parse);


        public static ulong ReadUInt64(this XmlNode node, string xpath)
            => ReadT(node, xpath, ulong.Parse);

        public static ulong ReadUInt64Attribute(this XmlNode node, string xpath)
            => ReadTAttribute(node, xpath, ulong.Parse);

        public static ulong? ReadUInt64IfExist(this XmlNode node, string xpath)
            => ReadTIfExist(node, xpath, ulong.Parse);

        public static ulong ReadUInt64OrDefault(this XmlNode node, string xpath, ulong def)
            => ReadTOrDefault(node, xpath, def, ulong.Parse);


        public static float ReadSingle(this XmlNode node, string xpath)
            => ReadT(node, xpath, float.Parse);

        public static float ReadSingle(this XmlNode node, string xpath, IFormatProvider provider)
            => ReadT(node, xpath, provider, float.Parse);

        public static float? ReadSingleIfExist(this XmlNode node, string xpath)
            => ReadTIfExist(node, xpath, float.Parse);

        public static float? ReadSingleIfExist(this XmlNode node, string xpath, IFormatProvider provider)
            => ReadTIfExist(node, xpath, provider, float.Parse);

        public static float ReadSingleOrDefault(this XmlNode node, string xpath, float def)
            => ReadTOrDefault(node, xpath, def, float.Parse);

        public static float ReadSingleOrDefault(this XmlNode node, string xpath, float def, IFormatProvider provider)
            => ReadTOrDefault(node, xpath, def, provider, float.Parse);


        public static double ReadDouble(this XmlNode node, string xpath)
            => ReadT(node, xpath, double.Parse);

        public static double ReadDouble(this XmlNode node, string xpath, IFormatProvider provider)
            => ReadT(node, xpath, provider, double.Parse);

        public static double? ReadDoubleIfExist(this XmlNode node, string xpath)
            => ReadTIfExist(node, xpath, double.Parse);

        public static double? ReadDoubleIfExist(this XmlNode node, string xpath, IFormatProvider provider)
            => ReadTIfExist(node, xpath, provider, double.Parse);

        public static double ReadDoubleOrDefault(this XmlNode node, string xpath, double def)
            => ReadTOrDefault(node, xpath, def, double.Parse);

        public static double ReadDoubleOrDefault(this XmlNode node, string xpath, double def, IFormatProvider provider)
            => ReadTOrDefault(node, xpath, def, provider, double.Parse);


        public static string ReadString(this XmlNode node, string xpath)
        {
            XmlNode child = node.SelectSingleNode(xpath);
            if (child == null)
                throw new InvalidDataException($"Missing element: {xpath}");

            return child.InnerText;
        }

        public static string ReadStringAttribute(this XmlNode node, string xpath)
        {
            XmlAttribute child = node.Attributes[xpath];
            if (child == null)
            {
                throw new InvalidDataException($"Missing element: {xpath}");
            }

            return child.InnerText;
        }

        public static string ReadStringAttributeOrDefault(this XmlNode node, string xpath)
        {
            XmlAttribute child = node.Attributes[xpath];
            if (child == null)
            {
                return null;
            }

            return child.InnerText;
        }

        public static string ReadStringIfExist(this XmlNode node, string xpath)
        {
            XmlNode child = node.SelectSingleNode(xpath);
            return child?.InnerText;
        }

        public static string ReadStringOrDefault(this XmlNode node, string xpath, string def)
        {
            XmlNode child = node.SelectSingleNode(xpath);
            return child?.InnerText ?? def;
        }
    }
}
