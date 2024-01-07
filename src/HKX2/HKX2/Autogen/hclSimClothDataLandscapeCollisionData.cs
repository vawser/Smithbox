using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSimClothDataLandscapeCollisionData : IHavokObject
    {
        public virtual uint Signature { get => 492394560; }
        
        public float m_landscapeRadius;
        public bool m_enableStuckParticleDetection;
        public float m_stuckParticlesStretchFactorSq;
        public bool m_pinchDetectionEnabled;
        public sbyte m_pinchDetectionPriority;
        public float m_pinchDetectionRadius;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_landscapeRadius = br.ReadSingle();
            m_enableStuckParticleDetection = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_stuckParticlesStretchFactorSq = br.ReadSingle();
            m_pinchDetectionEnabled = br.ReadBoolean();
            m_pinchDetectionPriority = br.ReadSByte();
            br.ReadUInt16();
            m_pinchDetectionRadius = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_landscapeRadius);
            bw.WriteBoolean(m_enableStuckParticleDetection);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_stuckParticlesStretchFactorSq);
            bw.WriteBoolean(m_pinchDetectionEnabled);
            bw.WriteSByte(m_pinchDetectionPriority);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_pinchDetectionRadius);
        }
    }
}
