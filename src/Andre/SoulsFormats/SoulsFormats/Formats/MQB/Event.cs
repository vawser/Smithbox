using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MQB
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Event
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

            public List<Parameter> Parameters { get; set; }

            public int Unk28 { get; set; }

            public List<Transform> Transforms { get; set; }

            public Event()
            {
                Parameters = new List<Parameter>();
                Transforms = new List<Transform>();
            }

            internal Event(BinaryReaderEx br)
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
                int parameterCount = br.ReadInt32();
                Unk28 = br.ReadInt32();
                br.AssertInt32(0);

                Parameters = new List<Parameter>(parameterCount);
                for (int i = 0; i < parameterCount; i++)
                    Parameters.Add(new Parameter(br));

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

            internal void Write(BinaryWriterEx bw, List<Parameter> allParameters, List<long> parameterValueOffsets)
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
                bw.WriteInt32(Parameters.Count);
                bw.WriteInt32(Unk28);
                bw.WriteInt32(0);

                foreach (Parameter parameter in Parameters)
                    parameter.Write(bw, allParameters, parameterValueOffsets);

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
