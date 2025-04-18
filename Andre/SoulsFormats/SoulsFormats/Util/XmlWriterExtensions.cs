using System;
using System.Xml;

namespace SoulsFormats.XmlExtensions
{
    internal static class XmlWriterExtensions
    {
        public static void WriteDefaultElement<T>(this XmlWriter xw,
            string localName, T value, T defaultValue) where T : IEquatable<T>
        {
            if (!Equals(value, defaultValue))
            {
                xw.WriteElementString(localName, value.ToString());
            }
        }

        public static void WriteDefaultElement(this XmlWriter xw,
            string localName, float value, float defaultValue, IFormatProvider provider)
        {
            if (value != defaultValue)
            {
                xw.WriteElementString(localName, value.ToString(provider));
            }
        }

        public static void WriteDefaultElement(this XmlWriter xw,
            string localName, float value, float defaultValue, string format, IFormatProvider provider)
        {
            if (value != defaultValue)
            {
                xw.WriteElementString(localName, value.ToString(format, provider));
            }
        }
    }
}
