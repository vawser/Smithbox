using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum MotorIndex
    {
        MOTOR_TWIST = 0,
        MOTOR_PLANE = 1,
        MOTOR_CONE = 2,
    }
    
    public partial class hkpRagdollConstraintData : hkpConstraintData
    {
        public override uint Signature { get => 3078430774; }
        
        public hkpRagdollConstraintDataAtoms m_atoms;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_atoms = new hkpRagdollConstraintDataAtoms();
            m_atoms.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            m_atoms.Write(s, bw);
        }
    }
}
