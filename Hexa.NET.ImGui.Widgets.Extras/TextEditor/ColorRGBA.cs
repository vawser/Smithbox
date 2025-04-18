namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    using System.Numerics;
    using System.Xml.Serialization;

    [XmlRoot("Color")]
    public struct ColorRGBA
    {
        [XmlAttribute("r")]
        public float R;

        [XmlAttribute("g")]
        public float G;

        [XmlAttribute("b")]
        public float B;

        [XmlAttribute("a")]
        public float A;

        public ColorRGBA(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public ColorRGBA(Vector4 color)
        {
            R = color.X;
            G = color.Y;
            B = color.Z;
            A = color.W;
        }

        public ColorRGBA(uint color)
        {
            R = (color >> 24 & 0xff) / (float)byte.MaxValue;
            G = (color >> 16 & 0xff) / (float)byte.MaxValue;
            B = (color >> 8 & 0xff) / (float)byte.MaxValue;
            A = (color & 0xff) / (float)byte.MaxValue;
        }

        public static implicit operator Vector4(ColorRGBA c)
        {
            return new(c.R, c.G, c.B, c.A);
        }

        public static implicit operator ColorRGBA(uint color)
        {
            return new(color);
        }
    }
}