using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace SoulsFormats
{
    /// <summary>
    /// A Scaleform GFx movie: an SWF whose signature is GFX (or CFX when zlib compressed), with GFx extension tags
    /// (1000+). FromSoftware's menus and HUDs from DS3 to NR (DS3, BB, SDT, ER, AC6, NR). Extension: .gfx (loose).
    ///
    /// The tag stream is kept losslessly. Tags with a typed class (placements, sprites, edit texts, shapes, external
    /// images, frame labels) are decoded, but only if re-encoding the decoded tag gives back the same bytes; any other
    /// tag, or one that fails that check, keeps its raw body (see <see cref="TypedFallbacks"/>). Scripts (DoABC,
    /// SymbolClass) are raw. See Claude/Documentation/Scaleform Editor Analysis.md.
    /// </summary>
    public partial class GFX : SoulsFile<GFX>
    {
        /// <summary>
        /// The three-letter signature: GFX (uncompressed) or CFX (zlib). FWS and CWS (plain SWF) are read too.
        /// </summary>
        public string Signature { get; set; } = "GFX";

        /// <summary>
        /// The SWF version (11 in most movies, 43 in NR's).
        /// </summary>
        public byte Version { get; set; } = 11;

        /// <summary>
        /// The stage, in twips (1920x1080 for menus, 550x400 for font libraries).
        /// </summary>
        public Rect FrameSize { get; set; }

        /// <summary>
        /// Frame rate, 8.8 fixed point.
        /// </summary>
        public ushort FrameRate { get; set; }

        /// <summary>
        /// The frame count of the root timeline in the header.
        /// </summary>
        public ushort FrameCount { get; set; }

        /// <summary>
        /// The root timeline's tags, End included.
        /// </summary>
        public List<Tag> Tags { get; set; } = new();

        /// <summary>
        /// Bytes after the root End tag but inside the header's length, raw (none in vanilla).
        /// </summary>
        public byte[] AfterEnd { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// The PC movies are zero padded past the header's length so the file is a multiple of 16 bytes. When this is
        /// set (16), the writer pads the same way, whatever the new length.
        /// </summary>
        public int PaddingAlignment { get; set; }

        /// <summary>
        /// Bytes past the header's length that are not plain alignment padding, written back raw.
        /// </summary>
        public byte[] Trailing { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Typed tags that did not round-trip when read (code, reason), and so were kept raw.
        /// </summary>
        public List<string> TypedFallbacks { get; } = new();

        /// <summary>
        /// Frame rate in frames per second.
        /// </summary>
        public float FramesPerSecond => FrameRate / 256f;

        /// <summary>
        /// The file is zlib compressed (CFX/CWS).
        /// </summary>
        public bool IsCompressed => Signature == "CFX" || Signature == "CWS";

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 8)
                return false;
            var sig = br.GetASCII(0, 3);
            return sig == "GFX" || sig == "CFX" || sig == "FWS" || sig == "CWS";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            var file = br.GetBytes(0, (int)br.Length);
            Signature = Encoding.ASCII.GetString(file, 0, 3);
            Version = file[3];
            int length = (int)BitConverter.ToUInt32(file, 4);

            byte[] body;
            if (IsCompressed)
            {
                using var z = new ZLibStream(new MemoryStream(file, 8, file.Length - 8), CompressionMode.Decompress);
                using var ms = new MemoryStream();
                z.CopyTo(ms);
                body = ms.ToArray();
            }
            else
            {
                if (length < 8 || length > file.Length)
                    throw new InvalidDataException($"GFX header length {length} does not fit a {file.Length} byte file.");
                body = file[8..length];
                var tail = file[length..];
                if (tail.Length > 0 && tail.All(b => b == 0) && file.Length % 16 == 0 && tail.Length < 16)
                    PaddingAlignment = 16;
                else
                    Trailing = tail;
            }

            var r = new SwfReader(body);
            FrameSize = Rect.Read(r);
            FrameRate = r.U16();
            FrameCount = r.U16();
            Tags = ReadTags(body, r.Position, body.Length, this, out var after);
            AfterEnd = after;
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            var w = new SwfWriter();
            FrameSize.Write(w);
            w.U16(FrameRate);
            w.U16(FrameCount);
            w.Bytes(WriteTags(Tags));
            w.Bytes(AfterEnd);
            var body = w.ToArray();

            bw.WriteASCII(Signature);
            bw.WriteByte(Version);
            bw.WriteUInt32((uint)(8 + body.Length));
            if (IsCompressed)
            {
                using var ms = new MemoryStream();
                using (var z = new ZLibStream(ms, CompressionLevel.Optimal, true))
                    z.Write(body);
                bw.WriteBytes(ms.ToArray());
                return;
            }
            bw.WriteBytes(body);
            if (PaddingAlignment > 0)
            {
                int total = 8 + body.Length;
                int pad = (PaddingAlignment - total % PaddingAlignment) % PaddingAlignment;
                bw.WriteBytes(new byte[pad]);
            }
            else
            {
                bw.WriteBytes(Trailing);
            }
        }

        /// <summary>
        /// Reads a tag list (the root timeline or a sprite's) from start to its End tag (or to end).
        /// </summary>
        internal static List<Tag> ReadTags(byte[] b, int pos, int end, GFX owner, out byte[] afterEnd)
        {
            var tags = new List<Tag>();
            afterEnd = Array.Empty<byte>();
            while (pos < end)
            {
                if (pos + 2 > end)
                    throw new InvalidDataException("Truncated tag header.");
                int codeAndLength = BitConverter.ToUInt16(b, pos);
                pos += 2;
                var code = (TagCode)(codeAndLength >> 6);
                int length = codeAndLength & 0x3F;
                bool longHeader = length == 0x3F;
                if (longHeader)
                {
                    length = BitConverter.ToInt32(b, pos);
                    pos += 4;
                }
                if (length < 0 || pos + length > end)
                    throw new InvalidDataException($"{code} tag runs past the end of its timeline.");
                var data = b[pos..(pos + length)];
                pos += length;

                tags.Add(CreateTag(code, longHeader, data, owner));
                if (code == TagCode.End)
                {
                    afterEnd = b[pos..end];
                    break;
                }
            }
            return tags;
        }

        private static Tag CreateTag(TagCode code, bool longHeader, byte[] data, GFX owner)
        {
            Tag typed = code switch
            {
                TagCode.PlaceObject2 or TagCode.PlaceObject3 => new PlaceObject(),
                TagCode.RemoveObject2 => new RemoveObject2(),
                TagCode.DefineSprite => new DefineSprite(),
                TagCode.DefineEditText => new DefineEditText(),
                TagCode.DefineShape or TagCode.DefineShape2 or TagCode.DefineShape3 or TagCode.DefineShape4 => new DefineShape(),
                TagCode.GfxDefineExternalImage2 => new DefineExternalImage2(),
                TagCode.FrameLabel => new FrameLabel(),
                _ => null,
            };
            if (typed != null)
            {
                typed.Code = code;
                typed.LongHeader = longHeader;
                try
                {
                    typed.Decode(data, owner);
                    if (typed.Encode().AsSpan().SequenceEqual(data))
                        return typed;
                    owner?.TypedFallbacks.Add($"{code}: re-encode differs");
                }
                catch (Exception e)
                {
                    owner?.TypedFallbacks.Add($"{code}: {e.Message}");
                }
            }
            var raw = new Tag { Code = code, LongHeader = longHeader };
            raw.Decode(data, owner);
            return raw;
        }

        /// <summary>
        /// Writes a tag list with each tag's header form (long when asked for or needed).
        /// </summary>
        internal static byte[] WriteTags(List<Tag> tags)
        {
            using var ms = new MemoryStream();
            foreach (var tag in tags)
            {
                var data = tag.Encode() ?? Array.Empty<byte>();
                int code = (int)tag.Code;
                if (code > 0x3FF)
                    throw new InvalidDataException($"Tag code {code} does not fit the header.");
                if (!tag.LongHeader && data.Length < 0x3F)
                {
                    ms.Write(BitConverter.GetBytes((ushort)(code << 6 | data.Length)));
                }
                else
                {
                    ms.Write(BitConverter.GetBytes((ushort)(code << 6 | 0x3F)));
                    ms.Write(BitConverter.GetBytes(data.Length));
                }
                ms.Write(data);
            }
            return ms.ToArray();
        }

        /// <summary>
        /// Every tag of the movie, the root timeline's and every sprite's, depth first in file order.
        /// </summary>
        public IEnumerable<Tag> AllTags()
        {
            var stack = new Stack<IEnumerator<Tag>>();
            stack.Push(Tags.GetEnumerator());
            while (stack.Count > 0)
            {
                var e = stack.Peek();
                if (!e.MoveNext())
                {
                    stack.Pop();
                    continue;
                }
                yield return e.Current;
                if (e.Current is DefineSprite s)
                    stack.Push(s.Tags.GetEnumerator());
            }
        }

        /// <summary>
        /// The characters this movie defines, by id. Imported characters (fonts from font.swf) are not included.
        /// </summary>
        public Dictionary<int, Tag> GetCharacters()
        {
            var chars = new Dictionary<int, Tag>();
            foreach (var tag in AllTags())
            {
                if (tag.DefinedCharacterId is int id)
                    chars[id] = tag;
            }
            return chars;
        }
    }
}
