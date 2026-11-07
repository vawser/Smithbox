using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// The occlusion graph of an Elden Ring map, used alongside its navmeshes: areas over a 4096 cell grid of the
    /// map's bounds, the layers within each area, and the links between areas. The game reads it with
    /// MSPOcclusionGraph (CS::OnavResCap); most maps have an empty one.
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
        /// The major format version; always 3.
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// The minor format version; 2 in most files. The game only loads 2; 26 Elden Ring files have 1, laid out the same.
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
        /// The number of grid cells along X, Y and Z; their product is always 4096, the bits of a cell mask.
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
        /// The areas of the graph. The game skips a file with none.
        /// </summary>
        public List<Area> Areas { get; set; }

        /// <summary>
        /// Areas without layers or links, stored inline after the areas; present in about 20 Elden Ring maps.
        /// The game builds a transform for each from its position and rotation.
        /// </summary>
        public List<UnkArea> UnkAreas { get; set; }

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
            UnkAreas = new List<UnkArea>();
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
            Version = br.AssertByte(3);
            SubVersion = br.ReadByte();
            br.AssertInt16(0);
            br.AssertInt32(0);
            br.ReadInt32(); // Relocation count
            long unkAreasOffset = br.ReadInt64();
            long unk18Offset = br.ReadInt64();
            long areaPointersOffset = br.ReadInt64();
            br.ReadInt64(); // Relocation table offset

            br.AssertUInt16(0);
            br.AssertUInt16(0); // The count of the always empty array at 0x18, presumably
            int areaCount = br.ReadUInt16();
            int unkAreaCount = br.ReadUInt16();
            br.AssertPattern(0x18, 0);

            BoundsMax = br.ReadVector3();
            BoundsMin = br.ReadVector3();
            GridCountX = br.ReadInt32();
            GridCountY = br.ReadInt32();
            GridCountZ = br.ReadInt32();

            if (unk18Offset != unkAreasOffset + unkAreaCount * UnkArea.Size)
                throw new InvalidDataException("The ONAV array at 0x18 is not empty; its layout is unknown.");

            long[] areaOffsets = br.GetInt64s(areaPointersOffset, areaCount);
            Areas = new List<Area>(areaCount);
            foreach (long offset in areaOffsets)
            {
                br.Position = offset;
                Areas.Add(new Area(br));
            }

            br.Position = unkAreasOffset;
            UnkAreas = new List<UnkArea>(unkAreaCount);
            for (int i = 0; i < unkAreaCount; i++)
                UnkAreas.Add(new UnkArea(br));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            if (Areas.Count > ushort.MaxValue || UnkAreas.Count > ushort.MaxValue)
                throw new InvalidDataException("ONAV counts are 16 bit.");

            // Offsets the game relocates, in the order the game files list them
            var relocations = new List<long> { 0x10, 0x18, 0x20 };

            bw.BigEndian = false;
            bw.WriteASCII("ONAV");
            bw.WriteByte(Version);
            bw.WriteByte(SubVersion);
            bw.WriteInt16(0);
            bw.WriteInt32(0);
            bw.ReserveInt32("RelocationCount");
            bw.ReserveInt64("UnkAreasOffset");
            bw.ReserveInt64("Unk18Offset");
            bw.ReserveInt64("AreaPointersOffset");
            bw.ReserveInt64("RelocationTableOffset");

            bw.WriteUInt16(0);
            bw.WriteUInt16(0);
            bw.WriteUInt16((ushort)Areas.Count);
            bw.WriteUInt16((ushort)UnkAreas.Count);
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
                area.Write(bw, relocations);
            }

            bw.FillInt64("UnkAreasOffset", bw.Position);
            foreach (UnkArea unkArea in UnkAreas)
                unkArea.Write(bw);

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
        /// The fields an area and an unknown area share.
        /// </summary>
        public abstract class AreaBase
        {
            /// <summary>
            /// The ID that links refer to. Areas and unknown areas share the numbering, so it is not an index.
            /// </summary>
            public ushort ID { get; set; }

            /// <summary>
            /// Unknown. For unknown areas the game reads it as the rotation mode, accepting 0 to 2; areas also use 4, 6 and 8.
            /// </summary>
            public byte Unk03 { get; set; }

            /// <summary>
            /// The position of the area.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Euler rotation in radians; the game takes the sine and cosine of each axis.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// The scale of the area's 16 bit local coordinates: the game multiplies it by 65535 * 0.5.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// One bit for each of the 4096 grid cells of the file; which cells the area covers.
            /// </summary>
            public byte[] CellMask { get; set; }

            internal AreaBase()
            {
                CellMask = new byte[CellMaskSize];
            }

            internal void ReadCommon(BinaryReaderEx br, out byte count02)
            {
                ID = br.ReadUInt16();
                count02 = br.ReadByte();
                Unk03 = br.ReadByte();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
            }

            internal void WriteCommon(BinaryWriterEx bw, byte count02)
            {
                bw.WriteUInt16(ID);
                bw.WriteByte(count02);
                bw.WriteByte(Unk03);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);
            }
        }

        /// <summary>
        /// An area of the graph, with its layers and its links to other areas.
        /// </summary>
        public class Area : AreaBase
        {
            /// <summary>
            /// Unknown; 0 to 3.
            /// </summary>
            public ushort Unk2A { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public byte Unk2C { get; set; }

            /// <summary>
            /// Unknown; 0 or 1, set with Unk30 and Unk38. The game checks it for 1.
            /// </summary>
            public byte Unk2D { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public short Unk2E { get; set; }

            /// <summary>
            /// Unknown; 0 or 1.
            /// </summary>
            public int Unk30 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk34 { get; set; }

            /// <summary>
            /// Unknown; 0 or 1.
            /// </summary>
            public int Unk38 { get; set; }

            /// <summary>
            /// Unknown; always 1.
            /// </summary>
            public ushort Unk3C { get; set; }

            /// <summary>
            /// Unknown; counts up from 0 within a file.
            /// </summary>
            public ushort Unk3E { get; set; }

            /// <summary>
            /// The layers of the area; at most 255.
            /// </summary>
            public List<Layer> Layers { get; set; }

            /// <summary>
            /// The links to other areas.
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

            internal Area(BinaryReaderEx br)
            {
                ReadCommon(br, out byte layerCount);
                int linkCount = br.ReadUInt16();
                Unk2A = br.ReadUInt16();
                Unk2C = br.ReadByte();
                Unk2D = br.ReadByte();
                Unk2E = br.ReadInt16();
                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                Unk38 = br.ReadInt32();
                Unk3C = br.ReadUInt16();
                Unk3E = br.ReadUInt16();
                CellMask = ReadCellMask(br);
                long linksOffset = br.ReadInt64();

                Layers = new List<Layer>(layerCount);
                for (int i = 0; i < layerCount; i++)
                    Layers.Add(new Layer(br));

                br.Position = linksOffset;
                Links = new List<Link>(linkCount);
                for (int i = 0; i < linkCount; i++)
                    Links.Add(new Link(br));
            }

            internal void Write(BinaryWriterEx bw, List<long> relocations)
            {
                if (Layers.Count > byte.MaxValue)
                    throw new InvalidDataException("An ONAV area can have at most 255 layers.");
                if (Links.Count > ushort.MaxValue)
                    throw new InvalidDataException("An ONAV area can have at most 65535 links.");

                long start = bw.Position;
                WriteCommon(bw, (byte)Layers.Count);
                bw.WriteUInt16((ushort)Links.Count);
                bw.WriteUInt16(Unk2A);
                bw.WriteByte(Unk2C);
                bw.WriteByte(Unk2D);
                bw.WriteInt16(Unk2E);
                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(Unk38);
                bw.WriteUInt16(Unk3C);
                bw.WriteUInt16(Unk3E);
                WriteCellMask(bw, CellMask);

                long linksPointer = bw.Position;
                relocations.Add(linksPointer);
                bw.WriteInt64(0);

                var layerPointers = new long[Layers.Count];
                for (int i = 0; i < Layers.Count; i++)
                {
                    layerPointers[i] = Layers[i].Write(bw);
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
        /// A layer of an area; links name the layer of each end.
        /// </summary>
        public class Layer
        {
            internal const int Size = 0x38;

            /// <summary>
            /// The index of the layer, as <see cref="Link.Layers"/> and <see cref="Entry.Layers"/> refer to it.
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
            /// Unknown.
            /// </summary>
            public Vector3 Unk04 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Unknown; small multiples of 5, or 0.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Unknown; 0 to 15.
            /// </summary>
            public int Unk18 { get; set; }

            /// <summary>
            /// Unknown; around 7500, counting up across the files.
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
            /// Unknown; 0 or a multiple of 10 up to 200.
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
            /// The layer's entries; at most 255.
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

            internal Layer(BinaryReaderEx br)
            {
                Index = br.ReadByte();
                Unk01 = br.ReadByte();
                Unk02 = br.ReadInt16();
                Unk04 = br.ReadVector3();
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
                long entriesOffset = br.ReadInt64();

                Entries = new List<Entry>(entryCount);
                br.StepIn(entriesOffset);
                for (int i = 0; i < entryCount; i++)
                    Entries.Add(new Entry(br));
                br.StepOut();
            }

            /// <summary>
            /// Writes the layer, returning the offset of its entries pointer for the caller to fill.
            /// </summary>
            internal long Write(BinaryWriterEx bw)
            {
                if (Entries.Count > byte.MaxValue)
                    throw new InvalidDataException("An ONAV layer can have at most 255 entries.");

                bw.WriteByte(Index);
                bw.WriteByte(Unk01);
                bw.WriteInt16(Unk02);
                bw.WriteVector3(Unk04);
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
                long pointer = bw.Position;
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
            /// Unknown; pairs two layer indices like <see cref="Link.Layers"/>.
            /// </summary>
            public class Entry
            {
                /// <summary>
                /// Two layer indices: the low nibble and the high nibble. The game reads the high one.
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
        /// A link from an area to another.
        /// </summary>
        public class Link
        {
            /// <summary>
            /// The <see cref="AreaBase.ID"/> of the area linked to.
            /// </summary>
            public ushort AreaID { get; set; }

            /// <summary>
            /// The low nibble is the <see cref="Layer.Index"/> in this area the game matches it with;
            /// the high nibble is passed on, presumably the layer in the area linked to.
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
            /// The layer in this area: the low nibble of <see cref="Layers"/>.
            /// </summary>
            public int LayerIndex
            {
                get => Layers & 0xF;
                set => Layers = (byte)((Layers & 0xF0) | (value & 0xF));
            }

            /// <summary>
            /// The high nibble of <see cref="Layers"/>.
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
        /// Unknown; an area without layers or links.
        /// </summary>
        public class UnkArea : AreaBase
        {
            internal const int Size = 0x230;

            /// <summary>
            /// Unknown; 0 or 1.
            /// </summary>
            public byte Unk02 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk28 { get; set; }

            /// <summary>
            /// Unknown; 0 or 1.
            /// </summary>
            public int Unk2C { get; set; }

            /// <summary>
            /// Creates an UnkArea with default values.
            /// </summary>
            public UnkArea() { }

            internal UnkArea(BinaryReaderEx br)
            {
                ReadCommon(br, out byte unk02);
                Unk02 = unk02;
                Unk28 = br.ReadInt32();
                Unk2C = br.ReadInt32();
                CellMask = ReadCellMask(br);
            }

            internal void Write(BinaryWriterEx bw)
            {
                WriteCommon(bw, Unk02);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(Unk2C);
                WriteCellMask(bw, CellMask);
            }

            /// <summary>
            /// Returns a string representation of the area.
            /// </summary>
            public override string ToString()
            {
                return $"UnkArea {ID}";
            }
        }
    }
}
