using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class fsnpCustomMeshParameterTriangleData : IHavokObject
    {
        public virtual uint Signature { get => 3419166696; }
        
        public uint m_primitiveDataIndex;
        public uint m_triangleDataIndex;
        public uint m_vertexIndexA;
        public uint m_vertexIndexB;
        public uint m_vertexIndexC;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_primitiveDataIndex = br.ReadUInt32();
            m_triangleDataIndex = br.ReadUInt32();
            m_vertexIndexA = br.ReadUInt32();
            m_vertexIndexB = br.ReadUInt32();
            m_vertexIndexC = br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_primitiveDataIndex);
            bw.WriteUInt32(m_triangleDataIndex);
            bw.WriteUInt32(m_vertexIndexA);
            bw.WriteUInt32(m_vertexIndexB);
            bw.WriteUInt32(m_vertexIndexC);
        }
    }
}
