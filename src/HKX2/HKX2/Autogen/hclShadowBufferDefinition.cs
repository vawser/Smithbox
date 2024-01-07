using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclShadowBufferDefinition : hclBufferDefinition
    {
        public override uint Signature { get => 1263612896; }
        
        public List<ushort> m_triangleIndices;
        public bool m_shadowPositions;
        public bool m_shadowNormals;
        public bool m_shadowTangents;
        public bool m_shadowBiTangents;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_triangleIndices = des.ReadUInt16Array(br);
            m_shadowPositions = br.ReadBoolean();
            m_shadowNormals = br.ReadBoolean();
            m_shadowTangents = br.ReadBoolean();
            m_shadowBiTangents = br.ReadBoolean();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteUInt16Array(bw, m_triangleIndices);
            bw.WriteBoolean(m_shadowPositions);
            bw.WriteBoolean(m_shadowNormals);
            bw.WriteBoolean(m_shadowTangents);
            bw.WriteBoolean(m_shadowBiTangents);
            bw.WriteUInt32(0);
        }
    }
}
