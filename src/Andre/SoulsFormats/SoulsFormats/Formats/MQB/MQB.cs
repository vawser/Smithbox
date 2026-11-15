using System;
using System.Collections.Generic;
using System.Linq;

namespace SoulsFormats
{
    /// <summary>
    /// A cutscene definition format which dates back to at least Armored Core V, short for MovieSequencer Binary. Extension: .mqb
    /// </summary>
    public partial class MQB : SoulsFile<MQB>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public enum MQBVersion : uint
        {
            DarkSouls2 = 0x94,
            DarkSouls2Scholar = 0xCA,
            Bloodborne = 0xCB,

            /// <summary>
            /// Also used by Sekiro, Elden Ring and Armored Core VI.
            /// </summary>
            DarkSouls3 = 0xCC,

            /// <summary>
            /// Same layout as <see cref="DarkSouls3"/>.
            /// </summary>
            Nightreign = 0xCD,
        }

        public bool BigEndian { get; set; }

        public MQBVersion Version { get; set; }

        public string Name { get; set; }

        public float Framerate { get; set; }

        public List<Resource> Resources { get; set; }

        public List<Cut> Cuts { get; set; }

        public string ResourceDirectory { get; set; }

        /// <summary>
        /// Empty timeline records after the last cut's timelines that no cut counts or refers to.
        /// Seen in a few DS3 files, probably left behind by the authoring tool when timelines were deleted.
        /// Kept only so the file writes back the same; the game can't see them.
        /// </summary>
        public int UnusedTimelineSlots { get; set; }

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
                || Version >= MQBVersion.DarkSouls3 && headerSize != 0x24)
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
            int unkX50 = br.ReadInt32();
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

            UnusedTimelineSlots = ReadUnusedTimelineSlots(br);

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
                case MQBVersion.DarkSouls3:
                case MQBVersion.Nightreign: bw.WriteInt32(0x24); break;
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

            var allParameters = new List<Parameter>();
            var ParameterValueOffsets = new List<long>();

            for (int i = 0; i < Resources.Count; i++)
                Resources[i].Write(bw, i, allParameters, ParameterValueOffsets);

            var offsetsByEvents = new Dictionary<Event, long>();
            for (int i = 0; i < Cuts.Count; i++)
                Cuts[i].Write(bw, Version, offsetsByEvents, i, allParameters, ParameterValueOffsets);

            for (int i = 0; i < Cuts.Count; i++)
                Cuts[i].WriteTimelines(bw, Version, i);

            WriteUnusedTimelineSlots(bw);

            for (int i = 0; i < Cuts.Count; i++)
                Cuts[i].WriteTimelineParameters(bw, i, allParameters, ParameterValueOffsets);

            for (int i = 0; i < Cuts.Count; i++)
                Cuts[i].WriteEventOffsets(bw, offsetsByEvents, i);

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

            for (int i = 0; i < allParameters.Count; i++)
                allParameters[i].WriteSequences(bw, i, ParameterValueOffsets[i]);

            for (int i = 0; i < allParameters.Count; i++)
                allParameters[i].WriteSequencePoints(bw, i);
        }

        internal static int TimelineRecordSize(MQBVersion version) => version == MQBVersion.DarkSouls2Scholar ? 0x20 : 0x14;

        /// <summary>
        /// Counts the empty records between the end of the last cut's timelines and the timeline parameters,
        /// which every timeline's parameter offset points into.
        /// </summary>
        private int ReadUnusedTimelineSlots(BinaryReaderEx br)
        {
            var withTimelines = Cuts.Where(c => c.Timelines.Count > 0).ToList();
            if (withTimelines.Count == 0)
                return 0;

            int recordSize = TimelineRecordSize(Version);
            long timelinesEnd = withTimelines.Max(c => c.TimelinesOffset + c.Timelines.Count * recordSize);
            long parametersStart = withTimelines.SelectMany(c => c.Timelines).Min(t => t.ParametersOffset);
            if (parametersStart <= timelinesEnd)
                return 0;

            long gap = parametersStart - timelinesEnd;
            if (gap % recordSize != 0)
                throw new FormatException($"Unexpected 0x{gap:X} bytes between the timelines and their parameters.");

            int count = (int)(gap / recordSize);
            br.StepIn(timelinesEnd);
            {
                for (int i = 0; i < count; i++)
                {
                    br.AssertVarint(parametersStart); // Event offsets offset
                    br.AssertInt32(0); // Event count
                    if (Version == MQBVersion.DarkSouls2Scholar)
                        br.AssertInt32(0);
                    br.AssertVarint(0); // Parameters offset
                    br.AssertInt32(0); // Parameter count
                    br.AssertInt32(0); // Parent timeline
                }
            }
            br.StepOut();
            return count;
        }

        private void WriteUnusedTimelineSlots(BinaryWriterEx bw)
        {
            // Their event offsets point to where the timeline parameters start, right after them
            long parametersStart = bw.Position + UnusedTimelineSlots * TimelineRecordSize(Version);
            for (int i = 0; i < UnusedTimelineSlots; i++)
            {
                bw.WriteVarint(parametersStart);
                bw.WriteInt32(0);
                if (Version == MQBVersion.DarkSouls2Scholar)
                    bw.WriteInt32(0);
                bw.WriteVarint(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
