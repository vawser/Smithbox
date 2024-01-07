using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ForceUpgrade610
    {
        HCL_FORCE_UPGRADE610 = 0,
    }
    
    public partial class hclMoveParticlesOperator : hclOperator
    {
        public override uint Signature { get => 3864686620; }
        
        public List<hclMoveParticlesOperatorVertexParticlePair> m_vertexParticlePairs;
        public uint m_simClothIndex;
        public uint m_refBufferIdx;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_vertexParticlePairs = des.ReadClassArray<hclMoveParticlesOperatorVertexParticlePair>(br);
            m_simClothIndex = br.ReadUInt32();
            m_refBufferIdx = br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclMoveParticlesOperatorVertexParticlePair>(bw, m_vertexParticlePairs);
            bw.WriteUInt32(m_simClothIndex);
            bw.WriteUInt32(m_refBufferIdx);
        }
    }
}
