using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A DS2 file that holds the break data of the breakable objects of a map. Unlike <see cref="BREAKOBJ"/>,
    /// it lists only the objects that have break data, each by the ObjectInstanceID of its MSB part.
    /// Extension: .breakobj
    /// </summary>
    public class BREAKOBJ2 : SoulsFile<BREAKOBJ2>
    {
        /// <summary>
        /// The break data of the objects, in the order they appear in the file.
        /// </summary>
        public List<Entry> Entries { get; set; }

        /// <summary>
        /// Creates an empty BREAKOBJ2.
        /// </summary>
        public BREAKOBJ2()
        {
            Entries = new List<Entry>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 0xC && br.GetASCII(0, 4) == "OBJB" && br.GetInt32(4) == 1;
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("OBJB");
            br.AssertInt32(1);
            int count = br.ReadInt32();

            Entries = new List<Entry>(count);
            for (int i = 0; i < count; i++)
                Entries.Add(new Entry(br));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteASCII("OBJB");
            bw.WriteInt32(1);
            bw.WriteInt32(Entries.Count);

            foreach (Entry entry in Entries)
                entry.Write(bw);
        }

        /// <summary>
        /// The break data of one object.
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// The ObjectInstanceID of the MSB part, which is the ID of its MapObjectInstanceParam row.
            /// </summary>
            public uint ObjectInstanceID { get; set; }

            /// <summary>
            /// Unknown; 0 in DS2, and a hash-like value in DS2S that repeats across maps.
            /// </summary>
            public int Unk04 { get; set; }

            /// <summary>
            /// Unknown; usually 1, 0 for some models with only one or two pieces, and 2 for a few.
            /// The same for every instance of a model.
            /// </summary>
            public int Unk08 { get; set; }

            /// <summary>
            /// One record per piece of the model; see <see cref="BreakPieces.Pieces"/>.
            /// </summary>
            public List<ulong> Pieces { get; set; }

            /// <summary>
            /// Creates an Entry with no pieces.
            /// </summary>
            public Entry()
            {
                Unk08 = 1;
                Pieces = new List<ulong>();
            }

            /// <summary>
            /// Creates a deep copy of the entry.
            /// </summary>
            public Entry DeepCopy()
            {
                return new Entry { ObjectInstanceID = ObjectInstanceID, Unk04 = Unk04, Unk08 = Unk08, Pieces = new List<ulong>(Pieces) };
            }

            internal Entry(BinaryReaderEx br)
            {
                ObjectInstanceID = br.ReadUInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                int count = br.ReadInt32();

                Pieces = new List<ulong>(count);
                for (int i = 0; i < count; i++)
                    Pieces.Add(br.ReadUInt64());
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt32(ObjectInstanceID);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Pieces.Count);
                foreach (ulong piece in Pieces)
                    bw.WriteUInt64(piece);
            }
        }
    }
}
