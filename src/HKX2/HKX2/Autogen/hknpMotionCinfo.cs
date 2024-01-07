using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpMotionCinfo : IHavokObject
    {
        public virtual uint Signature { get => 1765612310; }
        
        public ushort m_motionPropertiesId;
        public bool m_enableDeactivation;
        public float m_inverseMass;
        public float m_massFactor;
        public float m_maxLinearAccelerationDistancePerStep;
        public float m_maxRotationToPreventTunneling;
        public Vector4 m_inverseInertiaLocal;
        public Vector4 m_centerOfMassWorld;
        public Quaternion m_orientation;
        public Vector4 m_linearVelocity;
        public Vector4 m_angularVelocity;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_motionPropertiesId = br.ReadUInt16();
            m_enableDeactivation = br.ReadBoolean();
            br.ReadByte();
            m_inverseMass = br.ReadSingle();
            m_massFactor = br.ReadSingle();
            m_maxLinearAccelerationDistancePerStep = br.ReadSingle();
            m_maxRotationToPreventTunneling = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt32();
            m_inverseInertiaLocal = des.ReadVector4(br);
            m_centerOfMassWorld = des.ReadVector4(br);
            m_orientation = des.ReadQuaternion(br);
            m_linearVelocity = des.ReadVector4(br);
            m_angularVelocity = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_motionPropertiesId);
            bw.WriteBoolean(m_enableDeactivation);
            bw.WriteByte(0);
            bw.WriteSingle(m_inverseMass);
            bw.WriteSingle(m_massFactor);
            bw.WriteSingle(m_maxLinearAccelerationDistancePerStep);
            bw.WriteSingle(m_maxRotationToPreventTunneling);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            s.WriteVector4(bw, m_inverseInertiaLocal);
            s.WriteVector4(bw, m_centerOfMassWorld);
            s.WriteQuaternion(bw, m_orientation);
            s.WriteVector4(bw, m_linearVelocity);
            s.WriteVector4(bw, m_angularVelocity);
        }
    }
}
