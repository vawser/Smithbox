using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A cutscene definition format used since DS2, short for MovieSequencer Binary. Extension: .mqb
    /// </summary>
    public partial class MQB : SoulsFile<MQB>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public enum MQBVersion : uint
        {
            DarkSouls2 = 0x94,
            DarkSouls2Scholar = 0xCA,
            Bloodborne = 0xCB,
            DarkSouls3 = 0xCC,
        }

        public bool BigEndian { get; set; }

        public MQBVersion Version { get; set; }

        public string Name { get; set; }

        public float Framerate { get; set; }

        public List<Resource> Resources { get; set; }

        public List<Cut> Cuts { get; set; }

        public string ResourceDirectory { get; set; }

        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            return br.GetASCII(0, 4) == "MQB ";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("MQB ");
            br.BigEndian = BigEndian = br.AssertSByte([0, -1]) == -1;
            br.AssertByte(0);
            sbyte longFormat = br.AssertSByte([0, -1]);
            br.AssertByte(0);
            Version = br.ReadEnum32<MQBVersion>();
            int headerSize = br.ReadInt32();

            if (Version != MQBVersion.DarkSouls2Scholar && longFormat == -1
                || Version == MQBVersion.DarkSouls2Scholar && longFormat == 0)
                throw new FormatException($"Unexpected long format {longFormat} for version {Version}.");

            if (Version == MQBVersion.DarkSouls2 && headerSize != 0x14
                || Version == MQBVersion.DarkSouls2Scholar && headerSize != 0x28
                || Version == MQBVersion.Bloodborne && headerSize != 0x20
                || Version == MQBVersion.DarkSouls3 && headerSize != 0x24)
                throw new FormatException($"Unexpected header size {headerSize} for version {Version}.");

            br.VarintLong = Version == MQBVersion.DarkSouls2Scholar;
            long resourcePathsOffset = br.ReadVarint();
            if (Version == MQBVersion.DarkSouls2Scholar)
            {
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }
            else if (Version >= MQBVersion.Bloodborne)
            {
                br.AssertInt32(1);
                br.AssertInt32(0);
                br.AssertInt32(0);
                if (Version >= MQBVersion.DarkSouls3)
                    br.AssertInt32(0);
            }

            Name = br.ReadFixStrW(0x40);
            Framerate = br.ReadSingle();
            int resourceCount = br.ReadInt32();
            int cutCount = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);

            Resources = new List<Resource>(resourceCount);
            for (int i = 0; i < resourceCount; i++)
                Resources.Add(new Resource(br, i));

            Cuts = new List<Cut>(cutCount);
            for (int i = 0; i < cutCount; i++)
                Cuts.Add(new Cut(br, Version));

            br.Position = resourcePathsOffset;
            long[] resourcePathOffsets = br.ReadVarints(resourceCount);
            ResourceDirectory = br.ReadUTF16();
            for (int i = 0; i < resourceCount; i++)
            {
                long offset = resourcePathOffsets[i];
                if (offset != 0)
                    Resources[i].Path = br.GetUTF16(offset);
            }
        }

        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            bw.VarintLong = Version == MQBVersion.DarkSouls2Scholar;

            bw.WriteASCII("MQB ");
            bw.WriteSByte((sbyte)(BigEndian ? -1 : 0));
            bw.WriteByte(0);
            bw.WriteSByte((sbyte)(Version == MQBVersion.DarkSouls2Scholar ? -1 : 0));
            bw.WriteByte(0);
            bw.WriteUInt32((uint)Version);
            switch (Version)
            {
                case MQBVersion.DarkSouls2: bw.WriteInt32(0x14); break;
                case MQBVersion.DarkSouls2Scholar: bw.WriteInt32(0x28); break;
                case MQBVersion.Bloodborne: bw.WriteInt32(0x20); break;
                case MQBVersion.DarkSouls3: bw.WriteInt32(0x24); break;
                default:
                    throw new NotImplementedException($"Missing header size for version {Version}.");
            }

            bw.ReserveVarint("ResourcePathsOffset");
            if (Version == MQBVersion.DarkSouls2Scholar)
            {
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }
            else if (Version >= MQBVersion.Bloodborne)
            {
                bw.WriteInt32(1);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                if (Version >= MQBVersion.DarkSouls3)
                    bw.WriteInt32(0);
            }

            bw.WriteFixStrW(Name, 0x40, 0x00);
            bw.WriteSingle(Framerate);
            bw.WriteInt32(Resources.Count);
            bw.WriteInt32(Cuts.Count);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            var allCustomData = new List<CustomData>();
            var customDataValueOffsets = new List<long>();

            for (int i = 0; i < Resources.Count; i++)
                Resources[i].Write(bw, i, allCustomData, customDataValueOffsets);

            var offsetsByDispos = new Dictionary<Disposition, long>();
            for (int i = 0; i < Cuts.Count; i++)
                Cuts[i].Write(bw, Version, offsetsByDispos, i, allCustomData, customDataValueOffsets);

            for (int i = 0; i < Cuts.Count; i++)
                Cuts[i].WriteTimelines(bw, Version, i);

            for (int i = 0; i < Cuts.Count; i++)
                Cuts[i].WriteTimelineCustomData(bw, i, allCustomData, customDataValueOffsets);

            for (int i = 0; i < Cuts.Count; i++)
                Cuts[i].WriteDisposOffsets(bw, offsetsByDispos, i);

            bw.FillVarint("ResourcePathsOffset", bw.Position);
            for (int i = 0; i < Resources.Count; i++)
                bw.ReserveVarint($"ResourcePathOffset{i}");

            bw.WriteUTF16(ResourceDirectory, true);
            for (int i = 0; i < Resources.Count; i++)
            {
                if (Resources[i].Path == null)
                {
                    bw.FillVarint($"ResourcePathOffset{i}", 0);
                }
                else
                {
                    bw.FillVarint($"ResourcePathOffset{i}", bw.Position);
                    bw.WriteUTF16(Resources[i].Path, true);
                }
            }

            // I know this is weird, but trust me.
            if (Version >= MQBVersion.Bloodborne)
            {
                bw.WriteInt16(0);
                bw.Pad(4);
            }

            for (int i = 0; i < allCustomData.Count; i++)
                allCustomData[i].WriteSequences(bw, i, customDataValueOffsets[i]);

            for (int i = 0; i < allCustomData.Count; i++)
                allCustomData[i].WriteSequencePoints(bw, i);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
