using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpVehicleInstanceWheelInfo : IHavokObject
    {
        public virtual uint Signature { get => 529767248; }
        
        public hkContactPoint m_contactPoint;
        public float m_contactFriction;
        public uint m_contactShapeKey;
        public Vector4 m_hardPointWs;
        public Vector4 m_rayEndPointWs;
        public float m_currentSuspensionLength;
        public Vector4 m_suspensionDirectionWs;
        public Vector4 m_spinAxisChassisSpace;
        public Vector4 m_spinAxisWs;
        public Quaternion m_steeringOrientationChassisSpace;
        public float m_spinVelocity;
        public float m_noSlipIdealSpinVelocity;
        public float m_spinAngle;
        public float m_skidEnergyDensity;
        public float m_sideForce;
        public float m_forwardSlipVelocity;
        public float m_sideSlipVelocity;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_contactPoint = new hkContactPoint();
            m_contactPoint.Read(des, br);
            m_contactFriction = br.ReadSingle();
            br.ReadUInt32();
            m_contactShapeKey = br.ReadUInt32();
            br.ReadUInt32();
            m_hardPointWs = des.ReadVector4(br);
            m_rayEndPointWs = des.ReadVector4(br);
            m_currentSuspensionLength = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt32();
            m_suspensionDirectionWs = des.ReadVector4(br);
            m_spinAxisChassisSpace = des.ReadVector4(br);
            m_spinAxisWs = des.ReadVector4(br);
            m_steeringOrientationChassisSpace = des.ReadQuaternion(br);
            m_spinVelocity = br.ReadSingle();
            m_noSlipIdealSpinVelocity = br.ReadSingle();
            m_spinAngle = br.ReadSingle();
            m_skidEnergyDensity = br.ReadSingle();
            m_sideForce = br.ReadSingle();
            m_forwardSlipVelocity = br.ReadSingle();
            m_sideSlipVelocity = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_contactPoint.Write(s, bw);
            bw.WriteSingle(m_contactFriction);
            bw.WriteUInt32(0);
            bw.WriteUInt32(m_contactShapeKey);
            bw.WriteUInt32(0);
            s.WriteVector4(bw, m_hardPointWs);
            s.WriteVector4(bw, m_rayEndPointWs);
            bw.WriteSingle(m_currentSuspensionLength);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            s.WriteVector4(bw, m_suspensionDirectionWs);
            s.WriteVector4(bw, m_spinAxisChassisSpace);
            s.WriteVector4(bw, m_spinAxisWs);
            s.WriteQuaternion(bw, m_steeringOrientationChassisSpace);
            bw.WriteSingle(m_spinVelocity);
            bw.WriteSingle(m_noSlipIdealSpinVelocity);
            bw.WriteSingle(m_spinAngle);
            bw.WriteSingle(m_skidEnergyDensity);
            bw.WriteSingle(m_sideForce);
            bw.WriteSingle(m_forwardSlipVelocity);
            bw.WriteSingle(m_sideSlipVelocity);
            bw.WriteUInt32(0);
        }
    }
}
