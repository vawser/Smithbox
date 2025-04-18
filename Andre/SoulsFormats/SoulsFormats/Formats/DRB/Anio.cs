using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class DRB
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        public class Anio
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk00 { get; set; }

            /// <summary>
            /// Aniks in this Anio.
            /// </summary>
            public List<Anik> Aniks { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Creates an Anio with default values.
            /// </summary>
            public Anio()
            {
                Aniks = new List<Anik>();
            }

            internal Anio(BinaryReaderEx br, Dictionary<int, Anik> aniks)
            {
                Unk00 = br.ReadInt32();
                int anikCount = br.ReadInt32();
                int anikOffset = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Aniks = new List<Anik>(anikCount);
                for (int i = 0; i < anikCount; i++)
                {
                    int offset = anikOffset + ANIK_SIZE * i;
                    Aniks.Add(aniks[offset]);
                    aniks.Remove(offset);
                }
            }

            internal void Write(BinaryWriterEx bw, Queue<int> anikOffsets)
            {
                bw.WriteInt32(Unk00);
                bw.WriteInt32(Aniks.Count);
                bw.WriteInt32(anikOffsets.Dequeue());
                bw.WriteInt32(Unk0C);
            }

            /// <summary>
            /// Returns the number of Aniks in this Anio.
            /// </summary>
            public override string ToString()
            {
                return $"Anio[{Aniks.Count}]";
            }
        }
    }
}
