using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum MultiRotationAxisType
    {
        AxisXY = 0,
        AxisYX = 1,
    }
    
    public enum RotationAxisType
    {
        AxisX = 0,
        AxisY = 1,
    }
    
    public partial class CustomLookAtTwistModifier : hkbModifier
    {
        public override uint Signature { get => 4031613631; }
        
        public enum SetAngleMethod
        {
            LINEAR = 0,
            RAMPED = 1,
        }
        
        public enum RotationAxisCoordinates
        {
            ROTATION_AXIS_IN_MODEL_COORDINATES = 0,
            ROTATION_AXIS_IN_LOCAL_COORDINATES = 1,
        }
        
        public enum GainState
        {
            GainStateTargetGain = 0,
            GainStateOn = 1,
            GainStateOff = 2,
        }
        
        public int m_ModifierID;
        public MultiRotationAxisType m_rotationAxisType;
        public int m_SensingDummyPoly;
        public List<CustomLookAtTwistModifierTwistParam> m_twistParam;
        public float m_UpLimitAngle;
        public float m_DownLimitAngle;
        public float m_RightLimitAngle;
        public float m_LeftLimitAngle;
        public float m_UpMinimumAngle;
        public float m_DownMinimumAngle;
        public float m_RightMinimumAngle;
        public float m_LeftMinimumAngle;
        public short m_SensingAngle;
        public SetAngleMethod m_setAngleMethod;
        public bool m_isAdditive;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_ModifierID = br.ReadInt32();
            m_rotationAxisType = (MultiRotationAxisType)br.ReadSByte();
            br.ReadUInt16();
            br.ReadByte();
            m_SensingDummyPoly = br.ReadInt32();
            br.ReadUInt32();
            m_twistParam = des.ReadClassArray<CustomLookAtTwistModifierTwistParam>(br);
            m_UpLimitAngle = br.ReadSingle();
            m_DownLimitAngle = br.ReadSingle();
            m_RightLimitAngle = br.ReadSingle();
            m_LeftLimitAngle = br.ReadSingle();
            m_UpMinimumAngle = br.ReadSingle();
            m_DownMinimumAngle = br.ReadSingle();
            m_RightMinimumAngle = br.ReadSingle();
            m_LeftMinimumAngle = br.ReadSingle();
            m_SensingAngle = br.ReadInt16();
            m_setAngleMethod = (SetAngleMethod)br.ReadSByte();
            m_isAdditive = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt32(m_ModifierID);
            bw.WriteSByte((sbyte)m_rotationAxisType);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_SensingDummyPoly);
            bw.WriteUInt32(0);
            s.WriteClassArray<CustomLookAtTwistModifierTwistParam>(bw, m_twistParam);
            bw.WriteSingle(m_UpLimitAngle);
            bw.WriteSingle(m_DownLimitAngle);
            bw.WriteSingle(m_RightLimitAngle);
            bw.WriteSingle(m_LeftLimitAngle);
            bw.WriteSingle(m_UpMinimumAngle);
            bw.WriteSingle(m_DownMinimumAngle);
            bw.WriteSingle(m_RightMinimumAngle);
            bw.WriteSingle(m_LeftMinimumAngle);
            bw.WriteInt16(m_SensingAngle);
            bw.WriteSByte((sbyte)m_setAngleMethod);
            bw.WriteBoolean(m_isAdditive);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
