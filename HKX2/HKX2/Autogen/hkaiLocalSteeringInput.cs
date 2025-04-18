using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiLocalSteeringInput : IHavokObject
    {
        public virtual uint Signature { get => 4063352422; }
        
        public Vector4 m_currentPosition;
        public Vector4 m_currentForward;
        public Vector4 m_currentUp;
        public Vector4 m_currentVelocity;
        public Vector4 m_desiredVelocity;
        public Vector4 m_localGoalPlane;
        public float m_distToLocalGoal;
        public bool m_applyKinematicConstraints;
        public bool m_applyAvoidanceSteering;
        public bool m_enableLocalSteering;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_currentPosition = des.ReadVector4(br);
            m_currentForward = des.ReadVector4(br);
            m_currentUp = des.ReadVector4(br);
            m_currentVelocity = des.ReadVector4(br);
            m_desiredVelocity = des.ReadVector4(br);
            m_localGoalPlane = des.ReadVector4(br);
            m_distToLocalGoal = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
            m_applyKinematicConstraints = br.ReadBoolean();
            m_applyAvoidanceSteering = br.ReadBoolean();
            m_enableLocalSteering = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_currentPosition);
            s.WriteVector4(bw, m_currentForward);
            s.WriteVector4(bw, m_currentUp);
            s.WriteVector4(bw, m_currentVelocity);
            s.WriteVector4(bw, m_desiredVelocity);
            s.WriteVector4(bw, m_localGoalPlane);
            bw.WriteSingle(m_distToLocalGoal);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteBoolean(m_applyKinematicConstraints);
            bw.WriteBoolean(m_applyAvoidanceSteering);
            bw.WriteBoolean(m_enableLocalSteering);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
        }
    }
}
