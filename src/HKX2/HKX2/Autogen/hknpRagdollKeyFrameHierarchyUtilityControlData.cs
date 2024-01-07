using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpRagdollKeyFrameHierarchyUtilityControlData : IHavokObject
    {
        public virtual uint Signature { get => 2748361841; }
        
        public float m_hierarchyGain;
        public float m_velocityDamping;
        public float m_accelerationGain;
        public float m_velocityGain;
        public float m_positionGain;
        public float m_positionMaxLinearVelocity;
        public float m_positionMaxAngularVelocity;
        public float m_snapGain;
        public float m_snapMaxLinearVelocity;
        public float m_snapMaxAngularVelocity;
        public float m_snapMaxLinearDistance;
        public float m_snapMaxAngularDistance;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_hierarchyGain = br.ReadSingle();
            m_velocityDamping = br.ReadSingle();
            m_accelerationGain = br.ReadSingle();
            m_velocityGain = br.ReadSingle();
            m_positionGain = br.ReadSingle();
            m_positionMaxLinearVelocity = br.ReadSingle();
            m_positionMaxAngularVelocity = br.ReadSingle();
            m_snapGain = br.ReadSingle();
            m_snapMaxLinearVelocity = br.ReadSingle();
            m_snapMaxAngularVelocity = br.ReadSingle();
            m_snapMaxLinearDistance = br.ReadSingle();
            m_snapMaxAngularDistance = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_hierarchyGain);
            bw.WriteSingle(m_velocityDamping);
            bw.WriteSingle(m_accelerationGain);
            bw.WriteSingle(m_velocityGain);
            bw.WriteSingle(m_positionGain);
            bw.WriteSingle(m_positionMaxLinearVelocity);
            bw.WriteSingle(m_positionMaxAngularVelocity);
            bw.WriteSingle(m_snapGain);
            bw.WriteSingle(m_snapMaxLinearVelocity);
            bw.WriteSingle(m_snapMaxAngularVelocity);
            bw.WriteSingle(m_snapMaxLinearDistance);
            bw.WriteSingle(m_snapMaxAngularDistance);
        }
    }
}
