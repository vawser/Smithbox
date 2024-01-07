using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpRagdollMotorConstraintAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 2643776556; }
        
        public bool m_isEnabled;
        public Matrix4x4 m_target_bRca;
        public hkpConstraintMotor m_motors_0;
        public hkpConstraintMotor m_motors_1;
        public hkpConstraintMotor m_motors_2;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_isEnabled = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadByte();
            m_target_bRca = des.ReadMatrix3(br);
            m_motors_0 = des.ReadClassPointer<hkpConstraintMotor>(br);
            m_motors_1 = des.ReadClassPointer<hkpConstraintMotor>(br);
            m_motors_2 = des.ReadClassPointer<hkpConstraintMotor>(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_isEnabled);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
            s.WriteMatrix3(bw, m_target_bRca);
            s.WriteClassPointer<hkpConstraintMotor>(bw, m_motors_0);
            s.WriteClassPointer<hkpConstraintMotor>(bw, m_motors_1);
            s.WriteClassPointer<hkpConstraintMotor>(bw, m_motors_2);
            bw.WriteUInt64(0);
        }
    }
}
