using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MQB
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Resource
        {
            public string Name { get; set; }

            /// <summary>
            /// Index of the owning resource, or -1. An actor or camera is owned by the HKT whose skeleton animates it.
            /// </summary>
            public int ParentIndex { get; set; }

            /// <summary>
            /// What kind of resource this is, which decides the parameters its events take. For example 1 model,
            /// 3 camera, 27 subtitle, 103 SFX, 135 sound, 243 point light, 265 offset (.sib), 266 animation (.HKT).
            /// </summary>
            public int Type { get; set; }

            public List<Parameter> Parameters { get; set; }

            public string Path { get; set; }

            public Resource()
            {
                Name = "";
                Parameters = new List<Parameter>();
            }

            internal Resource(BinaryReaderEx br, int resourceIndex)
            {
                Name = br.ReadFixStrW(0x40);
                ParentIndex = br.ReadInt32();
                br.AssertInt32(resourceIndex);
                Type = br.ReadInt32();
                int parameterCount = br.ReadInt32();

                Parameters = new List<Parameter>(parameterCount);
                for (int i = 0; i < parameterCount; i++)
                    Parameters.Add(new Parameter(br));
            }

            internal void Write(BinaryWriterEx bw, int resourceIndex, List<Parameter> allParameters, List<long> parameterValueOffsets)
            {
                bw.WriteFixStrW(Name, 0x40, 0x00);
                bw.WriteInt32(ParentIndex);
                bw.WriteInt32(resourceIndex);
                bw.WriteInt32(Type);
                bw.WriteInt32(Parameters.Count);

                foreach (Parameter parameter in Parameters)
                    parameter.Write(bw, allParameters, parameterValueOffsets);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
