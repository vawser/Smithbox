using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStorageSetupMeshSectionSectionVertexFloatChannel : hclStorageSetupMeshSectionSectionVertexChannel
    {
        public override uint Signature { get => 4249131545; }
        
        public List<float> m_vertexFloats;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_vertexFloats = des.ReadSingleArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteSingleArray(bw, m_vertexFloats);
        }
    }
}
