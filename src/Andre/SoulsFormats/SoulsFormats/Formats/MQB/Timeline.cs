using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MQB
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Timeline
        {
            public List<Event> Events { get; set; }

            public List<Parameter> Parameters { get; set; }

            /// <summary>
            /// Index of the parent timeline in the same cut, or -1 for a root. The parent is usually earlier in the list.
            /// The tree goes offset.sib → HKT → actors and camera → effects attached to them.
            /// </summary>
            public int ParentIndex { get; set; }

            // Where the parameters were read from, for finding unused timeline slots before them
            internal long ParametersOffset;

            public Timeline()
            {
                Events = new List<Event>();
                Parameters = new List<Parameter>();
            }

            internal Timeline(BinaryReaderEx br, MQBVersion version, Dictionary<long, Event> eventsByOffset)
            {
                long eventOffsetsOffset = br.ReadVarint();
                int eventCount = br.ReadInt32();
                if (version == MQBVersion.DarkSouls2Scholar)
                    br.AssertInt32(0);
                long parameterOffset = br.ReadVarint();
                int parameterCount = br.ReadInt32();
                ParentIndex = br.ReadInt32();
                ParametersOffset = parameterOffset;

                Events = new List<Event>(eventCount);
                long[] eventOffsets = br.GetVarints(eventOffsetsOffset, eventCount);
                foreach (long eventOffset in eventOffsets)
                {
                    Events.Add(eventsByOffset[eventOffset]);
                    eventsByOffset.Remove(eventOffset);
                }

                br.StepIn(parameterOffset);
                {
                    Parameters = new List<Parameter>(parameterCount);
                    for (int i = 0; i < parameterCount; i++)
                        Parameters.Add(new Parameter(br));
                }
                br.StepOut();
            }

            internal void WriteEvents(BinaryWriterEx bw, int timelineIndex, Dictionary<Event, long> offsetsByEvent, List<Parameter> allParameters, List<long> parameterValueOffsets)
            {
                foreach (Event ev in Events)
                {
                    offsetsByEvent[ev] = bw.Position;
                    ev.Write(bw, timelineIndex, ParentIndex, allParameters, parameterValueOffsets);
                }
            }

            internal void Write(BinaryWriterEx bw, MQBVersion version, int cutIndex, int timelineIndex)
            {
                bw.ReserveVarint($"EventOffsetsOffset[{cutIndex}:{timelineIndex}]");
                bw.WriteInt32(Events.Count);
                if (version == MQBVersion.DarkSouls2Scholar)
                    bw.WriteInt32(0);
                bw.ReserveVarint($"TimelineParametersOffset[{cutIndex}:{timelineIndex}]");
                bw.WriteInt32(Parameters.Count);
                bw.WriteInt32(ParentIndex);
            }

            internal void WriteParameters(BinaryWriterEx bw, int cutIndex, int timelineIndex, List<Parameter> allParameters, List<long> parameterValueOffsets)
            {
                bw.FillVarint($"TimelineParametersOffset[{cutIndex}:{timelineIndex}]", bw.Position);
                foreach (Parameter parameter in Parameters)
                    parameter.Write(bw, allParameters, parameterValueOffsets);
            }

            internal void WriteEventOffsets(BinaryWriterEx bw, Dictionary<Event, long> offsetsByEvent, int cutIndex, int timelineIndex)
            {
                bw.FillVarint($"EventOffsetsOffset[{cutIndex}:{timelineIndex}]", bw.Position);
                foreach (Event ev in Events)
                    bw.WriteVarint(offsetsByEvent[ev]);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
