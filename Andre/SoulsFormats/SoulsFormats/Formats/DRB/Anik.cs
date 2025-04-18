using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class DRB
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        public class Anik
        {
            /// <summary>
            /// The name of this Anik.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk04 { get; set; }

            /// <summary>
            /// Unknown; always 0-1.
            /// </summary>
            public byte Unk08 { get; set; }

            /// <summary>
            /// Unknown; always 1-2.
            /// </summary>
            public byte Unk09 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public short Unk0A { get; set; }

            /// <summary>
            /// An offset into the INTP block.
            /// </summary>
            public int IntpOffset { get; set; }

            /// <summary>
            /// An offset into the ANIP block.
            /// </summary>
            public int AnipOffset { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk18 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk1C { get; set; }

            internal Anik(BinaryReaderEx br, Dictionary<int, string> strings)
            {
                int nameOffset = br.ReadInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadByte();
                Unk09 = br.ReadByte();
                Unk0A = br.ReadInt16();
                IntpOffset = br.ReadInt32();
                AnipOffset = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Name = strings[nameOffset];
            }

            internal void Write(BinaryWriterEx bw, Dictionary<string, int> stringOffsets)
            {
                bw.WriteInt32(stringOffsets[Name]);
                bw.WriteInt32(Unk04);
                bw.WriteByte(Unk08);
                bw.WriteByte(Unk09);
                bw.WriteInt16(Unk0A);
                bw.WriteInt32(IntpOffset);
                bw.WriteInt32(AnipOffset);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);
            }

            /// <summary>
            /// Returns the name of this Anik.
            /// </summary>
            public override string ToString()
            {
                return $"{Name}";
            }
        }
    }
}
