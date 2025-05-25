using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxVertexAnimation : hkReferencedObject
    {
        public override uint Signature { get => 661097651; }
        
        public float m_time;
        public hkxVertexBuffer m_vertData;
        public List<int> m_vertexIndexMap;
        public List<hkxVertexAnimationUsageMap> m_componentMap;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_time = br.ReadSingle();
            br.ReadUInt32();
            m_vertData = new hkxVertexBuffer();
            m_vertData.Read(des, br);
            m_vertexIndexMap = des.ReadInt32Array(br);
            m_componentMap = des.ReadClassArray<hkxVertexAnimationUsageMap>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_time);
            bw.WriteUInt32(0);
            m_vertData.Write(s, bw);
            s.WriteInt32Array(bw, m_vertexIndexMap);
            s.WriteClassArray<hkxVertexAnimationUsageMap>(bw, m_componentMap);
        }
    }
}
