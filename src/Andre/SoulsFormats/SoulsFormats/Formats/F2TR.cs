using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// Links the vertices of a FLVER to the vertices of FaceGen TRI files, so FaceGen morphs can move the game mesh.
    /// Extension: .flver2tri
    /// </summary>
    /// <remarks>
    /// Entry i belongs to FLVER mesh i and holds one index per vertex of that mesh: the TRI vertex it takes its position
    /// from (with Z negated: FLVER Z = -TRI Z). The entry's name is the TRI file.
    /// Version 1 (the facegen.fgbnd of DS1 to ER) has 12-byte entries with 32-bit offsets. Version 4 (the face parts
    /// of BB, DS3 and ER) has 40-byte entries with 64-bit offsets, and adds groups of named points.
    /// </remarks>
    public class F2TR : SoulsFile<F2TR>
    {
        /// <summary>
        /// Whether the file is big-endian.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// 1 or 4.
        /// </summary>
        public short Version { get; set; }

        /// <summary>
        /// One per FLVER mesh, in mesh order.
        /// </summary>
        public List<Entry> Entries { get; set; }

        /// <summary>
        /// Groups of points on the TRI (version 4 only; empty in version 1).
        /// </summary>
        public List<Group> Groups { get; set; }

        /// <summary>
        /// Creates an empty F2TR.
        /// </summary>
        public F2TR()
        {
            Version = 1;
            Entries = new List<Entry>();
            Groups = new List<Group>();
        }

        private bool IsWide => Version >= 4;

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
            Version = br.AssertInt16([1, 4]);
            br.AssertInt16(0);
            br.AssertInt16((short)(IsWide ? 0x18 : 0x10)); // Header size
            int entryCount = br.ReadInt16();
            br.AssertInt16((short)(IsWide ? 0x28 : 0xC)); // Entry size

            int groupCount = 0;
            if (IsWide)
            {
                groupCount = br.ReadInt16();
                br.AssertInt16(0x18); // Group size
                br.AssertInt32(0);
            }

            Entries = new List<Entry>(entryCount);
            for (int i = 0; i < entryCount; i++)
                Entries.Add(new Entry(br, IsWide));

            Groups = new List<Group>(groupCount);
            for (int i = 0; i < groupCount; i++)
                Groups.Add(new Group(br));
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
            bw.WriteInt16(Version);
            bw.WriteInt16(0);
            bw.WriteInt16((short)(IsWide ? 0x18 : 0x10));
            bw.WriteInt16((short)Entries.Count);
            bw.WriteInt16((short)(IsWide ? 0x28 : 0xC));

            if (IsWide)
            {
                bw.WriteInt16((short)Groups.Count);
                bw.WriteInt16(0x18);
                bw.WriteInt32(0);
            }

            for (int i = 0; i < Entries.Count; i++)
                Entries[i].Write(bw, i, IsWide);

            if (IsWide)
            {
                for (int i = 0; i < Groups.Count; i++)
                    Groups[i].Write(bw, i);
            }

            for (int i = 0; i < Entries.Count; i++)
                Entries[i].WriteIndices(bw, i, IsWide);

            if (IsWide)
            {
                // The 2-byte indices are padded to a 4-byte boundary, then come the per-vertex arrays of every entry
                // (all of the first kind, then all of the second), then the group points
                bw.Pad(4);
                for (int i = 0; i < Entries.Count; i++)
                    Entries[i].WriteExtraData(bw, i, first: true);
                for (int i = 0; i < Entries.Count; i++)
                    Entries[i].WriteExtraData(bw, i, first: false);
                for (int i = 0; i < Groups.Count; i++)
                    Groups[i].WritePoints(bw, i);
            }

            for (int i = 0; i < Entries.Count; i++)
                Entries[i].WriteName(bw, i, IsWide);

            if (IsWide)
            {
                for (int i = 0; i < Groups.Count; i++)
                    Groups[i].WriteName(bw, i);
            }
        }

        /// <summary>
        /// The TRI vertex of each vertex of one FLVER mesh.
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// The TRI file the indices point into.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// For each vertex of the FLVER mesh, the index of its TRI vertex.
            /// </summary>
            public List<short> Indices { get; set; }

            /// <summary>
            /// Version 4 only: a 4-byte value per vertex, or null. Present on meshes that sit off the TRI surface
            /// (e.g. the extra 349-vertex mesh of ER FG_A_0130). Meaning not yet known: three bytes and a zero.
            /// </summary>
            public List<int> ExtraData { get; set; }

            /// <summary>
            /// Version 4 only: a second 4-byte value per vertex, or null (BB and DS3 heads).
            /// </summary>
            public List<int> ExtraData2 { get; set; }

            /// <summary>
            /// Creates an empty Entry.
            /// </summary>
            public Entry()
            {
                Name = "";
                Indices = new List<short>();
            }

            internal Entry(BinaryReaderEx br, bool wide)
            {
                long nameOffset;
                long indicesOffset;
                long extraOffset = 0;
                long extraOffset2 = 0;
                int indexCount;
                if (wide)
                {
                    nameOffset = br.ReadInt64();
                    indicesOffset = br.ReadInt64();
                    extraOffset = br.ReadInt64();
                    extraOffset2 = br.ReadInt64();
                    indexCount = (int)br.ReadInt64();
                }
                else
                {
                    nameOffset = br.ReadInt32();
                    indicesOffset = br.ReadInt32();
                    indexCount = br.ReadInt16(); // Confirmed short from BE file
                    br.AssertInt16(0);
                }

                Name = br.GetUTF16(nameOffset);
                Indices = new List<short>(br.GetInt16s(indicesOffset, indexCount));
                ExtraData = extraOffset != 0 ? new List<int>(br.GetInt32s(extraOffset, indexCount)) : null;
                ExtraData2 = extraOffset2 != 0 ? new List<int>(br.GetInt32s(extraOffset2, indexCount)) : null;
            }

            internal void Write(BinaryWriterEx bw, int index, bool wide)
            {
                if (wide)
                {
                    bw.ReserveInt64($"NameOffset{index}");
                    bw.ReserveInt64($"IndicesOffset{index}");
                    bw.ReserveInt64($"ExtraOffset{index}");
                    bw.ReserveInt64($"ExtraOffset2{index}");
                    bw.WriteInt64(Indices.Count);
                }
                else
                {
                    bw.ReserveInt32($"NameOffset{index}");
                    bw.ReserveInt32($"IndicesOffset{index}");
                    bw.WriteInt16((short)Indices.Count);
                    bw.WriteInt16(0);
                }
            }

            internal void WriteIndices(BinaryWriterEx bw, int index, bool wide)
            {
                if (wide)
                    bw.FillInt64($"IndicesOffset{index}", bw.Position);
                else
                    bw.FillInt32($"IndicesOffset{index}", (int)bw.Position);
                bw.WriteInt16s(Indices);
            }

            internal void WriteExtraData(BinaryWriterEx bw, int index, bool first)
            {
                List<int> data = first ? ExtraData : ExtraData2;
                string name = first ? $"ExtraOffset{index}" : $"ExtraOffset2{index}";

                if (data == null)
                {
                    bw.FillInt64(name, 0);
                    return;
                }

                FaceGenUtil.AssertLength($"Extra data of entry {Name}", Indices.Count, data.Count);
                bw.FillInt64(name, bw.Position);
                bw.WriteInt32s(data);
            }

            internal void WriteName(BinaryWriterEx bw, int index, bool wide)
            {
                if (wide)
                    bw.FillInt64($"NameOffset{index}", bw.Position);
                else
                    bw.FillInt32($"NameOffset{index}", (int)bw.Position);
                bw.WriteUTF16(Name, true);
            }
        }

        /// <summary>
        /// A set of landmark points on the TRI: each point is a copy of the TRI vertex it names (in TRI space, Z not
        /// negated). Five groups (IDs 0-4) on BB, DS3 and ER heads; groups 0 and 1 mirror each other (likely the eyes).
        /// </summary>
        public class Group
        {
            /// <summary>
            /// The TRI file the points belong to.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// A small ID; 0 to 4 in the groups seen so far.
            /// </summary>
            public short ID { get; set; }

            public List<Point> Points { get; set; }

            /// <summary>
            /// Creates an empty Group.
            /// </summary>
            public Group()
            {
                Name = "";
                Points = new List<Point>();
            }

            internal Group(BinaryReaderEx br)
            {
                long nameOffset = br.ReadInt64();
                long pointsOffset = br.ReadInt64();
                short pointCount = br.ReadInt16();
                ID = br.ReadInt16();
                br.AssertInt32(0);

                Name = br.GetUTF16(nameOffset);

                Points = new List<Point>(pointCount);
                br.StepIn(pointsOffset);
                for (int i = 0; i < pointCount; i++)
                    Points.Add(new Point { Position = br.ReadVector3(), Index = br.ReadInt32() });
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt64($"GroupNameOffset{index}");
                bw.ReserveInt64($"GroupPointsOffset{index}");
                bw.WriteInt16((short)Points.Count);
                bw.WriteInt16(ID);
                bw.WriteInt32(0);
            }

            internal void WritePoints(BinaryWriterEx bw, int index)
            {
                bw.FillInt64($"GroupPointsOffset{index}", bw.Position);
                foreach (Point point in Points)
                {
                    bw.WriteVector3(point.Position);
                    bw.WriteInt32(point.Index);
                }
            }

            internal void WriteName(BinaryWriterEx bw, int index)
            {
                bw.FillInt64($"GroupNameOffset{index}", bw.Position);
                bw.WriteUTF16(Name, true);
            }
        }

        /// <summary>
        /// A position, and the TRI vertex it is at.
        /// </summary>
        public class Point
        {
            public Vector3 Position { get; set; }

            public int Index { get; set; }
        }
    }
}
