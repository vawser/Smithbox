using System.Collections.Generic;
using System.Linq;

namespace SoulsFormats
{
    public partial class MQB
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Cut
        {
            public string Name { get; set; }

            public int Unk44 { get; set; }

            /// <summary>
            /// Duration of the cut in frames.
            /// </summary>
            public int Duration { get; set; }

            public List<Timeline> Timelines { get; set; }

            public Cut()
            {
                Name = "";
                Timelines = new List<Timeline>();
            }

            internal Cut(BinaryReaderEx br, MQBVersion version)
            {
                Name = br.ReadFixStrW(0x40);
                int disposCount = br.ReadInt32();
                Unk44 = br.ReadInt32();
                Duration = br.ReadInt32();
                br.AssertInt32(0);

                int timelineCount = br.ReadInt32();
                if (version == MQBVersion.DarkSouls2Scholar)
                    br.AssertInt32(0);
                long timelinesOffset = br.ReadVarint();
                if (version != MQBVersion.DarkSouls2Scholar)
                    br.AssertInt64(0);

                var disposesByOffset = new Dictionary<long, Disposition>(disposCount);
                for (int i = 0; i < disposCount; i++)
                    disposesByOffset[br.Position] = new Disposition(br);

                br.StepIn(timelinesOffset);
                {
                    Timelines = new List<Timeline>(timelineCount);
                    for (int i = 0; i < timelineCount; i++)
                        Timelines.Add(new Timeline(br, version, disposesByOffset));
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, MQBVersion version, Dictionary<Disposition, long> offsetsByDispos, int cutIndex, List<CustomData> allCustomData, List<long> customDataValueOffsets)
            {
                int disposCount = Timelines.Sum(g => g.Dispositions.Count);
                bw.WriteFixStrW(Name, 0x40, 0x00);
                bw.WriteInt32(disposCount);
                bw.WriteInt32(Unk44);
                bw.WriteInt32(Duration);
                bw.WriteInt32(0);

                bw.WriteInt32(Timelines.Count);
                if (version == MQBVersion.DarkSouls2Scholar)
                    bw.WriteInt32(0);
                bw.ReserveVarint($"TimelinesOffset{cutIndex}");
                if (version != MQBVersion.DarkSouls2Scholar)
                    bw.WriteInt64(0);

                foreach (Timeline timeline in Timelines)
                    timeline.WriteDispositions(bw, offsetsByDispos, allCustomData, customDataValueOffsets);
            }

            internal void WriteTimelines(BinaryWriterEx bw, MQBVersion version, int cutIndex)
            {
                bw.FillVarint($"TimelinesOffset{cutIndex}", bw.Position);
                for (int i = 0; i < Timelines.Count; i++)
                    Timelines[i].Write(bw, version, cutIndex, i);
            }

            internal void WriteTimelineCustomData(BinaryWriterEx bw, int cutIndex, List<CustomData> allCustomData, List<long> customDataValueOffsets)
            {
                for (int i = 0; i < Timelines.Count; i++)
                    Timelines[i].WriteCustomData(bw, cutIndex, i, allCustomData, customDataValueOffsets);
            }

            internal void WriteDisposOffsets(BinaryWriterEx bw, Dictionary<Disposition, long> offsetsByDispos, int cutIndex)
            {
                for (int i = 0; i < Timelines.Count; i++)
                    Timelines[i].WriteDisposOffsets(bw, offsetsByDispos, cutIndex, i);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
