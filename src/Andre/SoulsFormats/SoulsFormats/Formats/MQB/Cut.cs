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
                int eventCount = br.ReadInt32();
                Unk44 = br.ReadInt32();
                Duration = br.ReadInt32();
                br.AssertInt32(0);

                int timelineCount = br.ReadInt32();
                if (version == MQBVersion.DarkSouls2Scholar)
                    br.AssertInt32(0);
                long timelinesOffset = br.ReadVarint();
                if (version != MQBVersion.DarkSouls2Scholar)
                    br.AssertInt64(0);

                var eventsByOffset = new Dictionary<long, Event>(eventCount);
                for (int i = 0; i < eventCount; i++)
                    eventsByOffset[br.Position] = new Event(br);

                br.StepIn(timelinesOffset);
                {
                    Timelines = new List<Timeline>(timelineCount);
                    for (int i = 0; i < timelineCount; i++)
                        Timelines.Add(new Timeline(br, version, eventsByOffset));
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, MQBVersion version, Dictionary<Event, long> offsetsByEvent, int cutIndex, List<Parameter> allParameters, List<long> parameterValueOffsets)
            {
                int eventCount = Timelines.Sum(g => g.Events.Count);
                bw.WriteFixStrW(Name, 0x40, 0x00);
                bw.WriteInt32(eventCount);
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
                    timeline.WriteEvents(bw, offsetsByEvent, allParameters, parameterValueOffsets);
            }

            internal void WriteTimelines(BinaryWriterEx bw, MQBVersion version, int cutIndex)
            {
                bw.FillVarint($"TimelinesOffset{cutIndex}", bw.Position);
                for (int i = 0; i < Timelines.Count; i++)
                    Timelines[i].Write(bw, version, cutIndex, i);
            }

            internal void WriteTimelineParameters(BinaryWriterEx bw, int cutIndex, List<Parameter> allParameters, List<long> parameterValueOffsets)
            {
                for (int i = 0; i < Timelines.Count; i++)
                    Timelines[i].WriteParameters(bw, cutIndex, i, allParameters, parameterValueOffsets);
            }

            internal void WriteEventOffsets(BinaryWriterEx bw, Dictionary<Event, long> offsetsByEvent, int cutIndex)
            {
                for (int i = 0; i < Timelines.Count; i++)
                    Timelines[i].WriteEventOffsets(bw, offsetsByEvent, cutIndex, i);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
