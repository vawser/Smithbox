using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A format that indicates which vertices of a FLVER are relevant for FaceGen. Extension: .flver2tri
    /// </summary>
    public class F2TR : SoulsFile<F2TR>
    {
        /// <summary>
        /// Whether the file is big-endian.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Entries in the file, probably meshes.
        /// </summary>
        public List<Entry> Entries { get; set; }

        /// <summary>
        /// Creates an empty F2TR.
        /// </summary>
        public F2TR()
        {
            Entries = new List<Entry>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "F2TR";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("F2TR");
            BigEndian = br.AssertByte([0, 0xFF]) == 0xFF;
            br.BigEndian = BigEndian;
            br.AssertByte(0);
            br.AssertInt16(1);
            br.AssertInt16(0);
            br.AssertInt16(0x10); // Header size?
            int entryCount = br.ReadInt16(); // Not actually confirmed
            br.AssertInt16(0xC); // Entry size?

            Entries = new List<Entry>(entryCount);
            for (int i = 0; i < entryCount; i++)
                Entries.Add(new Entry(br));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            bw.WriteASCII("F2TR");
            bw.WriteByte((byte)(BigEndian ? 0xFF : 0));
            bw.WriteByte(0);
            bw.WriteInt16(1);
            bw.WriteInt16(0);
            bw.WriteInt16(0x10);
            bw.WriteInt16((short)Entries.Count);
            bw.WriteInt16(0xC);

            for (int i = 0; i < Entries.Count; i++)
                Entries[i].Write(bw, i);

            for (int i = 0; i < Entries.Count; i++)
                Entries[i].WriteIndices(bw, i);

            for (int i = 0; i < Entries.Count; i++)
                Entries[i].WriteName(bw, i);
        }

        /// <summary>
        /// A collection of indices, probably corresponding to a mesh.
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// Name of the relevant FaceGen file, I think.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Presumably vertex indices in the flver.
            /// </summary>
            public List<short> Indices { get; set; }

            /// <summary>
            /// Creates an empty Entry.
            /// </summary>
            public Entry()
            {
                Name = "";
                Indices = new List<short>();
            }

            internal Entry(BinaryReaderEx br)
            {
                int nameOffset = br.ReadInt32();
                int indicesOffset = br.ReadInt32();
                short indexCount = br.ReadInt16(); // Confirmed short from BE file
                br.AssertInt16(0);

                Name = br.GetUTF16(nameOffset);
                Indices = new List<short>(br.GetInt16s(indicesOffset, indexCount));
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"NameOffset{index}");
                bw.ReserveInt32($"IndicesOffset{index}");
                bw.WriteInt16((short)Indices.Count);
                bw.WriteInt16(0);
            }

            internal void WriteIndices(BinaryWriterEx bw, int index)
            {
                bw.FillInt32($"IndicesOffset{index}", (int)bw.Position);
                bw.WriteInt16s(Indices);
            }

            internal void WriteName(BinaryWriterEx bw, int index)
            {
                bw.FillInt32($"NameOffset{index}", (int)bw.Position);
                bw.WriteUTF16(Name, true);
            }
        }
    }
}
