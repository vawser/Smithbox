using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclTransitionConstraintSet : hclConstraintSet
    {
        public override uint Signature { get => 2633903229; }
        
        public List<hclTransitionConstraintSetPerParticle> m_perParticleData;
        public float m_toAnimPeriod;
        public float m_toAnimPlusDelayPeriod;
        public float m_toSimPeriod;
        public float m_toSimPlusDelayPeriod;
        public uint m_referenceMeshBufferIdx;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_perParticleData = des.ReadClassArray<hclTransitionConstraintSetPerParticle>(br);
            m_toAnimPeriod = br.ReadSingle();
            m_toAnimPlusDelayPeriod = br.ReadSingle();
            m_toSimPeriod = br.ReadSingle();
            m_toSimPlusDelayPeriod = br.ReadSingle();
            m_referenceMeshBufferIdx = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclTransitionConstraintSetPerParticle>(bw, m_perParticleData);
            bw.WriteSingle(m_toAnimPeriod);
            bw.WriteSingle(m_toAnimPlusDelayPeriod);
            bw.WriteSingle(m_toSimPeriod);
            bw.WriteSingle(m_toSimPlusDelayPeriod);
            bw.WriteUInt32(m_referenceMeshBufferIdx);
            bw.WriteUInt32(0);
        }
    }
}
