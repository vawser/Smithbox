using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStaticShadowBufferDefinition : hclBufferDefinition
    {
        public override uint Signature { get => 1150964083; }
        
        public List<Vector4> m_staticPositions;
        public List<Vector4> m_staticNormals;
        public List<Vector4> m_staticTangents;
        public List<Vector4> m_staticBiTangents;
        public List<ushort> m_triangleIndices;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_staticPositions = des.ReadVector4Array(br);
            m_staticNormals = des.ReadVector4Array(br);
            m_staticTangents = des.ReadVector4Array(br);
            m_staticBiTangents = des.ReadVector4Array(br);
            m_triangleIndices = des.ReadUInt16Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4Array(bw, m_staticPositions);
            s.WriteVector4Array(bw, m_staticNormals);
            s.WriteVector4Array(bw, m_staticTangents);
            s.WriteVector4Array(bw, m_staticBiTangents);
            s.WriteUInt16Array(bw, m_triangleIndices);
        }
    }
}
