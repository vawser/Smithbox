using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum MotionType
    {
        MOTION_INVALID = 0,
        MOTION_DYNAMIC = 1,
        MOTION_SPHERE_INERTIA = 2,
        MOTION_BOX_INERTIA = 3,
        MOTION_KEYFRAMED = 4,
        MOTION_FIXED = 5,
        MOTION_THIN_BOX_INERTIA = 6,
        MOTION_CHARACTER = 7,
        MOTION_MAX_ID = 8,
    }
    
    public partial class hkpMotion : hkReferencedObject
    {
        public override uint Signature { get => 2255565999; }
        
        public MotionType m_type;
        public byte m_deactivationIntegrateCounter;
        public ushort m_deactivationNumInactiveFrames_0;
        public ushort m_deactivationNumInactiveFrames_1;
        public hkMotionState m_motionState;
        public Vector4 m_inertiaAndMassInv;
        public Vector4 m_linearVelocity;
        public Vector4 m_angularVelocity;
        public Vector4 m_deactivationRefPosition_0;
        public Vector4 m_deactivationRefPosition_1;
        public uint m_deactivationRefOrientation_0;
        public uint m_deactivationRefOrientation_1;
        public hkpMaxSizeMotion m_savedMotion;
        public ushort m_savedQualityTypeIndex;
        public short m_gravityFactor;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_type = (MotionType)br.ReadByte();
            m_deactivationIntegrateCounter = br.ReadByte();
            m_deactivationNumInactiveFrames_0 = br.ReadUInt16();
            m_deactivationNumInactiveFrames_1 = br.ReadUInt16();
            br.ReadUInt64();
            br.ReadUInt16();
            m_motionState = new hkMotionState();
            m_motionState.Read(des, br);
            m_inertiaAndMassInv = des.ReadVector4(br);
            m_linearVelocity = des.ReadVector4(br);
            m_angularVelocity = des.ReadVector4(br);
            m_deactivationRefPosition_0 = des.ReadVector4(br);
            m_deactivationRefPosition_1 = des.ReadVector4(br);
            m_deactivationRefOrientation_0 = br.ReadUInt32();
            m_deactivationRefOrientation_1 = br.ReadUInt32();
            m_savedMotion = des.ReadClassPointer<hkpMaxSizeMotion>(br);
            m_savedQualityTypeIndex = br.ReadUInt16();
            m_gravityFactor = br.ReadInt16();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte((byte)m_type);
            bw.WriteByte(m_deactivationIntegrateCounter);
            bw.WriteUInt16(m_deactivationNumInactiveFrames_0);
            bw.WriteUInt16(m_deactivationNumInactiveFrames_1);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
            m_motionState.Write(s, bw);
            s.WriteVector4(bw, m_inertiaAndMassInv);
            s.WriteVector4(bw, m_linearVelocity);
            s.WriteVector4(bw, m_angularVelocity);
            s.WriteVector4(bw, m_deactivationRefPosition_0);
            s.WriteVector4(bw, m_deactivationRefPosition_1);
            bw.WriteUInt32(m_deactivationRefOrientation_0);
            bw.WriteUInt32(m_deactivationRefOrientation_1);
            s.WriteClassPointer<hkpMaxSizeMotion>(bw, m_savedMotion);
            bw.WriteUInt16(m_savedQualityTypeIndex);
            bw.WriteInt16(m_gravityFactor);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
