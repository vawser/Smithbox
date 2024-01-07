using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpMinMaxQuadTree : IHavokObject
    {
        public virtual uint Signature { get => 242984164; }
        
        public List<hknpMinMaxQuadTreeMinMaxLevel> m_coarseTreeData;
        public Vector4 m_offset;
        public float m_multiplier;
        public float m_invMultiplier;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_coarseTreeData = des.ReadClassArray<hknpMinMaxQuadTreeMinMaxLevel>(br);
            m_offset = des.ReadVector4(br);
            m_multiplier = br.ReadSingle();
            m_invMultiplier = br.ReadSingle();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hknpMinMaxQuadTreeMinMaxLevel>(bw, m_coarseTreeData);
            s.WriteVector4(bw, m_offset);
            bw.WriteSingle(m_multiplier);
            bw.WriteSingle(m_invMultiplier);
            bw.WriteUInt64(0);
        }
    }
}
