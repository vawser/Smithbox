using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class fsnpCustomMeshParameterPrimitiveData : IHavokObject
    {
        public virtual uint Signature { get => 3013233085; }
        
        public List<byte> m_vertexData;
        public List<byte> m_triangleData;
        public List<byte> m_primitiveData;
        public uint m_materialNameData;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_vertexData = des.ReadByteArray(br);
            m_triangleData = des.ReadByteArray(br);
            m_primitiveData = des.ReadByteArray(br);
            m_materialNameData = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteByteArray(bw, m_vertexData);
            s.WriteByteArray(bw, m_triangleData);
            s.WriteByteArray(bw, m_primitiveData);
            bw.WriteUInt32(m_materialNameData);
            bw.WriteUInt32(0);
        }
    }
}
