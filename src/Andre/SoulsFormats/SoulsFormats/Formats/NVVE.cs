using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// Per-cell data for the flying navigation volume of an Armored Core VI map: one entry for each cell of the
    /// hkaiNavVolume it sits beside, in the same order, with a 0 to 1 fraction of the cell's floor area (its X by Z
    /// voxel columns) that meets a test made when the volume was generated, and that fraction bucketed into none,
    /// partial or full. The test is not known exactly. It tracks height above the ground: compared with the map's
    /// collision, columns with nothing within about two voxels (20 m) below the cell score 1 and columns near the
    /// ground score 0. Cells the file calls Full pass that rule 96–98% of the time, but 20–30% of the cells it calls
    /// None pass it too, so the real test is stricter.
    /// The file lives in the map's .nvvhktbnd, as binder entry 40000 (the volume is entry 30000). The game's
    /// CSHkAiNvvbndFileCap gives it to the NvveRepository, whose reader checks the magic and version 2 and takes the
    /// first type 1 and type 0 sections it finds.
    /// The file is a list of sections, each with a 16 byte header: type, version, size and count.
    /// Every game file round-trips byte for byte (2026-11-22: AC6 20).
    /// Extension: .nvve
    /// </summary>
    public class NVVE : SoulsFile<NVVE>
    {
        /// <summary>
        /// The format version; 2 in every file, and the only one the game loads.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The sections of the file, in file order. Every game file has an <see cref="InfoSection"/> then a
        /// <see cref="CellSection"/>; sections of other types are kept as <see cref="RawSection"/>.
        /// </summary>
        public List<Section> Sections { get; set; }

        /// <summary>
        /// The first info section, or null.
        /// </summary>
        public InfoSection Info => Sections.Find(s => s is InfoSection) as InfoSection;

        /// <summary>
        /// The entries of the first cell section, one per cell of the nav volume, or null.
        /// </summary>
        public List<CellEntry> Cells => (Sections.Find(s => s is CellSection) as CellSection)?.Entries;

        /// <summary>
        /// Creates an NVVE with an empty info section and no cells.
        /// </summary>
        public NVVE()
        {
            Version = 2;
            Sections = new List<Section> { new InfoSection(), new CellSection() };
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 0x10 && br.GetASCII(0, 4) == "NVVE";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("NVVE");
            Version = br.ReadInt32();
            br.ReadInt32(); // File size
            int sectionCount = br.ReadInt32();

            Sections = new List<Section>(sectionCount);
            for (int i = 0; i < sectionCount; i++)
            {
                int type = br.ReadInt32();
                int version = br.ReadInt32();
                int size = br.ReadInt32();
                int count = br.ReadInt32();
                long start = br.Position;

                Section section;
                if (type == 1 && version == 1 && count == 1 && size == InfoSection.EntrySize)
                    section = new InfoSection(br);
                else if (type == 0 && version == 2 && size == count * CellEntry.Size)
                    section = new CellSection(br, count);
                else
                    section = new RawSection(type, version, count, br.ReadBytes(size));

                br.Position = start + size;
                Sections.Add(section);
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteASCII("NVVE");
            bw.WriteInt32(Version);
            bw.ReserveInt32("FileSize");
            bw.WriteInt32(Sections.Count);

            foreach (var section in Sections)
            {
                bw.WriteInt32(section.Type);
                bw.WriteInt32(section.Version);
                bw.ReserveInt32("SectionSize");
                bw.WriteInt32(section.Count);
                long start = bw.Position;
                section.WriteData(bw);
                bw.FillInt32("SectionSize", (int)(bw.Position - start));
            }

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// A section of the file.
        /// </summary>
        public abstract class Section
        {
            /// <summary>
            /// The section type the game looks for: 0 cells, 1 info.
            /// </summary>
            public abstract int Type { get; }

            /// <summary>
            /// The version of the section layout.
            /// </summary>
            public abstract int Version { get; }

            /// <summary>
            /// The number of entries written in the section header.
            /// </summary>
            public abstract int Count { get; }

            internal abstract void WriteData(BinaryWriterEx bw);
        }

        /// <summary>
        /// Type 1, version 1: one 16 byte entry that identifies the volume.
        /// </summary>
        public class InfoSection : Section
        {
            internal const int EntrySize = 0x10;

            /// <inheritdoc/>
            public override int Type => 1;

            /// <inheritdoc/>
            public override int Version => 1;

            /// <inheritdoc/>
            public override int Count => 1;

            /// <summary>
            /// A 32 bit value that differs in every map, probably a hash or ID tying the file to the volume it was
            /// generated with.
            /// </summary>
            public uint VolumeHash { get; set; }

            /// <summary>
            /// Always 0.
            /// </summary>
            public int Unk04 { get; set; }

            /// <summary>
            /// Always 0.
            /// </summary>
            public int Unk08 { get; set; }

            /// <summary>
            /// Always 0.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Creates an info section with a zero hash.
            /// </summary>
            public InfoSection() { }

            internal InfoSection(BinaryReaderEx br)
            {
                VolumeHash = br.ReadUInt32();
                Unk04 = br.ReadInt32();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
            }

            internal override void WriteData(BinaryWriterEx bw)
            {
                bw.WriteUInt32(VolumeHash);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
            }
        }

        /// <summary>
        /// Type 0, version 2: one entry for each cell of the nav volume.
        /// </summary>
        public class CellSection : Section
        {
            /// <inheritdoc/>
            public override int Type => 0;

            /// <inheritdoc/>
            public override int Version => 2;

            /// <inheritdoc/>
            public override int Count => Entries.Count;

            /// <summary>
            /// The entries, in the order of the volume's cells.
            /// </summary>
            public List<CellEntry> Entries { get; set; }

            /// <summary>
            /// Creates a section with no entries.
            /// </summary>
            public CellSection()
            {
                Entries = new List<CellEntry>();
            }

            internal CellSection(BinaryReaderEx br, int count)
            {
                Entries = new List<CellEntry>(count);
                for (int i = 0; i < count; i++)
                    Entries.Add(new CellEntry(br));
            }

            internal override void WriteData(BinaryWriterEx bw)
            {
                foreach (var entry in Entries)
                    entry.Write(bw);
            }
        }

        /// <summary>
        /// A section of a type or version the class doesn't know, kept as bytes.
        /// </summary>
        public class RawSection : Section
        {
            private readonly int type;
            private readonly int version;
            private readonly int count;

            /// <inheritdoc/>
            public override int Type => type;

            /// <inheritdoc/>
            public override int Version => version;

            /// <inheritdoc/>
            public override int Count => count;

            /// <summary>
            /// The section data.
            /// </summary>
            public byte[] Data { get; set; }

            /// <summary>
            /// Creates a raw section.
            /// </summary>
            public RawSection(int type, int version, int count, byte[] data)
            {
                this.type = type;
                this.version = version;
                this.count = count;
                Data = data;
            }

            internal override void WriteData(BinaryWriterEx bw) => bw.WriteBytes(Data);
        }

        /// <summary>
        /// How much of a cell's floor area meets the test, bucketed from <see cref="CellEntry.Fraction"/>.
        /// </summary>
        public enum CellCoverage : int
        {
            /// <summary>
            /// Fraction at most 0.05; mostly exactly 0.
            /// </summary>
            None = 1,

            /// <summary>
            /// Fraction between 0.05 and 0.95.
            /// </summary>
            Partial = 2,

            /// <summary>
            /// Fraction at least 0.95; mostly exactly 1.
            /// </summary>
            Full = 3,
        }

        /// <summary>
        /// The data of one nav volume cell.
        /// </summary>
        public class CellEntry
        {
            internal const int Size = 0x10;

            /// <summary>
            /// The index of the cell in the hkaiNavVolume; equal to the entry's index in every game file.
            /// </summary>
            public int CellIndex { get; set; }

            /// <summary>
            /// The fraction bucketed into none, partial or full, at 0.05 and 0.95.
            /// </summary>
            public CellCoverage Coverage { get; set; }

            /// <summary>
            /// The share of the cell's voxel columns (its X by Z footprint) that meet the test, from 0 to 1.
            /// Always a whole number of columns over the cell's column count. High for cells well above the ground,
            /// low for cells near it (see the class summary).
            /// </summary>
            public float Fraction { get; set; }

            /// <summary>
            /// Always 0.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Creates an entry for cell 0 with no coverage.
            /// </summary>
            public CellEntry()
            {
                Coverage = CellCoverage.None;
            }

            /// <summary>
            /// Creates an entry whose coverage follows from the fraction, as in the game files.
            /// </summary>
            public CellEntry(int cellIndex, float fraction)
            {
                CellIndex = cellIndex;
                Fraction = fraction;
                Coverage = CoverageOf(fraction);
            }

            /// <summary>
            /// The bucket the game files put a fraction in.
            /// </summary>
            public static CellCoverage CoverageOf(float fraction)
            {
                if (fraction <= 0.05f) return CellCoverage.None;
                if (fraction >= 0.95f) return CellCoverage.Full;
                return CellCoverage.Partial;
            }

            internal CellEntry(BinaryReaderEx br)
            {
                CellIndex = br.ReadInt32();
                Coverage = (CellCoverage)br.ReadInt32();
                Fraction = br.ReadSingle();
                Unk0C = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(CellIndex);
                bw.WriteInt32((int)Coverage);
                bw.WriteSingle(Fraction);
                bw.WriteInt32(Unk0C);
            }

            /// <summary>
            /// Returns the entry as a string.
            /// </summary>
            public override string ToString() => $"{CellIndex}: {Coverage} {Fraction}";
        }
    }
}
