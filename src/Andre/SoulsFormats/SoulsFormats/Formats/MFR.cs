using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// An Elden Ring "Multi Forbidden Region" file: oriented boxes, one file per map, in which the game won't save the
    /// player's position or limits multiplayer. Each box is filed under the quadtree cell that holds its XZ footprint
    /// (<see cref="Entry.Cell"/>), so the game can find the boxes near a point. Coordinates are map-local, like the MSB.
    /// Extension: .mfr (loose, DCX compressed). See Claude/Documentation/MFR Format.md.
    /// </summary>
    public class MFR : SoulsFile<MFR>
    {
        /// <summary>
        /// What an entry's box blocks.
        /// </summary>
        public enum EntryType : byte
        {
            /// <summary>The player's position isn't saved inside the box.</summary>
            SaveLimit = 1,

            /// <summary>Multiplayer is limited inside the box.</summary>
            MultiplayLimit = 2,
        }

        /// <summary>
        /// Depth of the quadtree: the finest level has 2^GridDepth cells per side. 6 or 7 in vanilla.
        /// </summary>
        public int GridDepth { get; set; }

        /// <summary>
        /// Half the side of the quadtree's square, in metres.
        /// </summary>
        public float GridSize { get; set; }

        /// <summary>
        /// Centre of the quadtree. Only X and Z place the cells.
        /// </summary>
        public Vector3 Origin { get; set; }

        /// <summary>
        /// Unknown; always 0.
        /// </summary>
        public int Unk24 { get; set; }

        /// <summary>
        /// Unknown; always 74.
        /// </summary>
        public int Unk28 { get; set; }

        /// <summary>
        /// Unknown; always 0.
        /// </summary>
        public int Unk2C { get; set; }

        /// <summary>
        /// The boxes, in file order (not sorted by cell).
        /// </summary>
        public List<Entry> Entries { get; set; }

        /// <summary>
        /// Creates an empty MFR with the settings vanilla gives its empty files.
        /// </summary>
        public MFR()
        {
            GridDepth = 6;
            GridSize = 128;
            Origin = Vector3.Zero;
            Unk28 = 74;
            Entries = new List<Entry>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 0x30 && br.GetASCII(0, 4) == "MFRF";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("MFRF");
            br.AssertInt32(2);
            GridDepth = br.ReadInt32();
            GridSize = br.ReadSingle();
            Origin = br.ReadVector3();
            br.ReadInt32(); // File size
            int count = br.ReadInt32();
            Unk24 = br.ReadInt32();
            Unk28 = br.ReadInt32();
            Unk2C = br.ReadInt32();

            Entries = new List<Entry>(count);
            for (int i = 0; i < count; i++)
                Entries.Add(new Entry(br));
        }

        /// <summary>
        /// Serializes file data to a stream. Writes each entry's stored <see cref="Entry.Cell"/>; call
        /// <see cref="UpdateCells"/> first after moving, rotating or resizing boxes.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteASCII("MFRF");
            bw.WriteInt32(2);
            bw.WriteInt32(GridDepth);
            bw.WriteSingle(GridSize);
            bw.WriteVector3(Origin);
            bw.WriteInt32(0x30 + Entries.Count * 0x60);
            bw.WriteInt32(Entries.Count);
            bw.WriteInt32(Unk24);
            bw.WriteInt32(Unk28);
            bw.WriteInt32(Unk2C);

            foreach (Entry entry in Entries)
                entry.Write(bw);
        }

        /// <summary>
        /// Sets every entry's <see cref="Entry.Cell"/> from its box.
        /// </summary>
        public void UpdateCells()
        {
            foreach (Entry entry in Entries)
                entry.Cell = ComputeCell(entry);
        }

        /// <summary>
        /// The id of the smallest quadtree cell that holds the entry's XZ footprint: the cells of all coarser levels
        /// come first ((4^level - 1) / 3 of them), then the cell's Morton index (X in the even bits, Z in the odd).
        /// Matches every vanilla entry.
        /// </summary>
        public uint ComputeCell(Entry entry)
        {
            Vector3 h = entry.HalfExtents;
            float worldHalfX = Math.Abs(entry.AxisX.X) * h.X + Math.Abs(entry.AxisY.X) * h.Y + Math.Abs(entry.AxisZ.X) * h.Z;
            float worldHalfZ = Math.Abs(entry.AxisX.Z) * h.X + Math.Abs(entry.AxisY.Z) * h.Y + Math.Abs(entry.AxisZ.Z) * h.Z;

            float leaf = GridSize * 2f / MathF.Pow(2, GridDepth);
            int max = (1 << GridDepth) - 1;

            uint Cell(float v, float origin)
            {
                double raw = (GridSize + (v - origin)) / leaf;
                return raw < 0 ? 0 : raw > max ? (uint)max : (uint)raw;
            }

            Vector3 c = entry.Center;
            uint x0 = Cell(c.X - worldHalfX, Origin.X), x1 = Cell(c.X + worldHalfX, Origin.X);
            uint z0 = Cell(c.Z - worldHalfZ, Origin.Z), z1 = Cell(c.Z + worldHalfZ, Origin.Z);

            int level = GridDepth;
            while (level > 0 && (x0 != x1 || z0 != z1))
            {
                x0 >>= 1; x1 >>= 1; z0 >>= 1; z1 >>= 1;
                level--;
            }

            uint levelOffset = (uint)(((1L << (2 * level)) - 1) / 3);
            return levelOffset + (Spread(x0) | (Spread(z0) << 1));
        }

        private static uint Spread(uint v)
        {
            v &= 0xFFFF;
            v = (v | (v << 8)) & 0x00FF00FF;
            v = (v | (v << 4)) & 0x0F0F0F0F;
            v = (v | (v << 2)) & 0x33333333;
            return (v | (v << 1)) & 0x55555555;
        }

        /// <summary>
        /// One oriented box.
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// The quadtree cell the box is filed under; see <see cref="ComputeCell"/>.
            /// </summary>
            public uint Cell { get; set; }

            /// <summary>
            /// What the box blocks.
            /// </summary>
            public EntryType Type { get; set; }

            /// <summary>
            /// The three bytes after the type: 0 or 0xFF in vanilla (uninitialised padding), kept as read.
            /// </summary>
            public byte[] Padding05 { get; private set; }

            /// <summary>
            /// Half the box's size along its own axes.
            /// </summary>
            public Vector3 HalfExtents { get; set; }

            /// <summary>
            /// The W after the half extents: 0 or about 1 in vanilla, kept as read.
            /// </summary>
            public float HalfExtentsW { get; set; }

            /// <summary>
            /// The box's local X axis in map space (a unit vector).
            /// </summary>
            public Vector3 AxisX { get; set; }

            /// <summary>
            /// The box's local Y axis in map space (a unit vector).
            /// </summary>
            public Vector3 AxisY { get; set; }

            /// <summary>
            /// The box's local Z axis in map space (a unit vector). The axes are mirrored in some vanilla boxes.
            /// </summary>
            public Vector3 AxisZ { get; set; }

            /// <summary>
            /// The box's centre in map space.
            /// </summary>
            public Vector3 Center { get; set; }

            /// <summary>
            /// Creates an axis-aligned SaveLimit box of half size 1 at the origin.
            /// </summary>
            public Entry()
            {
                Type = EntryType.SaveLimit;
                Padding05 = new byte[3];
                HalfExtents = Vector3.One;
                AxisX = Vector3.UnitX;
                AxisY = Vector3.UnitY;
                AxisZ = Vector3.UnitZ;
            }

            /// <summary>
            /// The box's rotation and centre as a matrix (rows are the axes, then the centre), without its size.
            /// </summary>
            public Matrix4x4 Transform
            {
                get => new Matrix4x4(
                    AxisX.X, AxisX.Y, AxisX.Z, 0,
                    AxisY.X, AxisY.Y, AxisY.Z, 0,
                    AxisZ.X, AxisZ.Y, AxisZ.Z, 0,
                    Center.X, Center.Y, Center.Z, 1);
                set
                {
                    AxisX = new Vector3(value.M11, value.M12, value.M13);
                    AxisY = new Vector3(value.M21, value.M22, value.M23);
                    AxisZ = new Vector3(value.M31, value.M32, value.M33);
                    Center = value.Translation;
                }
            }

            internal Entry(BinaryReaderEx br)
            {
                Cell = br.ReadUInt32();
                Type = (EntryType)br.ReadByte();
                Padding05 = br.ReadBytes(3);
                br.AssertInt32(0);
                br.AssertInt32(0);
                HalfExtents = br.ReadVector3();
                HalfExtentsW = br.ReadSingle();
                AxisX = br.ReadVector3();
                br.AssertSingle(0);
                AxisY = br.ReadVector3();
                br.AssertSingle(0);
                AxisZ = br.ReadVector3();
                br.AssertSingle(0);
                Center = br.ReadVector3();
                br.AssertSingle(1);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt32(Cell);
                bw.WriteByte((byte)Type);
                bw.WriteBytes(Padding05 ?? new byte[3]);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteVector3(HalfExtents);
                bw.WriteSingle(HalfExtentsW);
                bw.WriteVector3(AxisX);
                bw.WriteSingle(0);
                bw.WriteVector3(AxisY);
                bw.WriteSingle(0);
                bw.WriteVector3(AxisZ);
                bw.WriteSingle(0);
                bw.WriteVector3(Center);
                bw.WriteSingle(1);
            }
        }
    }
}
