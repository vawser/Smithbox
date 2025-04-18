using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MQB
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Disposition
        {
            public int ID { get; set; }

            public int ResourceIndex { get; set; }

            /// <summary>
            /// Unknown; possibly a timeline index.
            /// </summary>
            public int Unk08 { get; set; }

            public int StartFrame { get; set; }

            /// <summary>
            /// Duration in frames.
            /// </summary>
            public int Duration { get; set; }

            public int Unk14 { get; set; }

            public int Unk18 { get; set; }

            public int Unk1C { get; set; }

            public int Unk20 { get; set; }

            public List<CustomData> CustomData { get; set; }

            public int Unk28 { get; set; }

            public List<Transform> Transforms { get; set; }

            public Disposition()
            {
                CustomData = new List<CustomData>();
                Transforms = new List<Transform>();
            }

            internal Disposition(BinaryReaderEx br)
            {
                ID = br.ReadInt32();
                ResourceIndex = br.ReadInt32();
                Unk08 = br.ReadInt32();
                StartFrame = br.ReadInt32();
                Duration = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();
                Unk20 = br.AssertInt32([0, 1]);
                int customDataCount = br.ReadInt32();
                Unk28 = br.ReadInt32();
                br.AssertInt32(0);

                CustomData = new List<CustomData>(customDataCount);
                for (int i = 0; i < customDataCount; i++)
                    CustomData.Add(new CustomData(br));

                br.AssertInt32(0);
                int transformCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                Transforms = new List<Transform>(transformCount);
                for (int i = 0; i < transformCount; i++)
                    Transforms.Add(new Transform(br));
            }

            internal void Write(BinaryWriterEx bw, List<CustomData> allCustomData, List<long> customDataValueOffsets)
            {
                bw.WriteInt32(ID);
                bw.WriteInt32(ResourceIndex);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(StartFrame);
                bw.WriteInt32(Duration);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);
                bw.WriteInt32(Unk20);
                bw.WriteInt32(CustomData.Count);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(0);

                foreach (CustomData customData in CustomData)
                    customData.Write(bw, allCustomData, customDataValueOffsets);

                bw.WriteInt32(0);
                bw.WriteInt32(Transforms.Count);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                foreach (Transform transform in Transforms)
                    transform.Write(bw);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
