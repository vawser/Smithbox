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

            public List<CustomData> CustomData { get; set; }

            public string Path { get; set; }

            public Resource()
            {
                Name = "";
                CustomData = new List<CustomData>();
            }

            internal Resource(BinaryReaderEx br, int resourceIndex)
            {
                Name = br.ReadFixStrW(0x40);
                ParentIndex = br.ReadInt32();
                br.AssertInt32(resourceIndex);
                Unk48 = br.ReadInt32();
                int customDataCount = br.ReadInt32();

                CustomData = new List<CustomData>(customDataCount);
                for (int i = 0; i < customDataCount; i++)
                    CustomData.Add(new CustomData(br));
            }

            internal void Write(BinaryWriterEx bw, int resourceIndex, List<CustomData> allCustomData, List<long> customDataValueOffsets)
            {
                bw.WriteFixStrW(Name, 0x40, 0x00);
                bw.WriteInt32(ParentIndex);
                bw.WriteInt32(resourceIndex);
                bw.WriteInt32(Unk48);
                bw.WriteInt32(CustomData.Count);

                foreach (CustomData customData in CustomData)
                    customData.Write(bw, allCustomData, customDataValueOffsets);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
