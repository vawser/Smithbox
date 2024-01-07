using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpPoweredChainDataConstraintInfo : IHavokObject
    {
        public virtual uint Signature { get => 4169854501; }
        
        public Vector4 m_pivotInA;
        public Vector4 m_pivotInB;
        public Quaternion m_aTc;
        public Quaternion m_bTc;
        public hkpConstraintMotor m_motors_0;
        public hkpConstraintMotor m_motors_1;
        public hkpConstraintMotor m_motors_2;
        public bool m_switchBodies;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_pivotInA = des.ReadVector4(br);
            m_pivotInB = des.ReadVector4(br);
            m_aTc = des.ReadQuaternion(br);
            m_bTc = des.ReadQuaternion(br);
            m_motors_0 = des.ReadClassPointer<hkpConstraintMotor>(br);
            m_motors_1 = des.ReadClassPointer<hkpConstraintMotor>(br);
            m_motors_2 = des.ReadClassPointer<hkpConstraintMotor>(br);
            m_switchBodies = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_pivotInA);
            s.WriteVector4(bw, m_pivotInB);
            s.WriteQuaternion(bw, m_aTc);
            s.WriteQuaternion(bw, m_bTc);
            s.WriteClassPointer<hkpConstraintMotor>(bw, m_motors_0);
            s.WriteClassPointer<hkpConstraintMotor>(bw, m_motors_1);
            s.WriteClassPointer<hkpConstraintMotor>(bw, m_motors_2);
            bw.WriteBoolean(m_switchBodies);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
