using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbPoweredRagdollControlData : IHavokObject
    {
        public virtual uint Signature { get => 257663515; }
        
        public float m_maxForce;
        public float m_tau;
        public float m_damping;
        public float m_proportionalRecoveryVelocity;
        public float m_constantRecoveryVelocity;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_maxForce = br.ReadSingle();
            m_tau = br.ReadSingle();
            m_damping = br.ReadSingle();
            m_proportionalRecoveryVelocity = br.ReadSingle();
            m_constantRecoveryVelocity = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_maxForce);
            bw.WriteSingle(m_tau);
            bw.WriteSingle(m_damping);
            bw.WriteSingle(m_proportionalRecoveryVelocity);
            bw.WriteSingle(m_constantRecoveryVelocity);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
