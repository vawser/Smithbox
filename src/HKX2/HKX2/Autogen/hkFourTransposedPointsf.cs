using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkFourTransposedPointsf : IHavokObject
    {
        public virtual uint Signature { get => 3466051647; }
        
        public Vector4 m_vertices_0;
        public Vector4 m_vertices_1;
        public Vector4 m_vertices_2;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_vertices_0 = des.ReadVector4(br);
            m_vertices_1 = des.ReadVector4(br);
            m_vertices_2 = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_vertices_0);
            s.WriteVector4(bw, m_vertices_1);
            s.WriteVector4(bw, m_vertices_2);
        }
    }
}
