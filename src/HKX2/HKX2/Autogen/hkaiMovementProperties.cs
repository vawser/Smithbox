using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum KinematicConstraintType
    {
        CONSTRAINTS_NONE = 0,
        CONSTRAINTS_LINEAR_AND_ANGULAR = 1,
        CONSTRAINTS_LINEAR_ONLY = 2,
    }
    
    public partial class hkaiMovementProperties : IHavokObject
    {
        public virtual uint Signature { get => 3491831670; }
        
        public float m_minVelocity;
        public float m_maxVelocity;
        public float m_maxAcceleration;
        public float m_maxDeceleration;
        public float m_leftTurnRadius;
        public float m_rightTurnRadius;
        public float m_maxAngularVelocity;
        public float m_maxTurnVelocity;
        public KinematicConstraintType m_kinematicConstraintType;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_minVelocity = br.ReadSingle();
            m_maxVelocity = br.ReadSingle();
            m_maxAcceleration = br.ReadSingle();
            m_maxDeceleration = br.ReadSingle();
            m_leftTurnRadius = br.ReadSingle();
            m_rightTurnRadius = br.ReadSingle();
            m_maxAngularVelocity = br.ReadSingle();
            m_maxTurnVelocity = br.ReadSingle();
            m_kinematicConstraintType = (KinematicConstraintType)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_minVelocity);
            bw.WriteSingle(m_maxVelocity);
            bw.WriteSingle(m_maxAcceleration);
            bw.WriteSingle(m_maxDeceleration);
            bw.WriteSingle(m_leftTurnRadius);
            bw.WriteSingle(m_rightTurnRadius);
            bw.WriteSingle(m_maxAngularVelocity);
            bw.WriteSingle(m_maxTurnVelocity);
            bw.WriteByte((byte)m_kinematicConstraintType);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
