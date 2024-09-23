using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A lightmap atlasing config file introduced in DS2. Extension: .btab
    /// </summary>
    public class BTAB : SoulsFile<BTAB>
    {
        /// <summary>
        /// Whether the file is big-endian; true for PS3/X360, false otherwise.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Whether the file uses the 64-bit format; true for DS3/BB, false for DS2.
        /// </summary>
        public bool LongFormat { get; set; }

        /// <summary>
        /// Material configs in this file.
        /// </summary>
        public List<Entry> Entries { get; set; }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = br.GetBoolean(0x10);

            br.AssertInt32(1);
            br.AssertInt32(0);
            int entryCount = br.ReadInt32();
            int stringsLength = br.ReadInt32();
            BigEndian = br.ReadBoolean();
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);
            br.VarintLong = LongFormat = br.AssertInt32([0x1C, 0x28]) == 0x28; // Entry size
            br.AssertPattern(0x24, 0x00);

            long stringsStart = br.Position;
            br.Skip(stringsLength);
            Entries = new List<Entry>(entryCount);
            for (int i = 0; i < entryCount; i++)
                Entries.Add(new Entry(br, stringsStart));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            bw.VarintLong = LongFormat;

            bw.WriteInt32(1);
            bw.WriteInt32(0);
            bw.WriteInt32(Entries.Count);
            bw.ReserveInt32("StringsLength");
            bw.WriteBoolean(BigEndian);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteInt32(LongFormat ? 0x28 : 0x1C);
            bw.WritePattern(0x24, 0x00);

            long stringsStart = bw.Position;
            var stringOffsets = new List<long>(Entries.Count * 2);
            foreach (Entry entry in Entries)
            {
                long partNameOffset = bw.Position - stringsStart;
                stringOffsets.Add(partNameOffset);
                bw.WriteUTF16(entry.PartName, true);
                bw.PadRelative(stringsStart, 8); // This padding is not consistent, but it's the best I can do

                long materialNameOffset = bw.Position - stringsStart;
                stringOffsets.Add(materialNameOffset);
                bw.WriteUTF16(entry.MaterialName, true);
                bw.PadRelative(stringsStart, 8);
            }

            bw.FillInt32("StringsLength", (int)(bw.Position - stringsStart));
            for (int i = 0; i < Entries.Count; i++)
                Entries[i].Write(bw, stringOffsets[i * 2], stringOffsets[i * 2 + 1]);
        }

        /// <summary>
        /// Configures lightmap atlasing for a certain part and material.
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// The name of the target part defined in an MSB file.
            /// </summary>
            public string PartName { get; set; }

            /// <summary>
            /// The name of the target material in the part's FLVER model.
            /// </summary>
            public string MaterialName { get; set; }

            /// <summary>
            /// The ID of the atlas texture to use.
            /// </summary>
            public int AtlasID { get; set; }

            /// <summary>
            /// Offsets the lightmap UVs.
            /// </summary>
            public Vector2 UVOffset { get; set; }

            /// <summary>
            /// Scales the lightmap UVs.
            /// </summary>
            public Vector2 UVScale { get; set; }

            /// <summary>
            /// Creates an Entry with default values.
            /// </summary>
            public Entry()
            {
                PartName = "";
                MaterialName = "";
                UVScale = Vector2.One;
            }

            public Entry GetClone()
            {
                return (Entry)MemberwiseClone();
            }

            internal Entry(BinaryReaderEx br, long nameStart)
            {
                long msbNameOffset = br.ReadVarint();
                long flverNameOffset = br.ReadVarint();
                AtlasID = br.ReadInt32();
                UVOffset = br.ReadVector2();
                UVScale = br.ReadVector2();
                if (br.VarintLong)
                    br.AssertInt32(0);

                PartName = br.GetUTF16(nameStart + msbNameOffset);
                MaterialName = br.GetUTF16(nameStart + flverNameOffset);
            }

            internal void Write(BinaryWriterEx bw, long partNameOffset, long materialNameOffset)
            {
                bw.WriteVarint(partNameOffset);
                bw.WriteVarint(materialNameOffset);
                bw.WriteInt32(AtlasID);
                bw.WriteVector2(UVOffset);
                bw.WriteVector2(UVScale);
                if (bw.VarintLong)
                    bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns the MSB part name and FLVER material name of the entry.
            /// </summary>
            public override string ToString()
            {
                return $"{PartName} : {MaterialName}";
            }
        }
    }
}
