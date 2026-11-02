using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// A file that holds the break data of the breakable assets of a map, in ER and NR. Each asset is listed by its
    /// model and instance number; assets whose model is not breakable are left out. Each map has a lod0 file, and
    /// open world tiles also have lod1 and lod2 files. Every asset is in exactly one of them, chosen by its model.
    /// Extension: .breakgeom
    /// </summary>
    public class BREAKGEOM : SoulsFile<BREAKGEOM>
    {
        /// <summary>
        /// The base of <see cref="Entry.ModelID"/>; an asset model AEGxxx_yyy has ID Base + xxx * 1000 + yyy.
        /// </summary>
        public const int AssetModelIDBase = 10000000;

        /// <summary>
        /// Unknown; always 0x19071000.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// The map the file belongs to, as the four parts of mAA_BB_CC_DD. The first three are stored as plain numbers,
        /// but the last is stored as if it were hexadecimal, so m60_45_36_10 has 0x10.
        /// </summary>
        public byte[] MapID { get; set; }

        /// <summary>
        /// The break data of the assets, ordered by model ID and then instance number.
        /// </summary>
        public List<Entry> Entries { get; set; }

        /// <summary>
        /// Creates an empty BREAKGEOM.
        /// </summary>
        public BREAKGEOM()
        {
            Version = 0x19071000;
            MapID = new byte[4];
            Entries = new List<Entry>();
        }

        /// <summary>
        /// Sets <see cref="MapID"/> from a map name such as m60_45_36_10.
        /// </summary>
        public void SetMapID(string mapName)
        {
            string[] parts = mapName.TrimStart('m').Split('_');
            if (parts.Length != 4)
                throw new InvalidDataException($"\"{mapName}\" is not a map name of the form mAA_BB_CC_DD.");

            MapID = new byte[]
            {
                byte.Parse(parts[0]),
                byte.Parse(parts[1]),
                byte.Parse(parts[2]),
                byte.Parse(parts[3], System.Globalization.NumberStyles.HexNumber),
            };
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 0x10 && br.GetASCII(0, 4) == "BOEG";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("BOEG");
            Version = br.ReadUInt32();
            int count = br.ReadInt32();

            // Stored as a little-endian 0xAABBCCDD
            byte[] mapId = br.ReadBytes(4);
            MapID = new byte[] { mapId[3], mapId[2], mapId[1], mapId[0] };

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
            bw.WriteASCII("BOEG");
            bw.WriteUInt32(Version);
            bw.WriteInt32(Entries.Count);
            bw.WriteBytes(new byte[] { MapID[3], MapID[2], MapID[1], MapID[0] });

            for (int i = 0; i < Entries.Count; i++)
                Entries[i].Write(bw, i);

            for (int i = 0; i < Entries.Count; i++)
            {
                bw.FillInt64($"PiecesOffset{i}", bw.Position);
                Entries[i].Pieces.Write(bw);
            }
        }

        /// <summary>
        /// The break data of one asset.
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// The instance number of the asset part, as in AEGxxx_yyy_[InstanceID].
            /// </summary>
            public int InstanceID { get; set; }

            /// <summary>
            /// Unknown; stored in the 15 bits below the instance number, and always 1.
            /// </summary>
            public short Unk00 { get; set; }

            /// <summary>
            /// The model of the asset part; see <see cref="AssetModelIDBase"/>.
            /// </summary>
            public int ModelID { get; set; }

            /// <summary>
            /// The break data of the asset.
            /// </summary>
            public BreakPieces Pieces { get; set; }

            /// <summary>
            /// Creates an Entry with no pieces.
            /// </summary>
            public Entry()
            {
                Unk00 = 1;
                Pieces = new BreakPieces();
            }

            /// <summary>
            /// Creates a deep copy of the entry.
            /// </summary>
            public Entry DeepCopy()
            {
                return new Entry { InstanceID = InstanceID, Unk00 = Unk00, ModelID = ModelID, Pieces = Pieces.DeepCopy() };
            }

            internal Entry(BinaryReaderEx br)
            {
                uint packed = br.ReadUInt32();
                Unk00 = (short)(packed & 0x7FFF);
                InstanceID = (int)(packed >> 15);
                ModelID = br.ReadInt32();
                long piecesOffset = br.ReadInt64();

                br.StepIn(piecesOffset);
                {
                    Pieces = BreakPieces.Read(br);
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteUInt32((uint)InstanceID << 15 | (uint)(Unk00 & 0x7FFF));
                bw.WriteInt32(ModelID);
                bw.ReserveInt64($"PiecesOffset{index}");
            }
        }
    }
}
