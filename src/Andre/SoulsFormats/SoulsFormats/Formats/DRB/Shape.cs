using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SoulsFormats
{
    public partial class DRB
    {
        internal enum ShapeType
        {
            Dialog,
            GouraudFrame,
            GouraudRect,
            GouraudSprite,
            Mask,
            MonoFrame,
            MonoRect,
            Null,
            ScrollText,
            Sprite,
            Text
        }

        /// <summary>
        /// Describes the appearance of a UI element.
        /// </summary>
        public abstract class Shape
        {
            internal abstract ShapeType Type { get; }

            /// <summary>
            /// Left bound of this element, relative to 1280x720.
            /// </summary>
            public short LeftEdge { get; set; }

            /// <summary>
            /// Top bound of this element, relative to 1280x720.
            /// </summary>
            public short TopEdge { get; set; }

            /// <summary>
            /// Right bound of this element, relative to 1280x720.
            /// </summary>
            public short RightEdge { get; set; }

            /// <summary>
            /// Bottom bound of this element, relative to 1280x720.
            /// </summary>
            public short BottomEdge { get; set; }

            /// <summary>
            /// For DSR, the X coordinate which the element scales relative to.
            /// </summary>
            public short ScalingOriginX { get; set; }

            /// <summary>
            /// For DSR, the Y coordinate which the element scales relative to.
            /// </summary>
            public short ScalingOriginY { get; set; }

            /// <summary>
            /// For DSR, the behavior of scaling for this element.
            /// </summary>
            public short ScalingMode { get; set; }

            internal Shape()
            {
                ScalingOriginX = -1;
                ScalingOriginY = -1;
            }

            internal static Shape Read(BinaryReaderEx br, DRBVersion version, Dictionary<int, string> strings, long shprStart)
            {
                int typeOffset = br.ReadInt32();
                int shprOffset = br.ReadInt32();
                string type = strings[typeOffset];

                Shape result;
                br.StepIn(shprStart + shprOffset);
                {
                    if (type == "Dialog")
                        result = new Dialog(br, version);
                    else if (type == "GouraudFrame")
                        result = new GouraudFrame(br, version);
                    else if (type == "GouraudRect")
                        result = new GouraudRect(br, version);
                    else if (type == "GouraudSprite")
                        result = new GouraudSprite(br, version);
                    else if (type == "Mask")
                        result = new Mask(br, version, strings);
                    else if (type == "MonoFrame")
                        result = new MonoFrame(br, version);
                    else if (type == "MonoRect")
                        result = new MonoRect(br, version);
                    else if (type == "Null")
                        result = new Null(br, version);
                    else if (type == "ScrollText")
                        result = new ScrollText(br, version, strings);
                    else if (type == "Sprite")
                        result = new Sprite(br, version);
                    else if (type == "Text")
                        result = new Text(br, version, strings);
                    else
                        throw new InvalidDataException($"Unknown shape type: {type}");
                }
                br.StepOut();
                return result;
            }

            internal Shape(BinaryReaderEx br, DRBVersion version)
            {
                LeftEdge = br.ReadInt16();
                TopEdge = br.ReadInt16();
                RightEdge = br.ReadInt16();
                BottomEdge = br.ReadInt16();

                if (version == DRBVersion.DarkSoulsRemastered && Type != ShapeType.Null)
                {
                    ScalingOriginX = br.ReadInt16();
                    ScalingOriginY = br.ReadInt16();
                    ScalingMode = br.ReadInt16();
                    br.AssertInt16(0);
                }
                else
                {
                    ScalingOriginX = -1;
                    ScalingOriginY = -1;
                    ScalingMode = 0;
                }
            }

            internal void WriteData(BinaryWriterEx bw, DRBVersion version, Dictionary<string, int> stringOffsets)
            {
                bw.WriteInt16(LeftEdge);
                bw.WriteInt16(TopEdge);
                bw.WriteInt16(RightEdge);
                bw.WriteInt16(BottomEdge);

                if (version == DRBVersion.DarkSoulsRemastered && Type != ShapeType.Null)
                {
                    bw.WriteInt16(ScalingOriginX);
                    bw.WriteInt16(ScalingOriginY);
                    bw.WriteInt16(ScalingMode);
                    bw.WriteInt16(0);
                }

                WriteSpecific(bw, stringOffsets);
            }

            internal abstract void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets);

            internal void WriteHeader(BinaryWriterEx bw, Dictionary<string, int> stringOffsets, Queue<int> shprOffsets)
            {
                bw.WriteInt32(stringOffsets[Type.ToString()]);
                bw.WriteInt32(shprOffsets.Dequeue());
            }

            /// <summary>
            /// Returns the type and bounds of this Shape.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} ({LeftEdge}, {TopEdge}) ({RightEdge}, {BottomEdge})";
            }

            /// <summary>
            /// Color blending mode of various shapes.
            /// </summary>
            public enum BlendingMode : byte
            {
                /// <summary>
                /// No transparency.
                /// </summary>
                Opaque = 0,

                /// <summary>
                /// Alpha transparency.
                /// </summary>
                Alpha = 1,

                /// <summary>
                /// Color is added to the underlying color value.
                /// </summary>
                Add = 2,

                /// <summary>
                /// Color is subtracted from the underlying color value.
                /// </summary>
                Subtract = 3,
            }

            /// <summary>
            /// Rotation and mirroring properties of a Sprite.
            /// </summary>
            [Flags]
            public enum SpriteOrientation : byte
            {
                /// <summary>
                /// No modification.
                /// </summary>
                None = 0,

                /// <summary>
                /// Rotate texture 90 degrees clockwise.
                /// </summary>
                RotateCW = 0x10,

                /// <summary>
                /// Rotate texture 180 degrees.
                /// </summary>
                Rotate180 = 0x20,

                /// <summary>
                /// Flip texture vertically.
                /// </summary>
                FlipVertical = 0x40,

                /// <summary>
                /// Flip texture horizontally.
                /// </summary>
                FlipHorizontal = 0x80,
            }

            /// <summary>
            /// Displays a region of a texture.
            /// </summary>
            public abstract class SpriteBase : Shape
            {
                /// <summary>
                /// Left bound of the texture region displayed by this element.
                /// </summary>
                public short TexLeftEdge { get; set; }

                /// <summary>
                /// Top bound of the texture region displayed by this element.
                /// </summary>
                public short TexTopEdge { get; set; }

                /// <summary>
                /// Right bound of the texture region displayed by this element.
                /// </summary>
                public short TexRightEdge { get; set; }

                /// <summary>
                /// Bottom bound of the texture region displayed by this element.
                /// </summary>
                public short TexBottomEdge { get; set; }

                /// <summary>
                /// The texture to display, indexing menu.tpf.
                /// </summary>
                public short TextureIndex { get; set; }

                /// <summary>
                /// Flags modifying the orientation of the texture.
                /// </summary>
                public SpriteOrientation Orientation { get; set; }

                /// <summary>
                /// Color blending mode.
                /// </summary>
                public BlendingMode BlendMode { get; set; }

                internal SpriteBase() : base()
                {
                    BlendMode = BlendingMode.Alpha;
                }

                internal SpriteBase(BinaryReaderEx br, DRBVersion version) : base(br, version)
                {
                    TexLeftEdge = br.ReadInt16();
                    TexTopEdge = br.ReadInt16();
                    TexRightEdge = br.ReadInt16();
                    TexBottomEdge = br.ReadInt16();
                    TextureIndex = br.ReadInt16();
                    Orientation = (SpriteOrientation)br.ReadByte();
                    BlendMode = br.ReadEnum8<BlendingMode>();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteInt16(TexLeftEdge);
                    bw.WriteInt16(TexTopEdge);
                    bw.WriteInt16(TexRightEdge);
                    bw.WriteInt16(TexBottomEdge);
                    bw.WriteInt16(TextureIndex);
                    bw.WriteByte((byte)Orientation);
                    bw.WriteByte((byte)BlendMode);
                }
            }

            /// <summary>
            /// Indicates the positioning of text within its element.
            /// </summary>
            [Flags]
            public enum AlignFlags : byte
            {
                /// <summary>
                /// Anchor to top left.
                /// </summary>
                TopLeft = 0,

                /// <summary>
                /// Anchor to right side.
                /// </summary>
                Right = 1,

                /// <summary>
                /// Center horizontally.
                /// </summary>
                CenterHorizontal = 2,

                /// <summary>
                /// Anchor to bottom side.
                /// </summary>
                Bottom = 4,

                /// <summary>
                /// Center vertically.
                /// </summary>
                CenterVertical = 8
            }

            /// <summary>
            /// Indicates the source of a text element's text.
            /// </summary>
            public enum TxtType : byte
            {
                /// <summary>
                /// Text is a literal value stored in the DRB.
                /// </summary>
                Literal = 0,

                /// <summary>
                /// Text is a static FMG ID.
                /// </summary>
                FMG = 1,

                /// <summary>
                /// Text is assigned at runtime.
                /// </summary>
                Dynamic = 2
            }

            /// <summary>
            /// Either a fixed text or scrolling text element.
            /// </summary>
            public abstract class TextBase : Shape
            {
                /// <summary>
                /// Color blending mode.
                /// </summary>
                public BlendingMode BlendMode { get; set; }

                /// <summary>
                /// Distance between each line of text.
                /// </summary>
                public short LineSpacing { get; set; }

                /// <summary>
                /// Index of a color in the menu color param, or 0 to use the CustomColor.
                /// </summary>
                public int PaletteColor { get; set; }

                /// <summary>
                /// Custom color used when PaletteColor is 0.
                /// </summary>
                public Color CustomColor { get; set; }

                /// <summary>
                /// From 0-11, different sizes of the menu font. 12 is the subtitle font.
                /// </summary>
                public short FontSize { get; set; }

                /// <summary>
                /// The horizontal and vertical alignment of the text.
                /// </summary>
                public AlignFlags Alignment { get; set; }

                /// <summary>
                /// Whether the element uses a text literal, a static FMG ID, or is assigned at runtime.
                /// </summary>
                public TxtType TextType { get; set; }

                /// <summary>
                /// The maximum characters to display.
                /// </summary>
                public int CharLength { get; set; }

                /// <summary>
                /// If TextType is Literal, the text to display, otherwise null.
                /// </summary>
                public string TextLiteral { get; set; }

                /// <summary>
                /// If TextType is FMG, the FMG ID to display, otherwise -1.
                /// </summary>
                public int TextID { get; set; }

                /// <summary>
                /// An unknown optional structure which is always present in practice.
                /// </summary>
                public UnknownA Unknown { get; set; }

                internal TextBase() : base()
                {
                    BlendMode = BlendingMode.Alpha;
                    TextType = TxtType.FMG;
                    TextID = -1;
                    Unknown = new UnknownA();
                }

                internal TextBase(BinaryReaderEx br, DRBVersion version, Dictionary<int, string> strings) : base(br, version)
                {
                    BlendMode = br.ReadEnum8<BlendingMode>();
                    bool unk01 = br.ReadBoolean();
                    LineSpacing = br.ReadInt16();
                    PaletteColor = br.ReadInt32();
                    CustomColor = ReadColor(br);
                    FontSize = br.ReadInt16();
                    Alignment = (AlignFlags)br.ReadByte();
                    TextType = br.ReadEnum8<TxtType>();
                    br.AssertInt32(0x1C); // Local offset to variable data below

                    if (TextType == TxtType.Literal)
                    {
                        int textOffset = br.ReadInt32();
                        TextLiteral = strings[textOffset];
                        CharLength = -1;
                        TextID = -1;
                    }
                    else if (TextType == TxtType.FMG)
                    {
                        CharLength = br.ReadInt32();
                        TextID = br.ReadInt32();
                        TextLiteral = null;
                    }
                    else if (TextType == TxtType.Dynamic)
                    {
                        CharLength = br.ReadInt32();
                        TextLiteral = null;
                        TextID = -1;
                    }

                    ReadSubtype(br);

                    if (unk01)
                        Unknown = new UnknownA(br);
                }

                internal virtual void ReadSubtype(BinaryReaderEx br) { }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteByte((byte)BlendMode);
                    bw.WriteBoolean(Unknown != null);
                    bw.WriteInt16(LineSpacing);
                    bw.WriteInt32(PaletteColor);
                    WriteColor(bw, CustomColor);
                    bw.WriteInt16(FontSize);
                    bw.WriteByte((byte)Alignment);
                    bw.WriteByte((byte)TextType);
                    bw.WriteInt32(0x1C);

                    if (TextType == TxtType.Literal)
                    {
                        bw.WriteInt32(stringOffsets[TextLiteral]);
                    }
                    else if (TextType == TxtType.FMG)
                    {
                        bw.WriteInt32(CharLength);
                        bw.WriteInt32(TextID);
                    }
                    else if (TextType == TxtType.Dynamic)
                    {
                        bw.WriteInt32(CharLength);
                    }

                    WriteSubtype(bw);

                    Unknown?.Write(bw);
                }

                internal virtual void WriteSubtype(BinaryWriterEx bw) { }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class UnknownA
                {
                    /// <summary>
                    /// Unknown; always 0.
                    /// </summary>
                    public int Unk00 { get; set; }

                    /// <summary>
                    /// Unknown optional structure, null if not present.
                    /// </summary>
                    public UnknownB SubUnknown { get; set; }

                    /// <summary>
                    /// Creates an UnknownA with default values.
                    /// </summary>
                    public UnknownA() { }

                    internal UnknownA(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt32();
                        short unk04 = br.AssertInt16([0, 1]);
                        if (unk04 == 1)
                            SubUnknown = new UnknownB(br);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(Unk00);
                        bw.WriteInt16((short)(SubUnknown == null ? 0 : 1));
                        SubUnknown?.Write(bw);
                    }
                }

                /// <summary>
                /// Unknown structure only observed in Shadow Assault: Tenchu.
                /// </summary>
                public class UnknownB
                {
                    /// <summary>
                    /// Unknown; always 0.
                    /// </summary>
                    public int Unk00 { get; set; }

                    /// <summary>
                    /// Unknown; always 0xFF.
                    /// </summary>
                    public short Unk04 { get; set; }

                    /// <summary>
                    /// Unknown; always 0 or 2.
                    /// </summary>
                    public short Unk06 { get; set; }

                    /// <summary>
                    /// Unknown; always 0 or 2.
                    /// </summary>
                    public short Unk08 { get; set; }

                    /// <summary>
                    /// Creates an UnknownB with default values.
                    /// </summary>
                    public UnknownB()
                    {
                        Unk04 = 0xFF;
                    }

                    internal UnknownB(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt32();
                        Unk04 = br.ReadInt16();
                        Unk06 = br.ReadInt16();
                        Unk08 = br.ReadInt16();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(Unk00);
                        bw.WriteInt16(Unk04);
                        bw.WriteInt16(Unk06);
                        bw.WriteInt16(Unk08);
                    }
                }
            }

            /// <summary>
            /// Displays another referenced group of elements.
            /// </summary>
            public class Dialog : Shape
            {
                internal override ShapeType Type => ShapeType.Dialog;

                /// <summary>
                /// Dlg referenced by this element; must be found in the parent DRB's Dlg list.
                /// </summary>
                public Dlg Dlg { get; set; }
                internal short DlgIndex;

                /// <summary>
                /// Unknown; always 0 or 1.
                /// </summary>
                public byte Unk02 { get; set; }

                /// <summary>
                /// Unknown; always 0 or 1. Determines whether the optional structure is loaded.
                /// </summary>
                public byte Unk03 { get; set; }

                /// <summary>
                /// Index of a color in the menu color param, or 0 to use the CustomColor.
                /// </summary>
                public int PaletteColor { get; set; }

                /// <summary>
                /// Custom color used when PaletteColor is 0.
                /// </summary>
                public Color CustomColor { get; set; }

                /// <summary>
                /// An unknown, optional, and unused structure; only loaded if Unk03 is non-zero.
                /// </summary>
                public UnknownA Unknown { get; set; }

                /// <summary>
                /// Creates a Dialog with default values.
                /// </summary>
                public Dialog()
                {
                    DlgIndex = -1;
                    Unk02 = 1;
                    Unk03 = 1;
                    CustomColor = Color.White;
                }

                internal Dialog(BinaryReaderEx br, DRBVersion version) : base(br, version)
                {
                    DlgIndex = br.ReadInt16();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                    PaletteColor = br.ReadInt32();
                    CustomColor = ReadColor(br);
                    bool unk0C = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);

                    if (unk0C)
                        Unknown = new UnknownA(br);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteInt16(DlgIndex);
                    bw.WriteByte(Unk02);
                    bw.WriteByte(Unk03);
                    bw.WriteInt32(PaletteColor);
                    WriteColor(bw, CustomColor);
                    bw.WriteByte((byte)(Unknown == null ? 0 : 1));
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class UnknownA
                {
                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short Unk00 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public short Unk02 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk04 { get; set; }

                    /// <summary>
                    /// Creates an UnknownA with default values.
                    /// </summary>
                    public UnknownA() { }

                    internal UnknownA(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt16();
                        Unk02 = br.ReadInt16();
                        Unk04 = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt16(Unk00);
                        bw.WriteInt16(Unk02);
                        bw.WriteInt32(Unk04);
                    }
                }
            }

            /// <summary>
            /// A rectangular frame with color interpolated between each corner.
            /// </summary>
            public class GouraudFrame : Shape
            {
                internal override ShapeType Type => ShapeType.GouraudFrame;

                /// <summary>
                /// Color blending mode.
                /// </summary>
                public BlendingMode BlendMode { get; set; }

                /// <summary>
                /// Thickness of the border.
                /// </summary>
                public byte Thickness { get; set; }

                /// <summary>
                /// The color at the top left corner.
                /// </summary>
                public Color TopLeftColor { get; set; }

                /// <summary>
                /// The color at the top right corner.
                /// </summary>
                public Color TopRightColor { get; set; }

                /// <summary>
                /// The color at the bottom right corner.
                /// </summary>
                public Color BottomRightColor { get; set; }

                /// <summary>
                /// The color at the bottom left corner.
                /// </summary>
                public Color BottomLeftColor { get; set; }

                /// <summary>
                /// Creates a GouraudFrame with default values.
                /// </summary>
                public GouraudFrame() : base()
                {
                    BlendMode = BlendingMode.Alpha;
                    Thickness = 1;
                    TopLeftColor = Color.Black;
                    TopRightColor = Color.Black;
                    BottomRightColor = Color.Black;
                    BottomLeftColor = Color.Black;
                }

                internal GouraudFrame(BinaryReaderEx br, DRBVersion version) : base(br, version)
                {
                    BlendMode = br.ReadEnum8<BlendingMode>();
                    br.AssertInt16(0);
                    Thickness = br.ReadByte();
                    TopLeftColor = ReadColor(br);
                    TopRightColor = ReadColor(br);
                    BottomRightColor = ReadColor(br);
                    BottomLeftColor = ReadColor(br);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteByte((byte)BlendMode);
                    bw.WriteInt16(0);
                    bw.WriteByte(Thickness);
                    WriteColor(bw, TopLeftColor);
                    WriteColor(bw, TopRightColor);
                    WriteColor(bw, BottomRightColor);
                    WriteColor(bw, BottomLeftColor);
                }
            }

            /// <summary>
            /// A rectangle with color interpolated between each corner.
            /// </summary>
            public class GouraudRect : Shape
            {
                internal override ShapeType Type => ShapeType.GouraudRect;

                /// <summary>
                /// Color blending mode.
                /// </summary>
                public BlendingMode BlendMode { get; set; }

                /// <summary>
                /// The color at the top left corner.
                /// </summary>
                public Color TopLeftColor { get; set; }

                /// <summary>
                /// The color at the top right corner.
                /// </summary>
                public Color TopRightColor { get; set; }

                /// <summary>
                /// The color at the bottom right corner.
                /// </summary>
                public Color BottomRightColor { get; set; }

                /// <summary>
                /// The color at the bottom left corner.
                /// </summary>
                public Color BottomLeftColor { get; set; }

                /// <summary>
                /// Creates a GouraudRect with default values.
                /// </summary>
                public GouraudRect() : base()
                {
                    BlendMode = BlendingMode.Alpha;
                    TopLeftColor = Color.Black;
                    TopRightColor = Color.Black;
                    BottomRightColor = Color.Black;
                    BottomLeftColor = Color.Black;
                }

                internal GouraudRect(BinaryReaderEx br, DRBVersion version) : base(br, version)
                {
                    BlendMode = br.ReadEnum8<BlendingMode>();
                    br.AssertInt16(0);
                    br.AssertByte(0);
                    TopLeftColor = ReadColor(br);
                    TopRightColor = ReadColor(br);
                    BottomRightColor = ReadColor(br);
                    BottomLeftColor = ReadColor(br);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteByte((byte)BlendMode);
                    bw.WriteInt16(0);
                    bw.WriteByte(0);
                    WriteColor(bw, TopLeftColor);
                    WriteColor(bw, TopRightColor);
                    WriteColor(bw, BottomRightColor);
                    WriteColor(bw, BottomLeftColor);
                }
            }

            /// <summary>
            /// Displays a texture region with color interpolated between each corner.
            /// </summary>
            public class GouraudSprite : SpriteBase
            {
                internal override ShapeType Type => ShapeType.GouraudSprite;

                /// <summary>
                /// The color at the top left corner.
                /// </summary>
                public Color TopLeftColor { get; set; }

                /// <summary>
                /// The color at the top right corner.
                /// </summary>
                public Color TopRightColor { get; set; }

                /// <summary>
                /// The color at the bottom right corner.
                /// </summary>
                public Color BottomRightColor { get; set; }

                /// <summary>
                /// The color at the bottom left corner.
                /// </summary>
                public Color BottomLeftColor { get; set; }

                /// <summary>
                /// Creates a GouraudSprite with default values.
                /// </summary>
                public GouraudSprite() : base()
                {
                    TopLeftColor = Color.White;
                    TopRightColor = Color.White;
                    BottomRightColor = Color.White;
                    BottomLeftColor = Color.White;
                }

                internal GouraudSprite(BinaryReaderEx br, DRBVersion version) : base(br, version)
                {
                    TopLeftColor = ReadColor(br);
                    TopRightColor = ReadColor(br);
                    BottomRightColor = ReadColor(br);
                    BottomLeftColor = ReadColor(br);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    base.WriteSpecific(bw, stringOffsets);
                    WriteColor(bw, TopLeftColor);
                    WriteColor(bw, TopRightColor);
                    WriteColor(bw, BottomRightColor);
                    WriteColor(bw, BottomLeftColor);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Mask : Shape
            {
                internal override ShapeType Type => ShapeType.Mask;

                /// <summary>
                /// Unknown.
                /// </summary>
                public string DlgoName { get; set; }

                /// <summary>
                /// Unknown; always 1.
                /// </summary>
                public byte Unk04 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk05 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk09 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public short Unk0D { get; set; }

                /// <summary>
                /// Creates a Mask with default values.
                /// </summary>
                public Mask() : base()
                {
                    Unk04 = 1;
                }

                internal Mask(BinaryReaderEx br, DRBVersion version, Dictionary<int, string> strings) : base(br, version)
                {
                    DlgoName = strings[br.ReadInt32()];
                    Unk04 = br.ReadByte();
                    Unk05 = br.ReadInt32();
                    Unk09 = br.ReadInt32();
                    Unk0D = br.ReadInt16();
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteInt32(stringOffsets[DlgoName]);
                    bw.WriteByte(Unk04);
                    bw.WriteInt32(Unk05);
                    bw.WriteInt32(Unk09);
                    bw.WriteInt16(Unk0D);
                }
            }

            /// <summary>
            /// A rectangular frame with a solid color.
            /// </summary>
            public class MonoFrame : Shape
            {
                internal override ShapeType Type => ShapeType.MonoFrame;

                /// <summary>
                /// Color blending mode.
                /// </summary>
                public BlendingMode BlendMode { get; set; }

                /// <summary>
                /// Thickness of the border.
                /// </summary>
                public byte Thickness { get; set; }

                /// <summary>
                /// Index of a color in the menu color param, or 0 to use the CustomColor.
                /// </summary>
                public int PaletteColor { get; set; }

                /// <summary>
                /// Custom color used when PaletteColor is 0.
                /// </summary>
                public Color CustomColor { get; set; }

                /// <summary>
                /// Creates a MonoFrame with default values.
                /// </summary>
                public MonoFrame() : base()
                {
                    BlendMode = BlendingMode.Alpha;
                    Thickness = 1;
                    CustomColor = Color.Black;
                }

                internal MonoFrame(BinaryReaderEx br, DRBVersion version) : base(br, version)
                {
                    BlendMode = br.ReadEnum8<BlendingMode>();
                    br.AssertInt16(0);
                    Thickness = br.ReadByte();
                    PaletteColor = br.ReadInt32();
                    CustomColor = ReadColor(br);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteByte((byte)BlendMode);
                    bw.WriteInt16(0);
                    bw.WriteByte(Thickness);
                    bw.WriteInt32(PaletteColor);
                    WriteColor(bw, CustomColor);
                }
            }

            /// <summary>
            /// A rectangle with a solid color.
            /// </summary>
            public class MonoRect : Shape
            {
                internal override ShapeType Type => ShapeType.MonoRect;

                /// <summary>
                /// Color blending mode.
                /// </summary>
                public BlendingMode BlendMode { get; set; }

                /// <summary>
                /// Index of a color in the menu color param, or 0 to use the CustomColor.
                /// </summary>
                public int PaletteColor { get; set; }

                /// <summary>
                /// Custom color used when PaletteColor is 0.
                /// </summary>
                public Color CustomColor { get; set; }

                /// <summary>
                /// Creates a MonoRect with default values.
                /// </summary>
                public MonoRect() : base()
                {
                    BlendMode = BlendingMode.Alpha;
                    CustomColor = Color.Black;
                }

                internal MonoRect(BinaryReaderEx br, DRBVersion version) : base(br, version)
                {
                    BlendMode = br.ReadEnum8<BlendingMode>();
                    br.AssertInt16(0);
                    br.AssertByte(0);
                    PaletteColor = br.ReadInt32();
                    CustomColor = ReadColor(br);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    bw.WriteByte((byte)BlendMode);
                    bw.WriteInt16(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(PaletteColor);
                    WriteColor(bw, CustomColor);
                }
            }

            /// <summary>
            /// An invisible element used to mark a position.
            /// </summary>
            public class Null : Shape
            {
                internal override ShapeType Type => ShapeType.Null;

                /// <summary>
                /// Creates a Null with default values.
                /// </summary>
                public Null() : base() { }

                internal Null(BinaryReaderEx br, DRBVersion version) : base(br, version) { }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets) { }
            }

            /// <summary>
            /// A scrolling text field.
            /// </summary>
            public class ScrollText : TextBase
            {
                internal override ShapeType Type => ShapeType.ScrollText;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkX00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkX02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkX04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkX06 { get; set; }

                /// <summary>
                /// Unknown; always 15 or 100.
                /// </summary>
                public short ScrollSpeed { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkX0A { get; set; }

                /// <summary>
                /// Creates a ScrollText with default values.
                /// </summary>
                public ScrollText() : base()
                {
                    ScrollSpeed = 15;
                }

                internal ScrollText(BinaryReaderEx br, DRBVersion version, Dictionary<int, string> strings) : base(br, version, strings) { }

                internal override void ReadSubtype(BinaryReaderEx br)
                {
                    UnkX00 = br.ReadInt16();
                    UnkX02 = br.ReadInt16();
                    UnkX04 = br.ReadInt16();
                    UnkX06 = br.ReadInt16();
                    ScrollSpeed = br.ReadInt16();
                    UnkX0A = br.ReadInt16();
                }

                internal override void WriteSubtype(BinaryWriterEx bw)
                {
                    bw.WriteInt16(UnkX00);
                    bw.WriteInt16(UnkX02);
                    bw.WriteInt16(UnkX04);
                    bw.WriteInt16(UnkX06);
                    bw.WriteInt16(ScrollSpeed);
                    bw.WriteInt16(UnkX0A);
                }
            }

            /// <summary>
            /// Displays a texture region with color tinting.
            /// </summary>
            public class Sprite : SpriteBase
            {
                internal override ShapeType Type => ShapeType.Sprite;

                /// <summary>
                /// Index of a color in the menu color param, or 0 to use the CustomColor.
                /// </summary>
                public int PaletteColor { get; set; }

                /// <summary>
                /// Custom color used when PaletteColor is 0.
                /// </summary>
                public Color CustomColor { get; set; }

                /// <summary>
                /// Creates a Sprite with default values.
                /// </summary>
                public Sprite() : base()
                {
                    CustomColor = Color.White;
                }

                internal Sprite(BinaryReaderEx br, DRBVersion version) : base(br, version)
                {
                    PaletteColor = br.ReadInt32();
                    CustomColor = ReadColor(br);
                }

                internal override void WriteSpecific(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
                {
                    base.WriteSpecific(bw, stringOffsets);
                    bw.WriteInt32(PaletteColor);
                    WriteColor(bw, CustomColor);
                }
            }

            /// <summary>
            /// A fixed text field.
            /// </summary>
            public class Text : TextBase
            {
                internal override ShapeType Type => ShapeType.Text;

                /// <summary>
                /// Creates a Text with default values.
                /// </summary>
                public Text() : base() { }

                internal Text(BinaryReaderEx br, DRBVersion version, Dictionary<int, string> strings) : base(br, version, strings) { }
            }
        }
    }
}
