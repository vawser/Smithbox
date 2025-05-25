using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclMeshMeshDeformOperatorTriangleVertexPair : IHavokObject
    {
        public virtual uint Signature { get => 1404285965; }
        
        public Vector4 m_localPosition;
        public Vector4 m_localNormal;
        public ushort m_triangleIndex;
        public float m_weight;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_localPosition = des.ReadVector4(br);
            m_localNormal = des.ReadVector4(br);
            m_triangleIndex = br.ReadUInt16();
            br.ReadUInt16();
            m_weight = br.ReadSingle();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_localPosition);
            s.WriteVector4(bw, m_localNormal);
            bw.WriteUInt16(m_triangleIndex);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_weight);
            bw.WriteUInt64(0);
        }
    }
}
