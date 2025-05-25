using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum InitialVelocityCoordinates
    {
        INITIAL_VELOCITY_IN_WORLD_COORDINATES = 0,
        INITIAL_VELOCITY_IN_MODEL_COORDINATES = 1,
    }
    
    public enum MotionMode
    {
        MOTION_MODE_FOLLOW_ANIMATION = 0,
        MOTION_MODE_DYNAMIC = 1,
    }
    
    public partial class hkbCharacterControllerModifier : hkbModifier
    {
        public override uint Signature { get => 1948489251; }
        
        public hkbCharacterControllerModifierControlData m_controlData;
        public Vector4 m_initialVelocity;
        public InitialVelocityCoordinates m_initialVelocityCoordinates;
        public MotionMode m_motionMode;
        public bool m_forceDownwardMomentum;
        public bool m_applyGravity;
        public bool m_setInitialVelocity;
        public bool m_isTouchingGround;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_controlData = new hkbCharacterControllerModifierControlData();
            m_controlData.Read(des, br);
            br.ReadUInt64();
            m_initialVelocity = des.ReadVector4(br);
            m_initialVelocityCoordinates = (InitialVelocityCoordinates)br.ReadSByte();
            m_motionMode = (MotionMode)br.ReadSByte();
            m_forceDownwardMomentum = br.ReadBoolean();
            m_applyGravity = br.ReadBoolean();
            m_setInitialVelocity = br.ReadBoolean();
            m_isTouchingGround = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_controlData.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_initialVelocity);
            bw.WriteSByte((sbyte)m_initialVelocityCoordinates);
            bw.WriteSByte((sbyte)m_motionMode);
            bw.WriteBoolean(m_forceDownwardMomentum);
            bw.WriteBoolean(m_applyGravity);
            bw.WriteBoolean(m_setInitialVelocity);
            bw.WriteBoolean(m_isTouchingGround);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
        }
    }
}
