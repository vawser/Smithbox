using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// A file that holds the break data of the breakable objects of a map, in DeS, DS1, BB and DS3.
    /// It has one slot for each Object part of the MSB with the same name, in MSB order, and a slot is
    /// filled when the object's model is breakable. DS2 uses a different layout; see <see cref="BREAKOBJ2"/>.
    /// Extension: .breakobj
    /// </summary>
    public class BREAKOBJ : SoulsFile<BREAKOBJ>
    {
        /// <summary>
        /// The version of DeS, DS1 and DS3 files.
        /// </summary>
        public const uint VersionDES = 0x08051701;

        /// <summary>
        /// The version of BB files.
        /// </summary>
        public const uint VersionBB = 0x14082501;

        /// <summary>
        /// Whether the object count and the break data are big-endian (DeS).
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Whether the magic and version are big-endian (DeS and BB). BB writes the rest of the file little-endian.
        /// </summary>
        public bool BigEndianHeader { get; set; }

        /// <summary>
        /// Looks like a date followed by a revision; see <see cref="VersionDES"/> and <see cref="VersionBB"/>.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// One slot per Object part of the MSB, in MSB order: the object's break data, or null if it has none.
        /// </summary>
        public List<BreakPieces> Objects { get; set; }

        /// <summary>
        /// Creates an empty BREAKOBJ with the DS1/DS3 layout.
        /// </summary>
        public BREAKOBJ()
        {
            Version = VersionDES;
            Objects = new List<BreakPieces>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 0xC)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "BJBO" || magic == "OBJB" && br.GetInt32(4) != 1;
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            BigEndianHeader = br.GetASCII(0, 4) == "OBJB";
            br.BigEndian = BigEndianHeader;
            br.AssertASCII(BigEndianHeader ? "OBJB" : "BJBO");
            Version = br.ReadUInt32();

            // The BB chalice dungeon map (m29_00_00_00) uses a third layout with no object mask
            if (Version != VersionDES && Version != VersionBB)
                throw new NotSupportedException($"BREAKOBJ version 0x{Version:X8} is not supported.");

            // BB has a big-endian header over a little-endian body, so a big-endian count must also fit the file
            br.BigEndian = BigEndianHeader && FitsFile(br, br.GetInt32(8));
            BigEndian = br.BigEndian;

            int count = br.ReadInt32();
            byte[] mask = br.ReadBytes((count + 7) / 8);

            Objects = new List<BreakPieces>(count);
            for (int i = 0; i < count; i++)
            {
                bool breakable = (mask[i / 8] >> (i % 8) & 1) != 0;
                Objects.Add(breakable ? BreakPieces.Read(br) : null);
            }

            if (br.Position != br.Length)
                throw new InvalidDataException($"BREAKOBJ has 0x{br.Length - br.Position:X} bytes after its last object.");
        }

        private static bool FitsFile(BinaryReaderEx br, int count)
        {
            return count >= 0 && 0xC + (count + 7L) / 8 <= br.Length;
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndianHeader;
            bw.WriteASCII(BigEndianHeader ? "OBJB" : "BJBO");
            bw.WriteUInt32(Version);

            bw.BigEndian = BigEndian;
            bw.WriteInt32(Objects.Count);

            // The mask is a bit per object from the low bit of each byte, whatever the endianness
            byte[] mask = new byte[(Objects.Count + 7) / 8];
            for (int i = 0; i < Objects.Count; i++)
            {
                if (Objects[i] != null)
                    mask[i / 8] |= (byte)(1 << (i % 8));
            }
            bw.WriteBytes(mask);

            foreach (BreakPieces pieces in Objects)
                pieces?.Write(bw);
        }
    }
}
