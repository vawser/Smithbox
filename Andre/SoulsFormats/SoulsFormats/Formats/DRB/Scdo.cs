using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class DRB
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        public class Scdo
        {
            /// <summary>
            /// The name of this Scdo.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Scdks in this Scdo.
            /// </summary>
            public List<Scdk> Scdks { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Creates a Scdo with default values.
            /// </summary>
            public Scdo()
            {
                Name = "";
                Scdks = new List<Scdk>();
            }

            internal Scdo(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Scdk> scdks)
            {
                int nameOffset = br.ReadInt32();
                int scdkCount = br.ReadInt32();
                int scdkOffset = br.ReadInt32();
                Unk0C = br.ReadInt32();

                Name = strings[nameOffset];
                Scdks = new List<Scdk>(scdkCount);
                for (int i = 0; i < scdkCount; i++)
                {
                    int offset = scdkOffset + SCDK_SIZE * i;
                    Scdks.Add(scdks[offset]);
                    scdks.Remove(offset);
                }
            }

            internal void Write(BinaryWriterEx bw, Dictionary<string, int> stringOffsets, Queue<int> scdkOffsets)
            {
                bw.WriteInt32(stringOffsets[Name]);
                bw.WriteInt32(Scdks.Count);
                bw.WriteInt32(scdkOffsets.Dequeue());
                bw.WriteInt32(Unk0C);
            }

            /// <summary>
            /// Returns the name and number of Scdks.
            /// </summary>
            public override string ToString()
            {
                return $"{Name}[{Scdks.Count}]";
            }
        }
    }
}
