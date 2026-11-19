using System;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class GFX
    {
        /// <summary>
        /// The pixel size of a twip, the SWF unit for positions and bounds.
        /// </summary>
        public const float TwipsPerPixel = 20f;

        /// <summary>
        /// SWF RECT: bounds in twips. Written with the bit width it was read with, unless a value no longer fits.
        /// </summary>
        public class Rect
        {
            /// <summary>Left edge, in twips.</summary>
            public int XMin { get; set; }

            /// <summary>Right edge, in twips.</summary>
            public int XMax { get; set; }

            /// <summary>Top edge, in twips.</summary>
            public int YMin { get; set; }

            /// <summary>Bottom edge, in twips.</summary>
            public int YMax { get; set; }

            /// <summary>The bit width read from the file; values are written with at least this many bits.</summary>
            public int Bits { get; set; }

            /// <summary>Width in pixels.</summary>
            public float Width => (XMax - XMin) / TwipsPerPixel;

            /// <summary>Height in pixels.</summary>
            public float Height => (YMax - YMin) / TwipsPerPixel;

            internal static Rect Read(SwfReader r)
            {
                var rect = new Rect();
                rect.Bits = (int)r.UB(5);
                rect.XMin = r.SB(rect.Bits);
                rect.XMax = r.SB(rect.Bits);
                rect.YMin = r.SB(rect.Bits);
                rect.YMax = r.SB(rect.Bits);
                r.Align();
                return rect;
            }

            internal void Write(SwfWriter w)
            {
                int n = Math.Max(Bits, SignedBits(XMin, XMax, YMin, YMax));
                w.UB(5, (uint)n);
                w.SB(n, XMin);
                w.SB(n, XMax);
                w.SB(n, YMin);
                w.SB(n, YMax);
                w.Align();
            }

            /// <summary>A copy.</summary>
            public Rect Clone() => (Rect)MemberwiseClone();
        }

        /// <summary>
        /// SWF MATRIX: x' = ScaleX*x + RotateSkew1*y + TranslateX, y' = RotateSkew0*x + ScaleY*y + TranslateY.
        /// Scale and rotate/skew are 16.16 fixed point, translation is in twips. Each part is written with the bit width
        /// it was read with (Flash sometimes used more bits than needed), unless a value no longer fits, so an
        /// unchanged matrix writes back byte-equal.
        /// </summary>
        public class Matrix
        {
            /// <summary>Scale is present. Without it the scale is 1.</summary>
            public bool HasScale { get; set; }

            /// <summary>Horizontal scale, 16.16 fixed point.</summary>
            public int ScaleX { get; set; } = 0x10000;

            /// <summary>Vertical scale, 16.16 fixed point.</summary>
            public int ScaleY { get; set; } = 0x10000;

            /// <summary>Rotate/skew is present. Without it both terms are 0.</summary>
            public bool HasRotate { get; set; }

            /// <summary>Rotate/skew term multiplying x into y', 16.16 fixed point.</summary>
            public int RotateSkew0 { get; set; }

            /// <summary>Rotate/skew term multiplying y into x', 16.16 fixed point.</summary>
            public int RotateSkew1 { get; set; }

            /// <summary>Horizontal translation, in twips.</summary>
            public int TranslateX { get; set; }

            /// <summary>Vertical translation, in twips.</summary>
            public int TranslateY { get; set; }

            /// <summary>Bit width of the scale values in the file.</summary>
            public int ScaleBits { get; set; }

            /// <summary>Bit width of the rotate/skew values in the file.</summary>
            public int RotateBits { get; set; }

            /// <summary>Bit width of the translation values in the file.</summary>
            public int TranslateBits { get; set; }

            internal static Matrix Read(SwfReader r)
            {
                var m = new Matrix();
                m.HasScale = r.UB(1) == 1;
                if (m.HasScale)
                {
                    m.ScaleBits = (int)r.UB(5);
                    m.ScaleX = r.SB(m.ScaleBits);
                    m.ScaleY = r.SB(m.ScaleBits);
                }
                m.HasRotate = r.UB(1) == 1;
                if (m.HasRotate)
                {
                    m.RotateBits = (int)r.UB(5);
                    m.RotateSkew0 = r.SB(m.RotateBits);
                    m.RotateSkew1 = r.SB(m.RotateBits);
                }
                m.TranslateBits = (int)r.UB(5);
                m.TranslateX = r.SB(m.TranslateBits);
                m.TranslateY = r.SB(m.TranslateBits);
                r.Align();
                return m;
            }

            internal void Write(SwfWriter w)
            {
                w.UB(1, HasScale ? 1u : 0u);
                if (HasScale)
                {
                    int n = Math.Max(ScaleBits, SignedBits(ScaleX, ScaleY));
                    w.UB(5, (uint)n);
                    w.SB(n, ScaleX);
                    w.SB(n, ScaleY);
                }
                w.UB(1, HasRotate ? 1u : 0u);
                if (HasRotate)
                {
                    int n = Math.Max(RotateBits, SignedBits(RotateSkew0, RotateSkew1));
                    w.UB(5, (uint)n);
                    w.SB(n, RotateSkew0);
                    w.SB(n, RotateSkew1);
                }
                int t = Math.Max(TranslateBits, SignedBits(TranslateX, TranslateY));
                w.UB(5, (uint)t);
                w.SB(t, TranslateX);
                w.SB(t, TranslateY);
                w.Align();
            }

            /// <summary>
            /// The matrix in pixels, as a row-vector System.Numerics matrix (point * matrix).
            /// </summary>
            public Matrix3x2 ToMatrix3x2()
            {
                float sx = HasScale ? ScaleX / 65536f : 1f;
                float sy = HasScale ? ScaleY / 65536f : 1f;
                float r0 = HasRotate ? RotateSkew0 / 65536f : 0f;
                float r1 = HasRotate ? RotateSkew1 / 65536f : 0f;
                return new Matrix3x2(sx, r0, r1, sy, TranslateX / TwipsPerPixel, TranslateY / TwipsPerPixel);
            }

            /// <summary>
            /// Sets the matrix from pixels (row-vector, see <see cref="ToMatrix3x2"/>). Keeps HasScale/HasRotate off when
            /// the identity values allow it, and keeps the bit widths as minimums.
            /// </summary>
            public void FromMatrix3x2(Matrix3x2 m)
            {
                ScaleX = Fixed(m.M11);
                ScaleY = Fixed(m.M22);
                RotateSkew0 = Fixed(m.M12);
                RotateSkew1 = Fixed(m.M21);
                HasScale = HasScale || ScaleX != 0x10000 || ScaleY != 0x10000;
                HasRotate = HasRotate || RotateSkew0 != 0 || RotateSkew1 != 0;
                TranslateX = (int)MathF.Round(m.M31 * TwipsPerPixel);
                TranslateY = (int)MathF.Round(m.M32 * TwipsPerPixel);
            }

            private static int Fixed(float v) => (int)MathF.Round(v * 65536f);

            /// <summary>Moves the matrix by a number of twips.</summary>
            public void Translate(int dxTwips, int dyTwips)
            {
                TranslateX += dxTwips;
                TranslateY += dyTwips;
            }

            /// <summary>A copy.</summary>
            public Matrix Clone() => (Matrix)MemberwiseClone();
        }

        /// <summary>
        /// SWF CXFORMWITHALPHA: colour = colour * Mult / 256 + Add, per RGBA channel.
        /// </summary>
        public class ColorTransform
        {
            /// <summary>The multiply terms are present. Without them they are 256 (1.0).</summary>
            public bool HasMult { get; set; }

            /// <summary>Red multiply, 8.8 fixed point.</summary>
            public int RedMult { get; set; } = 256;

            /// <summary>Green multiply, 8.8 fixed point.</summary>
            public int GreenMult { get; set; } = 256;

            /// <summary>Blue multiply, 8.8 fixed point.</summary>
            public int BlueMult { get; set; } = 256;

            /// <summary>Alpha multiply, 8.8 fixed point.</summary>
            public int AlphaMult { get; set; } = 256;

            /// <summary>The add terms are present. Without them they are 0.</summary>
            public bool HasAdd { get; set; }

            /// <summary>Red add, -255 to 255.</summary>
            public int RedAdd { get; set; }

            /// <summary>Green add, -255 to 255.</summary>
            public int GreenAdd { get; set; }

            /// <summary>Blue add, -255 to 255.</summary>
            public int BlueAdd { get; set; }

            /// <summary>Alpha add, -255 to 255.</summary>
            public int AlphaAdd { get; set; }

            /// <summary>The bit width in the file; values are written with at least this many bits.</summary>
            public int Bits { get; set; }

            internal static ColorTransform Read(SwfReader r)
            {
                var c = new ColorTransform();
                c.HasAdd = r.UB(1) == 1;
                c.HasMult = r.UB(1) == 1;
                c.Bits = (int)r.UB(4);
                if (c.HasMult)
                {
                    c.RedMult = r.SB(c.Bits);
                    c.GreenMult = r.SB(c.Bits);
                    c.BlueMult = r.SB(c.Bits);
                    c.AlphaMult = r.SB(c.Bits);
                }
                if (c.HasAdd)
                {
                    c.RedAdd = r.SB(c.Bits);
                    c.GreenAdd = r.SB(c.Bits);
                    c.BlueAdd = r.SB(c.Bits);
                    c.AlphaAdd = r.SB(c.Bits);
                }
                r.Align();
                return c;
            }

            internal void Write(SwfWriter w)
            {
                int n = Bits;
                if (HasMult)
                    n = Math.Max(n, SignedBits(RedMult, GreenMult, BlueMult, AlphaMult));
                if (HasAdd)
                    n = Math.Max(n, SignedBits(RedAdd, GreenAdd, BlueAdd, AlphaAdd));
                if (n > 15)
                    throw new InvalidDataException("Colour transform values need more than 15 bits.");
                w.UB(1, HasAdd ? 1u : 0u);
                w.UB(1, HasMult ? 1u : 0u);
                w.UB(4, (uint)n);
                if (HasMult)
                {
                    w.SB(n, RedMult);
                    w.SB(n, GreenMult);
                    w.SB(n, BlueMult);
                    w.SB(n, AlphaMult);
                }
                if (HasAdd)
                {
                    w.SB(n, RedAdd);
                    w.SB(n, GreenAdd);
                    w.SB(n, BlueAdd);
                    w.SB(n, AlphaAdd);
                }
                w.Align();
            }

            /// <summary>A copy.</summary>
            public ColorTransform Clone() => (ColorTransform)MemberwiseClone();
        }

        /// <summary>
        /// The kinds of PlaceObject3 surface filters.
        /// </summary>
        public enum FilterType : byte
        {
            /// <summary>Drop shadow.</summary>
            DropShadow = 0,
            /// <summary>Blur.</summary>
            Blur = 1,
            /// <summary>Glow.</summary>
            Glow = 2,
            /// <summary>Bevel.</summary>
            Bevel = 3,
            /// <summary>Gradient glow.</summary>
            GradientGlow = 4,
            /// <summary>Convolution.</summary>
            Convolution = 5,
            /// <summary>Colour matrix.</summary>
            ColorMatrix = 6,
            /// <summary>Gradient bevel.</summary>
            GradientBevel = 7,
        }

        /// <summary>
        /// A PlaceObject3 surface filter, kept as its raw body (the editor shows them but does not edit them yet).
        /// </summary>
        public class Filter
        {
            /// <summary>The filter kind.</summary>
            public FilterType Type { get; set; }

            /// <summary>The filter's fields after the type byte.</summary>
            public byte[] Data { get; set; }

            internal static Filter Read(SwfReader r)
            {
                var f = new Filter { Type = (FilterType)r.U8() };
                int size;
                switch (f.Type)
                {
                    case FilterType.DropShadow: size = 23; break;
                    case FilterType.Blur: size = 9; break;
                    case FilterType.Glow: size = 15; break;
                    case FilterType.Bevel: size = 27; break;
                    case FilterType.ColorMatrix: size = 80; break;
                    case FilterType.GradientGlow:
                    case FilterType.GradientBevel:
                        {
                            var colors = r.U8();
                            f.Data = new byte[1 + colors * 5 + 19];
                            f.Data[0] = colors;
                            Buffer.BlockCopy(r.Bytes(f.Data.Length - 1), 0, f.Data, 1, f.Data.Length - 1);
                            return f;
                        }
                    case FilterType.Convolution:
                        {
                            var x = r.U8();
                            var y = r.U8();
                            var rest = r.Bytes(8 + 4 * x * y + 5);
                            f.Data = new byte[2 + rest.Length];
                            f.Data[0] = x;
                            f.Data[1] = y;
                            Buffer.BlockCopy(rest, 0, f.Data, 2, rest.Length);
                            return f;
                        }
                    default: throw new InvalidDataException($"Unknown filter type {(int)f.Type}.");
                }
                f.Data = r.Bytes(size);
                return f;
            }

            internal void Write(SwfWriter w)
            {
                w.U8((byte)Type);
                w.Bytes(Data);
            }
        }
    }
}
