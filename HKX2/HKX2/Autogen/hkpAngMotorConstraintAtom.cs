using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpAngMotorConstraintAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 1112114262; }
        
        public bool m_isEnabled;
        public byte m_motorAxis;
        public float m_targetAngle;
        public hkpConstraintMotor m_motor;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_isEnabled = br.ReadBoolean();
            m_motorAxis = br.ReadByte();
            br.ReadUInt64();
            m_targetAngle = br.ReadSingle();
            m_motor = des.ReadClassPointer<hkpConstraintMotor>(br);
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_isEnabled);
            bw.WriteByte(m_motorAxis);
            bw.WriteUInt64(0);
            bw.WriteSingle(m_targetAngle);
            s.WriteClassPointer<hkpConstraintMotor>(bw, m_motor);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
