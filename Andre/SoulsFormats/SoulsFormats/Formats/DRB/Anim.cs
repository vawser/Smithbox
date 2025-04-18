using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class DRB
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        public class Anim
        {
            /// <summary>
            /// The name of this Anim.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Anios in this Anim.
            /// </summary>
            public List<Anio> Anios { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Unknown; always 4.
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Unknown; always 4.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Unknown; always 4.
            /// </summary>
            public int Unk18 { get; set; }

            /// <summary>
            /// Unknown; always 1.
            /// </summary>
            public int Unk1C { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk20 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk24 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk28 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk2C { get; set; }

            /// <summary>
            /// Creates an Anim with default values.
            /// </summary>
            public Anim()
            {
                Name = "";
                Anios = new List<Anio>();
                Unk10 = 4;
                Unk14 = 4;
                Unk18 = 4;
                Unk1C = 1;
            }

            internal Anim(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Anio> anios)
            {
                int nameOffset = br.ReadInt32();
                int anioCount = br.ReadInt32();
                int anioOffset = br.ReadInt32();
                Unk0C = br.ReadInt32();
                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();
                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();

                Name = strings[nameOffset];
                Anios = new List<Anio>(anioCount);
                for (int i = 0; i < anioCount; i++)
                {
                    int offset = anioOffset + ANIO_SIZE * i;
                    Anios.Add(anios[offset]);
                    anios.Remove(offset);
                }
            }

            internal void Write(BinaryWriterEx bw, Dictionary<string, int> stringOffsets, Queue<int> anioOffsets)
            {
                bw.WriteInt32(stringOffsets[Name]);
                bw.WriteInt32(Anios.Count);
                bw.WriteInt32(anioOffsets.Dequeue());
                bw.WriteInt32(Unk0C);
                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);
                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);
            }

            /// <summary>
            /// Returns the name and number of Anios.
            /// </summary>
            public override string ToString()
            {
                return $"{Name}[{Anios.Count}]";
            }
        }
    }
}
