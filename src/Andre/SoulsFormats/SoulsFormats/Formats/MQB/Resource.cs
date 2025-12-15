using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MQB
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Resource
        {
            public string Name { get; set; }

            public int ParentIndex { get; set; }

            public int Unk48 { get; set; }

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
                Unk48 = br.ReadInt32();
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
                bw.WriteInt32(Unk48);
                bw.WriteInt32(Parameters.Count);

                foreach (Parameter parameter in Parameters)
                    parameter.Write(bw, allParameters, parameterValueOffsets);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
