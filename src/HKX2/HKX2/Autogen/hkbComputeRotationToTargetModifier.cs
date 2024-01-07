using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbComputeRotationToTargetModifier : hkbModifier
    {
        public override uint Signature { get => 4275496900; }
        
        public Quaternion m_rotationOut;
        public Vector4 m_targetPosition;
        public Vector4 m_currentPosition;
        public Quaternion m_currentRotation;
        public Vector4 m_localAxisOfRotation;
        public Vector4 m_localFacingDirection;
        public bool m_resultIsDelta;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_rotationOut = des.ReadQuaternion(br);
            m_targetPosition = des.ReadVector4(br);
            m_currentPosition = des.ReadVector4(br);
            m_currentRotation = des.ReadQuaternion(br);
            m_localAxisOfRotation = des.ReadVector4(br);
            m_localFacingDirection = des.ReadVector4(br);
            m_resultIsDelta = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteQuaternion(bw, m_rotationOut);
            s.WriteVector4(bw, m_targetPosition);
            s.WriteVector4(bw, m_currentPosition);
            s.WriteQuaternion(bw, m_currentRotation);
            s.WriteVector4(bw, m_localAxisOfRotation);
            s.WriteVector4(bw, m_localFacingDirection);
            bw.WriteBoolean(m_resultIsDelta);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
