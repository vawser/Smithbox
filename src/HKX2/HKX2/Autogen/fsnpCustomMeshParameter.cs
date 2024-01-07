using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class fsnpCustomMeshParameter : hkReferencedObject
    {
        public override uint Signature { get => 3608770766; }
        
        public List<fsnpCustomMeshParameterTriangleData> m_triangleDataArray;
        public List<fsnpCustomMeshParameterPrimitiveData> m_primitiveDataArray;
        public int m_vertexDataStride;
        public int m_triangleDataStride;
        public uint m_version;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_triangleDataArray = des.ReadClassArray<fsnpCustomMeshParameterTriangleData>(br);
            m_primitiveDataArray = des.ReadClassArray<fsnpCustomMeshParameterPrimitiveData>(br);
            m_vertexDataStride = br.ReadInt32();
            m_triangleDataStride = br.ReadInt32();
            m_version = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<fsnpCustomMeshParameterTriangleData>(bw, m_triangleDataArray);
            s.WriteClassArray<fsnpCustomMeshParameterPrimitiveData>(bw, m_primitiveDataArray);
            bw.WriteInt32(m_vertexDataStride);
            bw.WriteInt32(m_triangleDataStride);
            bw.WriteUInt32(m_version);
            bw.WriteUInt32(0);
        }
    }
}
