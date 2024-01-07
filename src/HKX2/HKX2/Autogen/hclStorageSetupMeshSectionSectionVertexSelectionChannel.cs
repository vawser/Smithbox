using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStorageSetupMeshSectionSectionVertexSelectionChannel : hclStorageSetupMeshSectionSectionVertexChannel
    {
        public override uint Signature { get => 1482862195; }
        
        public List<uint> m_vertexIndices;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_vertexIndices = des.ReadUInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteUInt32Array(bw, m_vertexIndices);
        }
    }
}
