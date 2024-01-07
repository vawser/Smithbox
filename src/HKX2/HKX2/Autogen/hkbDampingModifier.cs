using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbDampingModifier : hkbModifier
    {
        public override uint Signature { get => 1755651333; }
        
        public float m_kP;
        public float m_kI;
        public float m_kD;
        public bool m_enableScalarDamping;
        public bool m_enableVectorDamping;
        public float m_rawValue;
        public float m_dampedValue;
        public Vector4 m_rawVector;
        public Vector4 m_dampedVector;
        public Vector4 m_vecErrorSum;
        public Vector4 m_vecPreviousError;
        public float m_errorSum;
        public float m_previousError;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_kP = br.ReadSingle();
            m_kI = br.ReadSingle();
            m_kD = br.ReadSingle();
            m_enableScalarDamping = br.ReadBoolean();
            m_enableVectorDamping = br.ReadBoolean();
            br.ReadUInt16();
            m_rawValue = br.ReadSingle();
            m_dampedValue = br.ReadSingle();
            m_rawVector = des.ReadVector4(br);
            m_dampedVector = des.ReadVector4(br);
            m_vecErrorSum = des.ReadVector4(br);
            m_vecPreviousError = des.ReadVector4(br);
            m_errorSum = br.ReadSingle();
            m_previousError = br.ReadSingle();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_kP);
            bw.WriteSingle(m_kI);
            bw.WriteSingle(m_kD);
            bw.WriteBoolean(m_enableScalarDamping);
            bw.WriteBoolean(m_enableVectorDamping);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_rawValue);
            bw.WriteSingle(m_dampedValue);
            s.WriteVector4(bw, m_rawVector);
            s.WriteVector4(bw, m_dampedVector);
            s.WriteVector4(bw, m_vecErrorSum);
            s.WriteVector4(bw, m_vecPreviousError);
            bw.WriteSingle(m_errorSum);
            bw.WriteSingle(m_previousError);
            bw.WriteUInt64(0);
        }
    }
}
