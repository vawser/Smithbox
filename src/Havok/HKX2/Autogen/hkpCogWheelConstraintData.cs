using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpCogWheelConstraintData : hkpConstraintData
    {
        public override uint Signature { get => 18785234; }
        
        public hkpCogWheelConstraintDataAtoms m_atoms;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_atoms = new hkpCogWheelConstraintDataAtoms();
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
