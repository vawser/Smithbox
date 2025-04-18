using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MQB
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Timeline
        {
            public List<Disposition> Dispositions { get; set; }

            public List<CustomData> CustomData { get; set; }

            /// <summary>
            /// Unknown; possibly a timeline index.
            /// </summary>
            public int Unk10 { get; set; }

            public Timeline()
            {
                Dispositions = new List<Disposition>();
                CustomData = new List<CustomData>();
            }

            internal Timeline(BinaryReaderEx br, MQBVersion version, Dictionary<long, Disposition> disposesByOffset)
            {
                long disposOffsetsOffset = br.ReadVarint();
                int disposCount = br.ReadInt32();
                if (version == MQBVersion.DarkSouls2Scholar)
                    br.AssertInt32(0);
                long customDataOffset = br.ReadVarint();
                int customDataCount = br.ReadInt32();
                Unk10 = br.ReadInt32();

                Dispositions = new List<Disposition>(disposCount);
                long[] disposOffsets = br.GetVarints(disposOffsetsOffset, disposCount);
                foreach (long disposOffset in disposOffsets)
                {
                    Dispositions.Add(disposesByOffset[disposOffset]);
                    disposesByOffset.Remove(disposOffset);
                }

                br.StepIn(customDataOffset);
                {
                    CustomData = new List<CustomData>(customDataCount);
                    for (int i = 0; i < customDataCount; i++)
                        CustomData.Add(new MQB.CustomData(br));
                }
                br.StepOut();
            }

            internal void WriteDispositions(BinaryWriterEx bw, Dictionary<Disposition, long> offsetsByDispos, List<CustomData> allCustomData, List<long> customDataValueOffsets)
            {
                foreach (Disposition dispos in Dispositions)
                {
                    offsetsByDispos[dispos] = bw.Position;
                    dispos.Write(bw, allCustomData, customDataValueOffsets);
                }
            }

            internal void Write(BinaryWriterEx bw, MQBVersion version, int cutIndex, int timelineIndex)
            {
                bw.ReserveVarint($"DisposOffsetsOffset[{cutIndex}:{timelineIndex}]");
                bw.WriteInt32(Dispositions.Count);
                if (version == MQBVersion.DarkSouls2Scholar)
                    bw.WriteInt32(0);
                bw.ReserveVarint($"TimelineCustomDataOffset[{cutIndex}:{timelineIndex}]");
                bw.WriteInt32(CustomData.Count);
                bw.WriteInt32(Unk10);
            }

            internal void WriteCustomData(BinaryWriterEx bw, int cutIndex, int timelineIndex, List<CustomData> allCustomData, List<long> customDataValueOffsets)
            {
                bw.FillVarint($"TimelineCustomDataOffset[{cutIndex}:{timelineIndex}]", bw.Position);
                foreach (CustomData customData in CustomData)
                    customData.Write(bw, allCustomData, customDataValueOffsets);
            }

            internal void WriteDisposOffsets(BinaryWriterEx bw, Dictionary<Disposition, long> offsetsByDispos, int cutIndex, int timelineIndex)
            {
                bw.FillVarint($"DisposOffsetsOffset[{cutIndex}:{timelineIndex}]", bw.Position);
                foreach (Disposition dispos in Dispositions)
                    bw.WriteVarint(offsetsByDispos[dispos]);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
