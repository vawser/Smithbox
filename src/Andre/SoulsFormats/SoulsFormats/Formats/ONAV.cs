using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// The sound occlusion graph of an Elden Ring or Armored Core VI map: the map's MSB MufflingBox and MufflingPortal
    /// regions, compiled into rooms (areas), their portals (layers) and the doorways between rooms (links).
    /// The game's sound engine (DLMOW::MOOcclusionInstance, through MSPOcclusionGraph and MSPLayerPathFinder_AStar) places
    /// the emitter and the listener in rooms, then finds a route between them through the portals, which drives the
    /// Wwise occlusion of the sound. The graphs of all loaded maps are queried together. Maps without muffling regions
    /// have an empty file.
    /// The file is a memory image: 64 bit offsets that the game turns into pointers from a relocation table.
    /// Extension: .onav, named after the map, in /map/onav.
    /// </summary>
    public class ONAV : SoulsFile<ONAV>
    {
        /// <summary>
        /// The size of a cell mask: one bit for each of the 4096 cells of the grid.
        /// </summary>
        public const int CellMaskSize = 0x200;

        /// <summary>
        /// The major format version: 3 in Elden Ring, 2 in Armored Core VI. Version 2 differs only in its layers
        /// (0x28 bytes instead of 0x38). AC6's exe only loads 3.2, the same as ER's, so it rejects its own 2.1 files.
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// The minor format version: 2 in most Elden Ring files, and 1 in 26 of them (laid out the same). Every Armored
        /// Core VI file has 1. The game only loads 2.
        /// </summary>
        public byte SubVersion { get; set; }

        /// <summary>
        /// The maximum corner of the bounds the grid covers. An empty file keeps the initial value, the smallest normal float
        /// (1.17549435E-38) on each axis.
        /// </summary>
        public Vector3 BoundsMax { get; set; }

        /// <summary>
        /// The minimum corner of the bounds the grid covers. An empty file keeps the initial value, float.MaxValue.
        /// </summary>
        public Vector3 BoundsMin { get; set; }

        /// <summary>
        /// The number of grid cells along X; the product of the three counts is always 4096, the bits of a cell mask.
        /// The grid is the broad phase of the point-in-room lookup.
        /// </summary>
        public int GridCountX { get; set; }

        /// <summary>
        /// The number of grid cells along Y.
        /// </summary>
        public int GridCountY { get; set; }

        /// <summary>
        /// The number of grid cells along Z.
        /// </summary>
        public int GridCountZ { get; set; }

        /// <summary>
        /// The rooms of the graph, one for each MufflingBox that isn't an extra volume. The game skips a file with none.
        /// </summary>
        public List<Area> Areas { get; set; }

        /// <summary>
        /// Extra MufflingBoxes of rooms, stored inline after the areas: each adds to or cuts out of the room whose
        /// <see cref="AreaBase.ID"/> it has. Present in about 20 Elden Ring maps. The game tests them before the areas.
        /// </summary>
        public List<Volume> Volumes { get; set; }

        /// <summary>
        /// Creates an empty ONAV, as the game files have it.
        /// </summary>
        public ONAV()
        {
            Version = 3;
            SubVersion = 2;
            BoundsMax = new Vector3(BitConverter.Int32BitsToSingle(0x00800000));
            BoundsMin = new Vector3(float.MaxValue);
            GridCountX = 16;
            GridCountY = 16;
            GridCountZ = 16;
            Areas = new List<Area>();
            Volumes = new List<Volume>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 0x74 && br.GetASCII(0, 4) == "ONAV";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("ONAV");
            Version = br.AssertByte([2, 3]);
            SubVersion = br.ReadByte();
            br.AssertInt16(0);
            br.AssertInt32(0);
            br.ReadInt32(); // Relocation count
            long volumesOffset = br.ReadInt64();
            long unk18Offset = br.ReadInt64();
            long areaPointersOffset = br.ReadInt64();
            br.ReadInt64(); // Relocation table offset

            br.AssertUInt16(0);
            br.AssertUInt16(0); // The count of the always empty array at 0x18, presumably
            int areaCount = br.ReadUInt16();
            int volumeCount = br.ReadUInt16();
            br.AssertPattern(0x18, 0);

            BoundsMax = br.ReadVector3();
            BoundsMin = br.ReadVector3();
            GridCountX = br.ReadInt32();
            GridCountY = br.ReadInt32();
            GridCountZ = br.ReadInt32();

            if (unk18Offset != volumesOffset + volumeCount * Volume.Size)
                throw new InvalidDataException("The ONAV array at 0x18 is not empty; its layout is unknown.");

            long[] areaOffsets = br.GetInt64s(areaPointersOffset, areaCount);
            Areas = new List<Area>(areaCount);
            foreach (long offset in areaOffsets)
            {
                br.Position = offset;
                Areas.Add(new Area(br, Version));
            }

            br.Position = volumesOffset;
            Volumes = new List<Volume>(volumeCount);
            for (int i = 0; i < volumeCount; i++)
                Volumes.Add(new Volume(br));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            if (Areas.Count > ushort.MaxValue || Volumes.Count > ushort.MaxValue)
                throw new InvalidDataException("ONAV counts are 16 bit.");
            if (Version != 2 && Version != 3)
                throw new InvalidDataException($"ONAV version {Version} is not supported; only 2 and 3 are.");

            // Offsets the game relocates, in the order the game files list them
            var relocations = new List<long> { 0x10, 0x18, 0x20 };

            bw.BigEndian = false;
            bw.WriteASCII("ONAV");
            bw.WriteByte(Version);
            bw.WriteByte(SubVersion);
            bw.WriteInt16(0);
            bw.WriteInt32(0);
            bw.ReserveInt32("RelocationCount");
            bw.ReserveInt64("VolumesOffset");
            bw.ReserveInt64("Unk18Offset");
            bw.ReserveInt64("AreaPointersOffset");
            bw.ReserveInt64("RelocationTableOffset");

            bw.WriteUInt16(0);
            bw.WriteUInt16(0);
            bw.WriteUInt16((ushort)Areas.Count);
            bw.WriteUInt16((ushort)Volumes.Count);
            bw.WritePattern(0x18, 0);

            bw.WriteVector3(BoundsMax);
            bw.WriteVector3(BoundsMin);
            bw.WriteInt32(GridCountX);
            bw.WriteInt32(GridCountY);
            bw.WriteInt32(GridCountZ);

            var areaOffsets = new List<long>(Areas.Count);
            foreach (Area area in Areas)
            {
                areaOffsets.Add(bw.Position);
                area.Write(bw, relocations, Version);
            }

            bw.FillInt64("VolumesOffset", bw.Position);
            foreach (Volume volume in Volumes)
                volume.Write(bw);

            bw.FillInt64("Unk18Offset", bw.Position);

            // Not relocated through the table: the game adds the base to each of these itself
            bw.FillInt64("AreaPointersOffset", bw.Position);
            foreach (long offset in areaOffsets)
                bw.WriteInt64(offset);

            bw.FillInt64("RelocationTableOffset", bw.Position);
            bw.FillInt32("RelocationCount", relocations.Count);
            foreach (long offset in relocations)
                bw.WriteInt64(offset);
        }

        private static byte[] ReadCellMask(BinaryReaderEx br) => br.ReadBytes(CellMaskSize);

        private static void WriteCellMask(BinaryWriterEx bw, byte[] mask)
        {
            if (mask == null || mask.Length != CellMaskSize)
                throw new InvalidDataException($"An ONAV cell mask must be {CellMaskSize} bytes.");
            bw.WriteBytes(mask);
        }

        /// <summary>
        /// The shape of an area's volume, at area byte +0x2F.
        /// </summary>
        public enum AreaShape : byte
        {
            /// <summary>
            /// An oriented box; every Elden Ring area. Its half extents are <see cref="AreaBase.Scale"/> × 65535 × 0.5.
            /// </summary>
            Box = 0,

            /// <summary>
            /// Unknown; never used. The game's distance test reads area +0x10 for it.
            /// </summary>
            Unk1 = 1,

            /// <summary>
            /// A sphere of radius <see cref="AreaBase.Scale"/>.X; never used.
            /// </summary>
            Sphere = 2,
        }

        /// <summary>
        /// The fields an area and a volume share: the box of a MufflingBox region.
        /// </summary>
        public abstract class AreaBase
        {
            /// <summary>
            /// The room's ID, which links refer to. A volume has the ID of the room it belongs to.
            /// </summary>
            public ushort ID { get; set; }

            /// <summary>
            /// The position of the box: the MufflingBox's MSB position, the centre of its base.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Euler rotation in radians: the MufflingBox's rotation, converted from degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// The size of the box divided by 65535: the MufflingBox's width, height and depth are Scale × 65535.
            /// The game multiplies it by 65535 × 0.5 for the half extents.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// One bit for each of the 4096 grid cells of the file: the cells the box overlaps, which the game tests
            /// before the box itself.
            /// </summary>
            public byte[] CellMask { get; set; }

            internal AreaBase()
            {
                CellMask = new byte[CellMaskSize];
            }

            internal void ReadCommon(BinaryReaderEx br, out byte byte02, out byte byte03)
            {
                ID = br.ReadUInt16();
                byte02 = br.ReadByte();
                byte03 = br.ReadByte();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
            }

            internal void WriteCommon(BinaryWriterEx bw, byte byte02, byte byte03)
            {
                bw.WriteUInt16(ID);
                bw.WriteByte(byte02);
                bw.WriteByte(byte03);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);
            }
        }

        /// <summary>
        /// A room of the graph: a MufflingBox, with its portals (<see cref="Layers"/>) and the doorways to other rooms
        /// (<see cref="Links"/>). Byte +3 is the room's total number of layer entries, written from <see cref="Layers"/>.
        /// </summary>
        public class Area : AreaBase
        {
            /// <summary>
            /// Unknown; 0 to 3. The game returns it with every point-in-room lookup; it matches nothing in the MSB.
            /// </summary>
            public ushort Unk2A { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public byte Unk2C { get; set; }

            /// <summary>
            /// When set, the game tests the box for every point, without checking <see cref="AreaBase.CellMask"/> (which
            /// is then full), and reports the flag with the lookup. Set on 88 Elden Ring rooms, mostly small doorway boxes.
            /// </summary>
            public bool IgnoreCellMask { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public byte Unk2E { get; set; }

            /// <summary>
            /// The shape of the room's volume; always <see cref="AreaShape.Box"/>.
            /// </summary>
            public AreaShape Shape { get; set; }

            /// <summary>
            /// Unknown; 0 or 1, always the same as <see cref="IgnoreCellMask"/>.
            /// </summary>
            public int Unk30 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk34 { get; set; }

            /// <summary>
            /// Unknown; 0 or 1, always the same as <see cref="IgnoreCellMask"/>.
            /// </summary>
            public int Unk38 { get; set; }

            /// <summary>
            /// Unknown; always 1.
            /// </summary>
            public ushort Unk3C { get; set; }

            /// <summary>
            /// The index of the room's transform among those the game builds; always the area's index in the file.
            /// </summary>
            public ushort TransformIndex { get; set; }

            /// <summary>
            /// The room's portals, one for each of its MufflingPortals; at most 255.
            /// </summary>
            public List<Layer> Layers { get; set; }

            /// <summary>
            /// The doorways to other rooms. Every link has a reverse one in the room it leads to.
            /// </summary>
            public List<Link> Links { get; set; }

            /// <summary>
            /// Creates an Area with default values.
            /// </summary>
            public Area()
            {
                Unk3C = 1;
                Layers = new List<Layer>();
                Links = new List<Link>();
            }

            internal Area(BinaryReaderEx br, byte version)
            {
                // Byte +3, the total entry count, is rebuilt from the layers on write
                ReadCommon(br, out byte layerCount, out _);
                int linkCount = br.ReadUInt16();
                Unk2A = br.ReadUInt16();
                Unk2C = br.ReadByte();
                IgnoreCellMask = br.ReadBoolean();
                Unk2E = br.ReadByte();
                Shape = br.ReadEnum8<AreaShape>();
                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                Unk38 = br.ReadInt32();
                Unk3C = br.ReadUInt16();
                TransformIndex = br.ReadUInt16();
                CellMask = ReadCellMask(br);
                long linksOffset = br.ReadInt64();

                // A version 2 layer has no known entry count: its entries end where the next list starts
                // (the next layer's entries, or the area's links), so they must be empty to be read safely
                long[] entriesOffsets = new long[layerCount];
                Layers = new List<Layer>(layerCount);
                for (int i = 0; i < layerCount; i++)
                    Layers.Add(new Layer(br, version, out entriesOffsets[i]));

                if (version == 2)
                {
                    for (int i = 0; i < layerCount; i++)
                    {
                        long end = i + 1 < layerCount ? entriesOffsets[i + 1] : linksOffset;
                        if (entriesOffsets[i] != end)
                            throw new InvalidDataException("An ONAV version 2 layer has entries; their count's location is unknown.");
                    }
                }

                br.Position = linksOffset;
                Links = new List<Link>(linkCount);
                for (int i = 0; i < linkCount; i++)
                    Links.Add(new Link(br));
            }

            internal void Write(BinaryWriterEx bw, List<long> relocations, byte version)
            {
                if (Layers.Count > byte.MaxValue)
                    throw new InvalidDataException("An ONAV area can have at most 255 layers.");
                if (version == 2 && Layers.Exists(l => l.Entries.Count > 0))
                    throw new InvalidDataException("An ONAV version 2 layer can't have entries; their count's location is unknown.");
                if (Links.Count > ushort.MaxValue)
                    throw new InvalidDataException("An ONAV area can have at most 65535 links.");

                int entryCount = 0;
                foreach (Layer layer in Layers)
                    entryCount += layer.Entries.Count;
                if (entryCount > byte.MaxValue)
                    throw new InvalidDataException("An ONAV area can have at most 255 layer entries in all.");

                WriteCommon(bw, (byte)Layers.Count, (byte)entryCount);
                bw.WriteUInt16((ushort)Links.Count);
                bw.WriteUInt16(Unk2A);
                bw.WriteByte(Unk2C);
                bw.WriteBoolean(IgnoreCellMask);
                bw.WriteByte(Unk2E);
                bw.WriteByte((byte)Shape);
                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(Unk38);
                bw.WriteUInt16(Unk3C);
                bw.WriteUInt16(TransformIndex);
                WriteCellMask(bw, CellMask);

                long linksPointer = bw.Position;
                relocations.Add(linksPointer);
                bw.WriteInt64(0);

                var layerPointers = new long[Layers.Count];
                for (int i = 0; i < Layers.Count; i++)
                {
                    layerPointers[i] = Layers[i].Write(bw, version);
                    relocations.Add(layerPointers[i]);
                }

                // Each layer's entries, then the links; an empty list points at where it would start
                for (int i = 0; i < Layers.Count; i++)
                {
                    FillOffset(bw, layerPointers[i]);
                    foreach (Layer.Entry entry in Layers[i].Entries)
                        entry.Write(bw);
                }

                FillOffset(bw, linksPointer);
                foreach (Link link in Links)
                    link.Write(bw);
            }

            private static void FillOffset(BinaryWriterEx bw, long pointer)
            {
                long position = bw.Position;
                bw.Position = pointer;
                bw.WriteInt64(position);
                bw.Position = position;
            }

            /// <summary>
            /// Returns a string representation of the area.
            /// </summary>
            public override string ToString()
            {
                return $"Area {ID} [{Layers.Count} Layers][{Links.Count} Links]";
            }
        }

        /// <summary>
        /// A portal of a room, one of its MufflingPortals: a node of the route search. Links name the layer at each end
        /// of a doorway, and the layer's entries are the paths through the room to its other portals.
        /// 0x38 bytes in version 3; 0x28 in version 2, which keeps <see cref="Index"/>, <see cref="Unk01"/> and
        /// <see cref="Unk02"/>, then has its own fields (the V2 properties) and the entries offset at +0x20. The other
        /// properties are only written in version 3, and the V2 ones only in 2.
        /// </summary>
        public class Layer
        {
            /// <summary>
            /// Version 2 only. Unknown; 0xFFC2 in the one Armored Core VI layer (m50_50_00_00).
            /// </summary>
            public int V2Unk04 { get; set; }

            /// <summary>
            /// Version 2 only. Unknown; 0.
            /// </summary>
            public int V2Unk08 { get; set; }

            /// <summary>
            /// Version 2 only. Unknown; 0x02000000 (byte +0x0F is 2, maybe the counterpart of <see cref="Unk29"/>).
            /// </summary>
            public int V2Unk0C { get; set; }

            /// <summary>
            /// Version 2 only. Unknown; 0.
            /// </summary>
            public int V2Unk10 { get; set; }

            /// <summary>
            /// Version 2 only. Unknown; 0.
            /// </summary>
            public int V2Unk14 { get; set; }

            /// <summary>
            /// Version 2 only. Unknown; 0.
            /// </summary>
            public int V2Unk18 { get; set; }

            /// <summary>
            /// Version 2 only. Unknown; 0.
            /// </summary>
            public int V2Unk1C { get; set; }

            /// <summary>
            /// The index of the layer within its room, as <see cref="Link.Layers"/> and <see cref="Entry.Layers"/> refer to it.
            /// </summary>
            public byte Index { get; set; }

            /// <summary>
            /// Unknown; always 0. When set the game adds the layer to a separate list.
            /// </summary>
            public byte Unk01 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public short Unk02 { get; set; }

            /// <summary>
            /// The position of the portal (its MufflingPortal point) in the room's local frame: relative to the centre
            /// of the room's box, rotated by the inverse of the room's rotation.
            /// </summary>
            public Vector3 PortalPosition { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Unknown; small multiples of 5, or 0. Set with <see cref="Unk18"/> and <see cref="Unk28"/> on about a third of
            /// the portals; presumably an occlusion parameter from MufflingPortal data SoulsFormats doesn't decode.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Unknown; 0 to 15. See <see cref="Unk14"/>.
            /// </summary>
            public int Unk18 { get; set; }

            /// <summary>
            /// Unknown; counts up from about 7500 across the portals of a file, presumably an ID.
            /// </summary>
            public int Unk1C { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk20 { get; set; }

            /// <summary>
            /// Unknown; always 0. When set the game uses it as a key into a map of the layers.
            /// </summary>
            public int Unk24 { get; set; }

            /// <summary>
            /// Unknown; 0 or a multiple of 10 up to 200. See <see cref="Unk14"/>.
            /// </summary>
            public byte Unk28 { get; set; }

            /// <summary>
            /// Unknown; always 2.
            /// </summary>
            public byte Unk29 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public byte Unk2B { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk2C { get; set; }

            /// <summary>
            /// The paths from this portal through the room to its other portals; at most 255.
            /// </summary>
            public List<Entry> Entries { get; set; }

            /// <summary>
            /// Creates a Layer with default values.
            /// </summary>
            public Layer()
            {
                Unk29 = 2;
                Entries = new List<Entry>();
            }

            internal Layer(BinaryReaderEx br, byte version, out long entriesOffset)
            {
                Index = br.ReadByte();
                Unk01 = br.ReadByte();
                Unk02 = br.ReadInt16();
                if (version == 2)
                {
                    V2Unk04 = br.ReadInt32();
                    V2Unk08 = br.ReadInt32();
                    V2Unk0C = br.ReadInt32();
                    V2Unk10 = br.ReadInt32();
                    V2Unk14 = br.ReadInt32();
                    V2Unk18 = br.ReadInt32();
                    V2Unk1C = br.ReadInt32();
                    entriesOffset = br.ReadInt64();
                    Unk29 = 2;
                    Entries = new List<Entry>();
                    return;
                }

                PortalPosition = br.ReadVector3();
                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();
                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();
                Unk28 = br.ReadByte();
                Unk29 = br.ReadByte();
                int entryCount = br.ReadByte();
                Unk2B = br.ReadByte();
                Unk2C = br.ReadInt32();
                entriesOffset = br.ReadInt64();

                Entries = new List<Entry>(entryCount);
                br.StepIn(entriesOffset);
                for (int i = 0; i < entryCount; i++)
                    Entries.Add(new Entry(br));
                br.StepOut();
            }

            /// <summary>
            /// Writes the layer, returning the offset of its entries pointer for the caller to fill.
            /// </summary>
            internal long Write(BinaryWriterEx bw, byte version)
            {
                if (Entries.Count > byte.MaxValue)
                    throw new InvalidDataException("An ONAV layer can have at most 255 entries.");

                long pointer;
                bw.WriteByte(Index);
                bw.WriteByte(Unk01);
                bw.WriteInt16(Unk02);
                if (version == 2)
                {
                    bw.WriteInt32(V2Unk04);
                    bw.WriteInt32(V2Unk08);
                    bw.WriteInt32(V2Unk0C);
                    bw.WriteInt32(V2Unk10);
                    bw.WriteInt32(V2Unk14);
                    bw.WriteInt32(V2Unk18);
                    bw.WriteInt32(V2Unk1C);
                    pointer = bw.Position;
                    bw.WriteInt64(0);
                    return pointer;
                }

                bw.WriteVector3(PortalPosition);
                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);
                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);
                bw.WriteByte(Unk28);
                bw.WriteByte(Unk29);
                bw.WriteByte((byte)Entries.Count);
                bw.WriteByte(Unk2B);
                bw.WriteInt32(Unk2C);
                pointer = bw.Position;
                bw.WriteInt64(0);
                return pointer;
            }

            /// <summary>
            /// Returns a string representation of the layer.
            /// </summary>
            public override string ToString()
            {
                return $"Layer {Index} [{Entries.Count} Entries]";
            }

            /// <summary>
            /// A path through the room, between this portal and another of the same room.
            /// </summary>
            public class Entry
            {
                /// <summary>
                /// Two layer indices of the room: the low nibble and the high nibble. The game reads the high one.
                /// </summary>
                public byte Layers { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public byte Unk01 { get; set; }

                /// <summary>
                /// Unknown; 0 or 2.
                /// </summary>
                public byte Unk02 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public byte Unk03 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Creates an Entry with default values.
                /// </summary>
                public Entry() { }

                internal Entry(BinaryReaderEx br)
                {
                    Layers = br.ReadByte();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteByte(Layers);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(Unk02);
                    bw.WriteByte(Unk03);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }
        }

        /// <summary>
        /// A doorway from a portal of this room to a portal of another: two MufflingPortal points, one on each side.
        /// </summary>
        public class Link
        {
            /// <summary>
            /// The <see cref="AreaBase.ID"/> of the room linked to.
            /// </summary>
            public ushort AreaID { get; set; }

            /// <summary>
            /// The low nibble is the <see cref="Layer.Index"/> of the portal in this room; the high nibble is the
            /// portal in the room linked to.
            /// </summary>
            public byte Layers { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public byte Unk03 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk04 { get; set; }

            /// <summary>
            /// The portal in this room: the low nibble of <see cref="Layers"/>.
            /// </summary>
            public int LayerIndex
            {
                get => Layers & 0xF;
                set => Layers = (byte)((Layers & 0xF0) | (value & 0xF));
            }

            /// <summary>
            /// The portal in the room linked to: the high nibble of <see cref="Layers"/>.
            /// </summary>
            public int TargetLayerIndex
            {
                get => Layers >> 4;
                set => Layers = (byte)((Layers & 0x0F) | ((value & 0xF) << 4));
            }

            /// <summary>
            /// Creates a Link with default values.
            /// </summary>
            public Link() { }

            internal Link(BinaryReaderEx br)
            {
                AreaID = br.ReadUInt16();
                Layers = br.ReadByte();
                Unk03 = br.ReadByte();
                Unk04 = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt16(AreaID);
                bw.WriteByte(Layers);
                bw.WriteByte(Unk03);
                bw.WriteInt32(Unk04);
            }

            /// <summary>
            /// Returns a string representation of the link.
            /// </summary>
            public override string ToString()
            {
                return $"-> Area {AreaID} (layer {LayerIndex} -> {TargetLayerIndex})";
            }
        }

        /// <summary>
        /// An extra MufflingBox of a room. A point inside it belongs to the room whose <see cref="AreaBase.ID"/> it has,
        /// or, when <see cref="Excludes"/> is set, can't belong to that room (a hole cut out of it).
        /// </summary>
        public class Volume : AreaBase
        {
            internal const int Size = 0x230;

            /// <summary>
            /// When set, a point inside the volume is excluded from the room; otherwise the volume adds to the room.
            /// 15 of the 25 Elden Ring volumes exclude.
            /// </summary>
            public bool Excludes { get; set; }

            /// <summary>
            /// The rotation mode the game reads when it builds the volume's transform, 0 to 2 (other values assert);
            /// always 0.
            /// </summary>
            public byte RotationMode { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk28 { get; set; }

            /// <summary>
            /// Unknown; 0 or 1.
            /// </summary>
            public int Unk2C { get; set; }

            /// <summary>
            /// Creates a Volume with default values.
            /// </summary>
            public Volume() { }

            internal Volume(BinaryReaderEx br)
            {
                ReadCommon(br, out byte excludes, out byte rotationMode);
                if (excludes > 1)
                    throw new InvalidDataException($"ONAV volume exclusion flag {excludes} is not 0 or 1.");
                Excludes = excludes == 1;
                RotationMode = rotationMode;
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();
                CellMask = ReadCellMask(br);
            }

            internal void Write(BinaryWriterEx bw)
            {
                WriteCommon(bw, (byte)(Excludes ? 1 : 0), RotationMode);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);
                WriteCellMask(bw, CellMask);
            }

            /// <summary>
            /// Returns a string representation of the volume.
            /// </summary>
            public override string ToString()
            {
                return $"Volume of area {ID}{(Excludes ? " (excludes)" : "")}";
            }
        }
    }
}
