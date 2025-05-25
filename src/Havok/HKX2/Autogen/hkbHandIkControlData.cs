using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbHandIkControlData : IHavokObject
    {
        public virtual uint Signature { get => 3609955607; }
        
        public enum HandleChangeMode
        {
            HANDLE_CHANGE_MODE_ABRUPT = 0,
            HANDLE_CHANGE_MODE_CONSTANT_VELOCITY = 1,
        }
        
        public Vector4 m_targetPosition;
        public Quaternion m_targetRotation;
        public Vector4 m_targetNormal;
        public hkbHandle m_targetHandle;
        public float m_transformOnFraction;
        public float m_normalOnFraction;
        public float m_fadeInDuration;
        public float m_fadeOutDuration;
        public float m_extrapolationTimeStep;
        public float m_handleChangeSpeed;
        public HandleChangeMode m_handleChangeMode;
        public bool m_fixUp;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_targetPosition = des.ReadVector4(br);
            m_targetRotation = des.ReadQuaternion(br);
            m_targetNormal = des.ReadVector4(br);
            m_targetHandle = des.ReadClassPointer<hkbHandle>(br);
            m_transformOnFraction = br.ReadSingle();
            m_normalOnFraction = br.ReadSingle();
            m_fadeInDuration = br.ReadSingle();
            m_fadeOutDuration = br.ReadSingle();
            m_extrapolationTimeStep = br.ReadSingle();
            m_handleChangeSpeed = br.ReadSingle();
            m_handleChangeMode = (HandleChangeMode)br.ReadSByte();
            m_fixUp = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_targetPosition);
            s.WriteQuaternion(bw, m_targetRotation);
            s.WriteVector4(bw, m_targetNormal);
            s.WriteClassPointer<hkbHandle>(bw, m_targetHandle);
            bw.WriteSingle(m_transformOnFraction);
            bw.WriteSingle(m_normalOnFraction);
            bw.WriteSingle(m_fadeInDuration);
            bw.WriteSingle(m_fadeOutDuration);
            bw.WriteSingle(m_extrapolationTimeStep);
            bw.WriteSingle(m_handleChangeSpeed);
            bw.WriteSByte((sbyte)m_handleChangeMode);
            bw.WriteBoolean(m_fixUp);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
