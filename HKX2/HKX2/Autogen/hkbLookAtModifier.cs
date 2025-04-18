using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbLookAtModifier : hkbModifier
    {
        public override uint Signature { get => 1858727363; }
        
        public Vector4 m_targetWS;
        public Vector4 m_headForwardLS;
        public Vector4 m_neckForwardLS;
        public Vector4 m_neckRightLS;
        public Vector4 m_eyePositionHS;
        public float m_newTargetGain;
        public float m_onGain;
        public float m_offGain;
        public float m_limitAngleDegrees;
        public float m_limitAngleLeft;
        public float m_limitAngleRight;
        public float m_limitAngleUp;
        public float m_limitAngleDown;
        public short m_headIndex;
        public short m_neckIndex;
        public bool m_isOn;
        public bool m_individualLimitsOn;
        public bool m_isTargetInsideLimitCone;
        public short m_SensingAngle;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_targetWS = des.ReadVector4(br);
            m_headForwardLS = des.ReadVector4(br);
            m_neckForwardLS = des.ReadVector4(br);
            m_neckRightLS = des.ReadVector4(br);
            m_eyePositionHS = des.ReadVector4(br);
            m_newTargetGain = br.ReadSingle();
            m_onGain = br.ReadSingle();
            m_offGain = br.ReadSingle();
            m_limitAngleDegrees = br.ReadSingle();
            m_limitAngleLeft = br.ReadSingle();
            m_limitAngleRight = br.ReadSingle();
            m_limitAngleUp = br.ReadSingle();
            m_limitAngleDown = br.ReadSingle();
            m_headIndex = br.ReadInt16();
            m_neckIndex = br.ReadInt16();
            m_isOn = br.ReadBoolean();
            m_individualLimitsOn = br.ReadBoolean();
            m_isTargetInsideLimitCone = br.ReadBoolean();
            br.ReadByte();
            m_SensingAngle = br.ReadInt16();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_targetWS);
            s.WriteVector4(bw, m_headForwardLS);
            s.WriteVector4(bw, m_neckForwardLS);
            s.WriteVector4(bw, m_neckRightLS);
            s.WriteVector4(bw, m_eyePositionHS);
            bw.WriteSingle(m_newTargetGain);
            bw.WriteSingle(m_onGain);
            bw.WriteSingle(m_offGain);
            bw.WriteSingle(m_limitAngleDegrees);
            bw.WriteSingle(m_limitAngleLeft);
            bw.WriteSingle(m_limitAngleRight);
            bw.WriteSingle(m_limitAngleUp);
            bw.WriteSingle(m_limitAngleDown);
            bw.WriteInt16(m_headIndex);
            bw.WriteInt16(m_neckIndex);
            bw.WriteBoolean(m_isOn);
            bw.WriteBoolean(m_individualLimitsOn);
            bw.WriteBoolean(m_isTargetInsideLimitCone);
            bw.WriteByte(0);
            bw.WriteInt16(m_SensingAngle);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
