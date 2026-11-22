using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// An Armored Core 6 "AI thread info" file: for each AI character of a mission, the worker slot its AI is assigned to,
    /// under three slot counts (5, 6 and 12). The game loads map:/aithreadInfo/{missionId:D4}.aith when a mission starts
    /// (armoredcore6.exe 1412632D2). It can also write these files itself (Game.AI.IsEnableAiThreadInfoOutput,
    /// Game.AI.AiThreadInfoOutputDirPath), so the shipped files are probably recorded or generated, not written by hand.
    /// There is no header: the file is just a list of 16-byte rows.
    /// Extension: .aith
    /// </summary>
    public class AITH : SoulsFile<AITH>
    {
        /// <summary>
        /// One row per AI character, in file order. The vanilla files list the mission map's characters by ascending
        /// index, then the characters of the mission's placement map (MissionParam.placementMapUid).
        /// </summary>
        public List<Entry> Entries { get; set; }

        /// <summary>
        /// Creates an empty AITH.
        /// </summary>
        public AITH()
        {
            Entries = new List<Entry>();
        }

        /// <summary>
        /// Checks whether the data looks like this format. With no magic this is only a check of shape (whole rows,
        /// 100 in the last byte of each), so use it together with the .aith extension.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length == 0 || br.Length % Entry.Size != 0)
                return false;

            for (long i = 0; i < br.Length; i += Entry.Size)
            {
                if (br.GetByte(i + 0xF) != 100)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            long count = br.Length / Entry.Size;
            Entries = new List<Entry>((int)count);

            for (long i = 0; i < count; i++)
                Entries.Add(new Entry(br));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;

            foreach (Entry entry in Entries)
                entry.Write(bw);
        }

        /// <summary>
        /// The worker slots of one AI character.
        /// </summary>
        public class Entry
        {
            internal const int Size = 0x10;

            /// <summary>
            /// The character: Index in bits 0-19, BlockIndex in bits 20-27, Type in bits 28-31.
            /// Kept whole, so values that don't fit that split still write back unchanged.
            /// </summary>
            public uint Selector { get; set; }

            /// <summary>
            /// The character's index in its map. Below the map's MSB Enemy count it is that Enemy part
            /// (2981 of 2998 vanilla rows); above it, it is probably a character spawned at runtime.
            /// </summary>
            public int Index
            {
                get => (int)(Selector & 0xFFFFF);
                set => Selector = (Selector & 0xFFF00000) | ((uint)value & 0xFFFFF);
            }

            /// <summary>
            /// The map's slot in the game's map list: the same for every row of a map, 0x108-0x171 in vanilla.
            /// </summary>
            public byte BlockIndex
            {
                get => (byte)((Selector >> 20) & 0xFF);
                set => Selector = (Selector & 0xF00FFFFF) | ((uint)value << 20);
            }

            /// <summary>
            /// The kind of selector. Always 1 in vanilla; probably "map character".
            /// </summary>
            public byte Type
            {
                get => (byte)(Selector >> 28);
                set => Selector = (Selector & 0x0FFFFFFF) | ((uint)(value & 0xF) << 28);
            }

            /// <summary>
            /// Map ID of the character, the last part of mAA_BB_CC_DD (written first).
            /// </summary>
            public byte MapDD { get; set; }

            /// <summary>
            /// Map ID of the character, the third part of mAA_BB_CC_DD.
            /// </summary>
            public byte MapCC { get; set; }

            /// <summary>
            /// Map ID of the character, the second part of mAA_BB_CC_DD.
            /// </summary>
            public byte MapBB { get; set; }

            /// <summary>
            /// Map ID of the character, the first part of mAA_BB_CC_DD (written last).
            /// </summary>
            public byte MapAA { get; set; }

            /// <summary>
            /// The character's slot out of 5 (0-4).
            /// </summary>
            public byte Slot5 { get; set; }

            /// <summary>
            /// The character's slot out of 6 (0-5).
            /// </summary>
            public byte Slot6 { get; set; }

            /// <summary>
            /// The character's slot out of 12 (0-11).
            /// </summary>
            public byte Slot12 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public byte Unk0B { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public byte Unk0C { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public byte Unk0D { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public byte Unk0E { get; set; }

            /// <summary>
            /// Unknown; always 100. Possibly a weight or percentage.
            /// </summary>
            public byte Unk0F { get; set; }

            /// <summary>
            /// The character's map as mAA_BB_CC_DD.
            /// </summary>
            public string MapName => $"m{MapAA:D2}_{MapBB:D2}_{MapCC:D2}_{MapDD:D2}";

            /// <summary>
            /// Creates an Entry with the vanilla constant values.
            /// </summary>
            public Entry()
            {
                Type = 1;
                Unk0F = 100;
            }

            internal Entry(BinaryReaderEx br)
            {
                Selector = br.ReadUInt32();
                MapDD = br.ReadByte();
                MapCC = br.ReadByte();
                MapBB = br.ReadByte();
                MapAA = br.ReadByte();
                Slot5 = br.ReadByte();
                Slot6 = br.ReadByte();
                Slot12 = br.ReadByte();
                Unk0B = br.ReadByte();
                Unk0C = br.ReadByte();
                Unk0D = br.ReadByte();
                Unk0E = br.ReadByte();
                Unk0F = br.ReadByte();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt32(Selector);
                bw.WriteByte(MapDD);
                bw.WriteByte(MapCC);
                bw.WriteByte(MapBB);
                bw.WriteByte(MapAA);
                bw.WriteByte(Slot5);
                bw.WriteByte(Slot6);
                bw.WriteByte(Slot12);
                bw.WriteByte(Unk0B);
                bw.WriteByte(Unk0C);
                bw.WriteByte(Unk0D);
                bw.WriteByte(Unk0E);
                bw.WriteByte(Unk0F);
            }

            /// <summary>
            /// Returns the map, index and slots of the entry.
            /// </summary>
            public override string ToString()
            {
                return $"{MapName} #{Index} slots {Slot5}/{Slot6}/{Slot12}";
            }
        }
    }
}
