using System;
using System.Collections.Generic;
using System.Drawing;

namespace SoulsFormats
{
    public partial class GFX
    {
        /// <summary>
        /// SWF and GFx tag codes seen in FromSoftware's movies (others are kept as raw tags all the same).
        /// </summary>
        public enum TagCode : ushort
        {
#pragma warning disable CS1591
            End = 0,
            ShowFrame = 1,
            DefineShape = 2,
            PlaceObject = 4,
            RemoveObject = 5,
            DefineBits = 6,
            DefineButton = 7,
            SetBackgroundColor = 9,
            DefineFont = 10,
            DefineText = 11,
            DoAction = 12,
            DefineBitsLossless = 20,
            DefineBitsJPEG2 = 21,
            DefineShape2 = 22,
            Protect = 24,
            PlaceObject2 = 26,
            RemoveObject2 = 28,
            DefineShape3 = 32,
            DefineText2 = 33,
            DefineButton2 = 34,
            DefineBitsJPEG3 = 35,
            DefineBitsLossless2 = 36,
            DefineEditText = 37,
            DefineSprite = 39,
            FrameLabel = 43,
            DefineMorphShape = 46,
            DefineFont2 = 48,
            ExportAssets = 56,
            ImportAssets = 57,
            DoInitAction = 59,
            FileAttributes = 69,
            PlaceObject3 = 70,
            ImportAssets2 = 71,
            DoABCDefine = 72,
            DefineFontAlignZones = 73,
            CSMTextSettings = 74,
            DefineFont3 = 75,
            SymbolClass = 76,
            Metadata = 77,
            DefineScalingGrid = 78,
            DoABC = 82,
            DefineShape4 = 83,
            DefineMorphShape2 = 84,
            DefineSceneAndFrameLabelData = 86,
            DefineFontName = 88,
            DefineBitsJPEG4 = 90,
            GfxExporterInfo = 1000,
            GfxDefineExternalImage = 1001,
            GfxFontTextureInfo = 1002,
            GfxDefineExternalGradient = 1003,
            GfxDefineGradientMap = 1004,
            GfxDefineCompactedFont = 1005,
            GfxDefineExternalSound = 1006,
            GfxDefineExternalStreamSound = 1007,
            GfxDefineSubImage = 1008,
            GfxDefineExternalImage2 = 1009,
#pragma warning restore CS1591
        }

        /// <summary>
        /// A tag. Tags without a typed class below keep their body as raw bytes and write it back unchanged.
        /// </summary>
        public class Tag
        {
            /// <summary>The tag code.</summary>
            public TagCode Code { get; set; }

            /// <summary>
            /// The header used the long (6-byte) form. Flash wrote it for some short tags; it is kept so unchanged tags
            /// write back byte-equal. Bodies of 63 bytes or more always use the long form.
            /// </summary>
            public bool LongHeader { get; set; }

            /// <summary>The raw body, for tags without a typed class (or whose typed read did not round-trip).</summary>
            public byte[] Data { get; set; }

            /// <summary>The character id this tag defines, or null when it defines none.</summary>
            public virtual int? DefinedCharacterId
            {
                get
                {
                    switch (Code)
                    {
                        case TagCode.DefineBits:
                        case TagCode.DefineButton:
                        case TagCode.DefineFont:
                        case TagCode.DefineText:
                        case TagCode.DefineBitsLossless:
                        case TagCode.DefineBitsJPEG2:
                        case TagCode.DefineText2:
                        case TagCode.DefineButton2:
                        case TagCode.DefineBitsJPEG3:
                        case TagCode.DefineBitsLossless2:
                        case TagCode.DefineMorphShape:
                        case TagCode.DefineFont2:
                        case TagCode.DefineFont3:
                        case TagCode.DefineMorphShape2:
                        case TagCode.DefineBitsJPEG4:
                        case TagCode.GfxDefineExternalImage:
                        case TagCode.GfxDefineSubImage:
                            return Data != null && Data.Length >= 2 ? BitConverter.ToUInt16(Data, 0) : null;
                        default:
                            return null;
                    }
                }
            }

            internal virtual void Decode(byte[] data, GFX owner) { Data = data; }

            internal virtual byte[] Encode() => Data;

            /// <summary>
            /// The tag's body as it would be written now: a snapshot of its fields, for undo.
            /// </summary>
            public byte[] GetBytes() => Encode() ?? Array.Empty<byte>();

            /// <summary>
            /// Restores the tag's fields from a body taken with <see cref="GetBytes"/>, keeping the object (so references
            /// to it stay valid). Not for DefineSprite, whose child tags would be replaced by new objects.
            /// </summary>
            public void SetBytes(byte[] data)
            {
                if (this is DefineSprite)
                    throw new InvalidOperationException("A DefineSprite cannot be restored from bytes.");
                Decode(data, null);
            }

            /// <summary>The tag code's name.</summary>
            public override string ToString() => Enum.IsDefined(Code) ? Code.ToString() : $"Tag{(int)Code}";
        }

        /// <summary>
        /// PlaceObject2 (26) and PlaceObject3 (70): adds an instance at a depth of the current timeline, or (Move)
        /// changes the instance already there. Fields that are null are absent from the tag.
        /// </summary>
        public class PlaceObject : Tag
        {
            /// <summary>Modifies the instance already at Depth (with CharacterId: replaces its character).</summary>
            public bool Move { get; set; }

            /// <summary>PlaceObject3 only: the character is a bitmap placed directly (GFx DefineExternalImage2).</summary>
            public bool HasImage { get; set; }

            /// <summary>PlaceObject3 only: the reserved top flag bit, kept as read.</summary>
            public bool Reserved { get; set; }

            /// <summary>Depth on the timeline (draw order: higher is in front).</summary>
            public ushort Depth { get; set; }

            /// <summary>PlaceObject3 only: the class to instantiate.</summary>
            public string ClassName { get; set; }

            /// <summary>The character to place.</summary>
            public ushort? CharacterId { get; set; }

            /// <summary>The transform.</summary>
            public Matrix Matrix { get; set; }

            /// <summary>The colour transform.</summary>
            public ColorTransform ColorTransform { get; set; }

            /// <summary>Morph ratio (also used as a key by some exporters).</summary>
            public ushort? Ratio { get; set; }

            /// <summary>
            /// The instance name. The game finds elements by instance-name path, so renaming breaks the game's lookups.
            /// </summary>
            public string Name { get; set; }

            /// <summary>This instance masks the depths up to ClipDepth.</summary>
            public ushort? ClipDepth { get; set; }

            /// <summary>PlaceObject3 only: surface filters.</summary>
            public List<Filter> Filters { get; set; }

            /// <summary>PlaceObject3 only: blend mode (SWF values, 0/1 normal).</summary>
            public byte? BlendMode { get; set; }

            /// <summary>PlaceObject3 only: cache-as-bitmap value.</summary>
            public byte? BitmapCache { get; set; }

            /// <summary>PlaceObject3 only: visibility.</summary>
            public byte? Visible { get; set; }

            /// <summary>PlaceObject3 only: opaque background colour.</summary>
            public Color? BackgroundColor { get; set; }

            /// <summary>Clip event actions (AS2), raw. None of the FromSoftware movies have any.</summary>
            public byte[] ClipActions { get; set; }

            /// <summary>Is a PlaceObject3.</summary>
            public bool IsPlaceObject3 => Code == TagCode.PlaceObject3;

            internal override void Decode(byte[] data, GFX owner)
            {
                var r = new SwfReader(data);
                // Optional fields are absent unless their flag is set (SetBytes reuses the object)
                ClassName = null; CharacterId = null; Matrix = null; ColorTransform = null; Ratio = null; Name = null;
                ClipDepth = null; Filters = null; BlendMode = null; BitmapCache = null; Visible = null; BackgroundColor = null; ClipActions = null;
                int f1 = r.U8();
                int f2 = IsPlaceObject3 ? r.U8() : 0;
                Move = (f1 & 0x01) != 0;
                HasImage = (f2 & 0x10) != 0;
                Reserved = (f2 & 0x80) != 0;
                Depth = r.U16();
                // The spec has the class name when (HasClassName || (HasImage && HasCharacter)); the files only carry
                // it for HasClassName. HasImage + a character id places a bitmap character directly.
                if ((f2 & 0x08) != 0) ClassName = r.Str();
                if ((f1 & 0x02) != 0) CharacterId = r.U16();
                if ((f1 & 0x04) != 0) Matrix = Matrix.Read(r);
                if ((f1 & 0x08) != 0) ColorTransform = ColorTransform.Read(r);
                if ((f1 & 0x10) != 0) Ratio = r.U16();
                if ((f1 & 0x20) != 0) Name = r.Str();
                if ((f1 & 0x40) != 0) ClipDepth = r.U16();
                if ((f2 & 0x01) != 0)
                {
                    int n = r.U8();
                    Filters = new List<Filter>(n);
                    for (int i = 0; i < n; i++)
                        Filters.Add(Filter.Read(r));
                }
                if ((f2 & 0x02) != 0) BlendMode = r.U8();
                if ((f2 & 0x04) != 0) BitmapCache = r.U8();
                if ((f2 & 0x20) != 0) Visible = r.U8();
                if ((f2 & 0x40) != 0)
                {
                    var c = r.Bytes(4);
                    BackgroundColor = Color.FromArgb(c[3], c[0], c[1], c[2]);
                }
                if ((f1 & 0x80) != 0) ClipActions = r.Rest();
                if (r.Remaining != 0)
                    throw new System.IO.InvalidDataException("PlaceObject has bytes past its fields.");
            }

            internal override byte[] Encode()
            {
                var w = new SwfWriter();
                int f1 = (Move ? 0x01 : 0) | (CharacterId != null ? 0x02 : 0) | (Matrix != null ? 0x04 : 0)
                    | (ColorTransform != null ? 0x08 : 0) | (Ratio != null ? 0x10 : 0) | (Name != null ? 0x20 : 0)
                    | (ClipDepth != null ? 0x40 : 0) | (ClipActions != null ? 0x80 : 0);
                w.U8((byte)f1);
                if (IsPlaceObject3)
                {
                    int f2 = (Filters != null ? 0x01 : 0) | (BlendMode != null ? 0x02 : 0) | (BitmapCache != null ? 0x04 : 0)
                        | (ClassName != null ? 0x08 : 0) | (HasImage ? 0x10 : 0) | (Visible != null ? 0x20 : 0)
                        | (BackgroundColor != null ? 0x40 : 0) | (Reserved ? 0x80 : 0);
                    w.U8((byte)f2);
                }
                w.U16(Depth);
                if (IsPlaceObject3 && ClassName != null) w.Str(ClassName);
                if (CharacterId != null) w.U16(CharacterId.Value);
                Matrix?.Write(w);
                ColorTransform?.Write(w);
                if (Ratio != null) w.U16(Ratio.Value);
                if (Name != null) w.Str(Name);
                if (ClipDepth != null) w.U16(ClipDepth.Value);
                if (IsPlaceObject3)
                {
                    if (Filters != null)
                    {
                        w.U8((byte)Filters.Count);
                        foreach (var f in Filters)
                            f.Write(w);
                    }
                    if (BlendMode != null) w.U8(BlendMode.Value);
                    if (BitmapCache != null) w.U8(BitmapCache.Value);
                    if (Visible != null) w.U8(Visible.Value);
                    if (BackgroundColor is Color c)
                        w.Bytes(new[] { c.R, c.G, c.B, c.A });
                }
                if (ClipActions != null) w.Bytes(ClipActions);
                return w.ToArray();
            }

            /// <summary>Depth and name, for display.</summary>
            public override string ToString() => $"{Code} depth {Depth}{(Name != null ? $" '{Name}'" : "")}";
        }

        /// <summary>
        /// RemoveObject2 (28): removes the instance at a depth of the current timeline.
        /// </summary>
        public class RemoveObject2 : Tag
        {
            /// <summary>The depth to clear.</summary>
            public ushort Depth { get; set; }

            internal override void Decode(byte[] data, GFX owner)
            {
                var r = new SwfReader(data);
                Depth = r.U16();
                if (r.Remaining != 0)
                    throw new System.IO.InvalidDataException("RemoveObject2 has extra bytes.");
            }

            internal override byte[] Encode()
            {
                var w = new SwfWriter();
                w.U16(Depth);
                return w.ToArray();
            }
        }

        /// <summary>
        /// DefineSprite (39): a MovieClip symbol with its own timeline of control tags.
        /// </summary>
        public class DefineSprite : Tag
        {
            /// <summary>The character id.</summary>
            public ushort Id { get; set; }

            /// <summary>The frame count in the header (the ShowFrame tags are what the player plays).</summary>
            public ushort FrameCount { get; set; }

            /// <summary>The sprite's timeline.</summary>
            public List<Tag> Tags { get; set; } = new();

            /// <inheritdoc/>
            public override int? DefinedCharacterId => Id;

            internal override void Decode(byte[] data, GFX owner)
            {
                Id = BitConverter.ToUInt16(data, 0);
                FrameCount = BitConverter.ToUInt16(data, 2);
                Tags = ReadTags(data, 4, data.Length, owner, out var after);
                if (after.Length != 0)
                    throw new System.IO.InvalidDataException("DefineSprite has bytes after its End tag.");
            }

            internal override byte[] Encode()
            {
                var w = new SwfWriter();
                w.U16(Id);
                w.U16(FrameCount);
                w.Bytes(WriteTags(Tags));
                return w.ToArray();
            }

            /// <summary>Id, for display.</summary>
            public override string ToString() => $"DefineSprite {Id}";
        }

        /// <summary>
        /// Horizontal text alignment of an edit text.
        /// </summary>
        public enum TextAlign : byte
        {
            /// <summary>Left.</summary>
            Left = 0,
            /// <summary>Right.</summary>
            Right = 1,
            /// <summary>Centre.</summary>
            Center = 2,
            /// <summary>Justify.</summary>
            Justify = 3,
        }

        /// <summary>
        /// The layout block of an edit text.
        /// </summary>
        public class EditTextLayout
        {
            /// <summary>Alignment.</summary>
            public TextAlign Align { get; set; }

            /// <summary>Left margin, in twips.</summary>
            public ushort LeftMargin { get; set; }

            /// <summary>Right margin, in twips.</summary>
            public ushort RightMargin { get; set; }

            /// <summary>First-line indent, in twips.</summary>
            public ushort Indent { get; set; }

            /// <summary>Extra line spacing, in twips.</summary>
            public short Leading { get; set; }

            /// <summary>A copy.</summary>
            public EditTextLayout Clone() => (EditTextLayout)MemberwiseClone();
        }

        /// <summary>
        /// DefineEditText (37): a text field. The game sets the shown text through code (from FMG), so InitialText is
        /// mostly the author's placeholder. Fields that are null are absent from the tag.
        /// </summary>
        public class DefineEditText : Tag
        {
            /// <summary>The character id.</summary>
            public ushort Id { get; set; }

            /// <summary>The text box, in twips relative to the instance.</summary>
            public Rect Bounds { get; set; }

            /// <summary>Wraps words.</summary>
            public bool WordWrap { get; set; }

            /// <summary>Several lines.</summary>
            public bool Multiline { get; set; }

            /// <summary>Password field.</summary>
            public bool Password { get; set; }

            /// <summary>Read-only.</summary>
            public bool ReadOnly { get; set; }

            /// <summary>Resizes to fit the text.</summary>
            public bool AutoSize { get; set; }

            /// <summary>Not selectable.</summary>
            public bool NoSelect { get; set; }

            /// <summary>Draws a border.</summary>
            public bool Border { get; set; }

            /// <summary>Was a static text in the authoring tool.</summary>
            public bool WasStatic { get; set; }

            /// <summary>The text is HTML.</summary>
            public bool Html { get; set; }

            /// <summary>Renders with the font's glyph outlines.</summary>
            public bool UseOutlines { get; set; }

            /// <summary>The font character.</summary>
            public ushort? FontId { get; set; }

            /// <summary>The font class name.</summary>
            public string FontClass { get; set; }

            /// <summary>Font height in twips (present when FontId or FontClass is).</summary>
            public ushort FontHeight { get; set; }

            /// <summary>Text colour.</summary>
            public Color? TextColor { get; set; }

            /// <summary>Maximum length.</summary>
            public ushort? MaxLength { get; set; }

            /// <summary>Alignment and margins.</summary>
            public EditTextLayout Layout { get; set; }

            /// <summary>The bound variable name (empty for none).</summary>
            public string VariableName { get; set; } = "";

            /// <summary>The initial text.</summary>
            public string InitialText { get; set; }

            /// <inheritdoc/>
            public override int? DefinedCharacterId => Id;

            internal override void Decode(byte[] data, GFX owner)
            {
                var r = new SwfReader(data);
                Id = r.U16();
                Bounds = Rect.Read(r);
                FontId = null; FontClass = null; FontHeight = 0; TextColor = null; MaxLength = null; Layout = null; InitialText = null;
                int f1 = r.U8();
                int f2 = r.U8();
                WordWrap = (f1 & 0x40) != 0;
                Multiline = (f1 & 0x20) != 0;
                Password = (f1 & 0x10) != 0;
                ReadOnly = (f1 & 0x08) != 0;
                AutoSize = (f2 & 0x40) != 0;
                NoSelect = (f2 & 0x10) != 0;
                Border = (f2 & 0x08) != 0;
                WasStatic = (f2 & 0x04) != 0;
                Html = (f2 & 0x02) != 0;
                UseOutlines = (f2 & 0x01) != 0;
                if ((f1 & 0x01) != 0) FontId = r.U16();
                if ((f2 & 0x80) != 0) FontClass = r.Str();
                if (FontId != null || FontClass != null) FontHeight = r.U16();
                if ((f1 & 0x04) != 0)
                {
                    var c = r.Bytes(4);
                    TextColor = Color.FromArgb(c[3], c[0], c[1], c[2]);
                }
                if ((f1 & 0x02) != 0) MaxLength = r.U16();
                if ((f2 & 0x20) != 0)
                {
                    Layout = new EditTextLayout
                    {
                        Align = (TextAlign)r.U8(),
                        LeftMargin = r.U16(),
                        RightMargin = r.U16(),
                        Indent = r.U16(),
                        Leading = r.S16(),
                    };
                }
                VariableName = r.Str();
                if ((f1 & 0x80) != 0) InitialText = r.Str();
                if (r.Remaining != 0)
                    throw new System.IO.InvalidDataException("DefineEditText has bytes past its fields.");
            }

            internal override byte[] Encode()
            {
                var w = new SwfWriter();
                w.U16(Id);
                Bounds.Write(w);
                int f1 = (FontId != null ? 0x01 : 0) | (MaxLength != null ? 0x02 : 0) | (TextColor != null ? 0x04 : 0)
                    | (ReadOnly ? 0x08 : 0) | (Password ? 0x10 : 0) | (Multiline ? 0x20 : 0) | (WordWrap ? 0x40 : 0)
                    | (InitialText != null ? 0x80 : 0);
                int f2 = (UseOutlines ? 0x01 : 0) | (Html ? 0x02 : 0) | (WasStatic ? 0x04 : 0) | (Border ? 0x08 : 0)
                    | (NoSelect ? 0x10 : 0) | (Layout != null ? 0x20 : 0) | (AutoSize ? 0x40 : 0) | (FontClass != null ? 0x80 : 0);
                w.U8((byte)f1);
                w.U8((byte)f2);
                if (FontId != null) w.U16(FontId.Value);
                if (FontClass != null) w.Str(FontClass);
                if (FontId != null || FontClass != null) w.U16(FontHeight);
                if (TextColor is Color c) w.Bytes(new[] { c.R, c.G, c.B, c.A });
                if (MaxLength != null) w.U16(MaxLength.Value);
                if (Layout != null)
                {
                    w.U8((byte)Layout.Align);
                    w.U16(Layout.LeftMargin);
                    w.U16(Layout.RightMargin);
                    w.U16(Layout.Indent);
                    w.S16(Layout.Leading);
                }
                w.Str(VariableName ?? "");
                if (InitialText != null) w.Str(InitialText);
                return w.ToArray();
            }

            /// <summary>Id, for display.</summary>
            public override string ToString() => $"DefineEditText {Id}";
        }

        /// <summary>
        /// DefineShape 1–4 (2, 22, 32, 83): a vector shape. The header (id, bounds) is typed; the fill/line styles and
        /// shape records are kept raw for now.
        /// </summary>
        public class DefineShape : Tag
        {
            /// <summary>The character id.</summary>
            public ushort Id { get; set; }

            /// <summary>The shape bounds including strokes, in twips.</summary>
            public Rect Bounds { get; set; }

            /// <summary>DefineShape4 only: the bounds without strokes.</summary>
            public Rect EdgeBounds { get; set; }

            /// <summary>DefineShape4 only: scaling-stroke and winding flags.</summary>
            public byte Shape4Flags { get; set; }

            /// <summary>Fill styles, line styles and shape records, raw.</summary>
            public byte[] Records { get; set; }

            /// <summary>The DefineShape version (1–4).</summary>
            public int Version => Code switch { TagCode.DefineShape2 => 2, TagCode.DefineShape3 => 3, TagCode.DefineShape4 => 4, _ => 1 };

            /// <inheritdoc/>
            public override int? DefinedCharacterId => Id;

            internal override void Decode(byte[] data, GFX owner)
            {
                var r = new SwfReader(data);
                Id = r.U16();
                Bounds = Rect.Read(r);
                if (Code == TagCode.DefineShape4)
                {
                    EdgeBounds = Rect.Read(r);
                    Shape4Flags = r.U8();
                }
                Records = r.Rest();
            }

            internal override byte[] Encode()
            {
                var w = new SwfWriter();
                w.U16(Id);
                Bounds.Write(w);
                if (Code == TagCode.DefineShape4)
                {
                    EdgeBounds.Write(w);
                    w.U8(Shape4Flags);
                }
                w.Bytes(Records);
                return w.ToArray();
            }

            /// <summary>Id, for display.</summary>
            public override string ToString() => $"{Code} {Id}";
        }

        /// <summary>
        /// GFx DefineExternalImage2 (1009): a bitmap character loaded from outside the movie. FileName (a .tga) is
        /// resolved by the game to a cell of the menu texture atlases (sblytbnd shoebox layouts over the menu TPFs).
        /// </summary>
        public class DefineExternalImage2 : Tag
        {
            /// <summary>The character id (32-bit in this tag).</summary>
            public uint Id { get; set; }

            /// <summary>The GFx bitmap format code (13 in the FromSoftware movies).</summary>
            public ushort BitmapFormat { get; set; }

            /// <summary>Width the image is shown at, in pixels.</summary>
            public ushort TargetWidth { get; set; }

            /// <summary>Height the image is shown at, in pixels.</summary>
            public ushort TargetHeight { get; set; }

            /// <summary>The export name.</summary>
            public string ExportName { get; set; }

            /// <summary>The image file name.</summary>
            public string FileName { get; set; }

            /// <summary>Bytes after the file name, raw (none in the FromSoftware movies).</summary>
            public byte[] Extra { get; set; }

            /// <inheritdoc/>
            public override int? DefinedCharacterId => (int)Id;

            internal override void Decode(byte[] data, GFX owner)
            {
                var r = new SwfReader(data);
                Id = r.U32();
                BitmapFormat = r.U16();
                TargetWidth = r.U16();
                TargetHeight = r.U16();
                ExportName = r.PStr();
                FileName = r.PStr();
                Extra = r.Rest();
            }

            internal override byte[] Encode()
            {
                var w = new SwfWriter();
                w.U32(Id);
                w.U16(BitmapFormat);
                w.U16(TargetWidth);
                w.U16(TargetHeight);
                w.PStr(ExportName);
                w.PStr(FileName);
                w.Bytes(Extra);
                return w.ToArray();
            }

            /// <summary>Id and file, for display.</summary>
            public override string ToString() => $"ExternalImage {Id} {FileName}";
        }

        /// <summary>
        /// FrameLabel (43): names the current frame. The game jumps to labels by name, so renaming breaks it.
        /// </summary>
        public class FrameLabel : Tag
        {
            /// <summary>The label.</summary>
            public string Name { get; set; }

            /// <summary>Bytes after the name (the named-anchor flag), raw.</summary>
            public byte[] Extra { get; set; }

            internal override void Decode(byte[] data, GFX owner)
            {
                var r = new SwfReader(data);
                Name = r.Str();
                Extra = r.Rest();
            }

            internal override byte[] Encode()
            {
                var w = new SwfWriter();
                w.Str(Name);
                w.Bytes(Extra);
                return w.ToArray();
            }

            /// <summary>The label, for display.</summary>
            public override string ToString() => $"FrameLabel '{Name}'";
        }
    }
}
