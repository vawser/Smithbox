using System;
using System.Drawing;

namespace SoulsFormats
{
    public partial class GFX
    {
        /// <summary>
        /// The kind of a shape fill style.
        /// </summary>
        public enum FillKind
        {
            /// <summary>A solid colour.</summary>
            Solid,
            /// <summary>A linear, radial or focal gradient (Color is its first stop).</summary>
            Gradient,
            /// <summary>A bitmap (BitmapId, with BitmapMatrix mapping bitmap pixels to the shape, in twips).</summary>
            Bitmap,
            /// <summary>A fill type this reader does not know.</summary>
            Unknown,
        }

        /// <summary>
        /// A shape's first fill style, for a preview: most FromSoftware shapes are one rectangle with one fill.
        /// </summary>
        public class ShapeFill
        {
            /// <summary>The fill kind.</summary>
            public FillKind Kind { get; set; }

            /// <summary>The raw fill style type (0x00 solid, 0x10/0x12/0x13 gradients, 0x40–0x43 bitmaps).</summary>
            public byte Type { get; set; }

            /// <summary>Solid colour, or the first gradient stop.</summary>
            public Color Color { get; set; } = Color.White;

            /// <summary>The bitmap character, for bitmap fills.</summary>
            public ushort BitmapId { get; set; }

            /// <summary>Bitmap or gradient matrix (bitmap pixels or gradient square to shape twips).</summary>
            public Matrix Matrix { get; set; }

            /// <summary>The bitmap repeats (types 0x40, 0x42) rather than clamping.</summary>
            public bool Repeats => Type == 0x40 || Type == 0x42;
        }

        /// <summary>
        /// Reads the first fill style of a shape, or null when it has none (or its records can't be read).
        /// Only reads; the shape's records stay raw.
        /// </summary>
        public static ShapeFill ReadFirstFill(DefineShape shape)
        {
            try
            {
                if (shape.Records == null || shape.Records.Length == 0)
                    return null;

                var r = new SwfReader(shape.Records);
                int count = r.U8();
                if (count == 0xFF && shape.Version >= 2)
                    count = r.U16();
                if (count == 0)
                    return null;

                bool alpha = shape.Version >= 3;
                Color ReadColor()
                {
                    var red = r.U8();
                    var green = r.U8();
                    var blue = r.U8();
                    var a = alpha ? r.U8() : (byte)255;
                    return Color.FromArgb(a, red, green, blue);
                }

                var fill = new ShapeFill { Type = r.U8() };
                switch (fill.Type)
                {
                    case 0x00:
                        fill.Kind = FillKind.Solid;
                        fill.Color = ReadColor();
                        break;
                    case 0x10:
                    case 0x12:
                    case 0x13:
                        {
                            fill.Kind = FillKind.Gradient;
                            fill.Matrix = Matrix.Read(r);
                            int stops = r.U8() & 0x0F;
                            if (stops > 0)
                            {
                                r.U8(); // ratio
                                fill.Color = ReadColor();
                            }
                            break;
                        }
                    case 0x40:
                    case 0x41:
                    case 0x42:
                    case 0x43:
                        fill.Kind = FillKind.Bitmap;
                        fill.BitmapId = r.U16();
                        fill.Matrix = Matrix.Read(r);
                        break;
                    default:
                        fill.Kind = FillKind.Unknown;
                        break;
                }
                return fill;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
