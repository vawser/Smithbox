using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A collection of spherical harmonics light probes for lighting characters and objects in a map. Extension: .btpb
    /// </summary>
    public class BTPB : SoulsFile<BTPB>
    {
        /// <summary>
        /// Indicates the format of the file and supported features.
        /// </summary>
        public BTPBVersion Version { get; set; }

        /// <summary>
        /// Unknown; probably bounding box min.
        /// </summary>
        public Vector3 Unk1C { get; set; }

        /// <summary>
        /// Unknown; probably bounding box max.
        /// </summary>
        public Vector3 Unk28 { get; set; }

        /// <summary>
        /// Groups of light probes in the map.
        /// </summary>
        public List<Group> Groups { get; set; }

        /// <summary>
        /// Creates an empty BTPB formatted for Dark Souls 3.
        /// </summary>
        public BTPB()
        {
            Version = BTPBVersion.DarkSouls3;
            Groups = new List<Group>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            bool bigEndian = br.BigEndian = br.GetBoolean(0x10);

            int unk00 = br.AssertInt32([2, 3]);
            int unk04 = br.AssertInt32([0, 1]);
            int groupCount = br.ReadInt32();
            int dataLength = br.ReadInt32();
            br.AssertBoolean(bigEndian);
            br.AssertPattern(3, 0x00);
            int groupSize = br.AssertInt32([0x40, 0x48, 0x98]);
            int probeSize = br.AssertInt32([0x1C, 0x48]);
            Unk1C = br.ReadVector3();
            Unk28 = br.ReadVector3();
            br.AssertInt64(0);

            if (!bigEndian && unk00 == 2 && unk04 == 1 && groupSize == 0x40 && probeSize == 0x1C)
                Version = BTPBVersion.DarkSouls2LE;
            else if (bigEndian && unk00 == 2 && unk04 == 1 && groupSize == 0x40 && probeSize == 0x1C)
                Version = BTPBVersion.DarkSouls2BE;
            else if (!bigEndian && unk00 == 2 && unk04 == 1 && groupSize == 0x48 && probeSize == 0x48)
                Version = BTPBVersion.Bloodborne;
            else if (!bigEndian && unk00 == 3 && unk04 == 0 && groupSize == 0x98 && probeSize == 0x48)
                Version = BTPBVersion.DarkSouls3;
            else
                throw new InvalidDataException($"Unknown BTPB format. {nameof(bigEndian)}:{bigEndian} {nameof(unk00)}:0x{unk00:X}" +
                    $" {nameof(unk04)}:0x{unk04:X} {nameof(groupSize)}:0x{groupSize:X} {nameof(probeSize)}:0x{probeSize:X}");

            br.VarintLong = Version >= BTPBVersion.Bloodborne;

            long dataStart = br.Position;
            br.Skip(dataLength);
            Groups = new List<Group>(groupCount);
            for (int i = 0; i < groupCount; i++)
                Groups.Add(new Group(br, Version, dataStart));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bool bigEndian;
            int unk00, unk04, groupSize, probeSize;
            if (Version == BTPBVersion.DarkSouls2LE || Version == BTPBVersion.DarkSouls2BE)
            {
                bigEndian = Version == BTPBVersion.DarkSouls2BE;
                unk00 = 2;
                unk04 = 1;
                groupSize = 0x40;
                probeSize = 0x1C;
            }
            else if (Version == BTPBVersion.Bloodborne)
            {
                bigEndian = false;
                unk00 = 2;
                unk04 = 1;
                groupSize = 0x48;
                probeSize = 0x48;
            }
            else if (Version == BTPBVersion.DarkSouls3)
            {
                bigEndian = false;
                unk00 = 3;
                unk04 = 0;
                groupSize = 0x98;
                probeSize = 0x48;
            }
            else
                throw new NotImplementedException($"Write is apparently not supported for BTPB version {Version}.");

            bw.BigEndian = bigEndian;
            bw.VarintLong = Version >= BTPBVersion.Bloodborne;

            bw.WriteInt32(unk00);
            bw.WriteInt32(unk04);
            bw.WriteInt32(Groups.Count);
            bw.ReserveInt32("DataLength");
            bw.WriteBoolean(bigEndian);
            bw.WritePattern(3, 0x00);
            bw.WriteInt32(groupSize);
            bw.WriteInt32(probeSize);
            bw.WriteVector3(Unk1C);
            bw.WriteVector3(Unk28);
            bw.WriteInt64(0);

            long[] nameOffsets = new long[Groups.Count];
            long[] probesOffsets = new long[Groups.Count];

            long dataStart = bw.Position;
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteData(bw, Version, dataStart, out nameOffsets[i], out probesOffsets[i]);
            bw.FillInt32("DataLength", (int)(bw.Position - dataStart));

            for (int i = 0; i < Groups.Count; i++)
                Groups[i].Write(bw, Version, nameOffsets[i], probesOffsets[i]);
        }

        /// <summary>
        /// Supported BTPB formats.
        /// </summary>
        public enum BTPBVersion
        {
            /// <summary>
            /// Dark Souls 2 on PC and SotFS on all platforms.
            /// </summary>
            DarkSouls2LE,

            /// <summary>
            /// Dark Souls 2 on PS3 and X360.
            /// </summary>
            DarkSouls2BE,

            /// <summary>
            /// Bloodborne.
            /// </summary>
            Bloodborne,

            /// <summary>
            /// Dark Souls 3 on all platforms.
            /// </summary>
            DarkSouls3,
        }

        /// <summary>
        /// A volume containing light probes with some additional configuration.
        /// </summary>
        public class Group
        {
            /// <summary>
            /// An optional name for the group. Presence appears to be indicated by the lowest bit of Flags08.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Appears to be flags, highly speculative.
            /// </summary>
            public int Flags08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk18 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk1C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk20 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk24 { get; set; }

            /// <summary>
            /// Unknown; probably bounding box min.
            /// </summary>
            public Vector3 Unk28 { get; set; }

            /// <summary>
            /// Unknown; probably bounding box max.
            /// </summary>
            public Vector3 Unk34 { get; set; }

            /// <summary>
            /// Light probes in this group.
            /// </summary>
            public List<Probe> Probes { get; set; }

            /// <summary>
            /// Unknown; only present since DS3.
            /// </summary>
            public float Unk48 { get; set; }

            /// <summary>
            /// Unknown; only present since DS3.
            /// </summary>
            public float Unk4C { get; set; }

            /// <summary>
            /// Unknown; only present since DS3.
            /// </summary>
            public float Unk50 { get; set; }

            /// <summary>
            /// Unknown; only present since DS3.
            /// </summary>
            public byte Unk94 { get; set; }

            /// <summary>
            /// Unknown; only present since DS3.
            /// </summary>
            public byte Unk95 { get; set; }

            /// <summary>
            /// Unknown; only present since DS3.
            /// </summary>
            public byte Unk96 { get; set; }

            /// <summary>
            /// Creates an empty Group with default values.
            /// </summary>
            public Group()
            {
                Probes = new List<Probe>();
            }

            internal Group(BinaryReaderEx br, BTPBVersion version, long dataStart)
            {
                long nameOffset = br.ReadVarint();
                Flags08 = br.ReadInt32();
                int probeCount = br.ReadInt32();
                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadSingle();
                Unk20 = br.ReadSingle();
                Unk24 = br.ReadSingle();
                Unk28 = br.ReadVector3();
                Unk34 = br.ReadVector3();
                long probesOffset = br.ReadVarint();

                if (version >= BTPBVersion.DarkSouls3)
                {
                    Unk48 = br.ReadSingle();
                    Unk4C = br.ReadSingle();
                    Unk50 = br.ReadSingle();
                    br.AssertPattern(0x40, 0x00);
                    Unk94 = br.ReadByte();
                    Unk95 = br.ReadByte();
                    Unk96 = br.ReadByte();
                    br.AssertByte(0);
                }

                if ((Flags08 & 1) != 0)
                    Name = br.GetUTF16(dataStart + nameOffset);

                br.StepIn(dataStart + probesOffset);
                {
                    Probes = new List<Probe>(probeCount);
                    for (int i = 0; i < probeCount; i++)
                        Probes.Add(new Probe(br, version));
                }
                br.StepOut();
            }

            internal void WriteData(BinaryWriterEx bw, BTPBVersion version, long dataStart, out long nameOffset, out long probesOffset)
            {
                if ((Flags08 & 1) != 0)
                {
                    nameOffset = bw.Position - dataStart;
                    bw.WriteUTF16(Name, true);
                    if ((bw.Position - dataStart) % 8 != 0)
                        bw.Position += 8 - (bw.Position - dataStart) % 8;
                }
                else
                {
                    nameOffset = 0;
                }

                probesOffset = bw.Position - dataStart;
                foreach (Probe probe in Probes)
                    probe.Write(bw, version);
            }

            internal void Write(BinaryWriterEx bw, BTPBVersion version, long nameOffset, long probesOffset)
            {
                bw.WriteVarint(nameOffset);
                bw.WriteInt32(Flags08);
                bw.WriteInt32(Probes.Count);
                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteSingle(Unk1C);
                bw.WriteSingle(Unk20);
                bw.WriteSingle(Unk24);
                bw.WriteVector3(Unk28);
                bw.WriteVector3(Unk34);
                bw.WriteVarint(probesOffset);

                if (version >= BTPBVersion.DarkSouls3)
                {
                    bw.WriteSingle(Unk48);
                    bw.WriteSingle(Unk4C);
                    bw.WriteSingle(Unk50);
                    bw.WritePattern(0x40, 0x00);
                    bw.WriteByte(Unk94);
                    bw.WriteByte(Unk95);
                    bw.WriteByte(Unk96);
                    bw.WriteByte(0);
                }
            }
        }

        /// <summary>
        /// A probe giving directional lighting information at a given point.
        /// </summary>
        public class Probe
        {
            /// <summary>
            /// First-order spherical harmonics coefficients in R0G0B0R1G1B1... order.
            /// </summary>
            public short[] Coefficients { get; private set; }

            /// <summary>
            /// Multiplies sun lighting, where 0 is 0% sun and 1024 is 100%.
            /// </summary>
            public short LightMask { get; set; }

            /// <summary>
            /// Unknown; always 0 outside the chalice BTPB.
            /// </summary>
            public short Unk1A { get; set; }

            /// <summary>
            /// The position of the probe; not present in DS2.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Creates a Probe with default values.
            /// </summary>
            public Probe()
            {
                Coefficients = new short[12];
            }

            internal Probe(BinaryReaderEx br, BTPBVersion version)
            {
                Coefficients = br.ReadInt16s(12);
                LightMask = br.ReadInt16();
                Unk1A = br.ReadInt16();

                if (version >= BTPBVersion.Bloodborne)
                {
                    Position = br.ReadVector3();
                    br.AssertPattern(0x20, 0x00);
                }
            }

            internal void Write(BinaryWriterEx bw, BTPBVersion version)
            {
                bw.WriteInt16s(Coefficients);
                bw.WriteInt16(LightMask);
                bw.WriteInt16(Unk1A);

                if (version >= BTPBVersion.Bloodborne)
                {
                    bw.WriteVector3(Position);
                    bw.WritePattern(0x20, 0x00);
                }
            }
        }
    }
}
