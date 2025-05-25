using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpHingeConstraintData : hkpConstraintData
    {
        public override uint Signature { get => 2417647610; }
        
        public hkpHingeConstraintDataAtoms m_atoms;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_atoms = new hkpHingeConstraintDataAtoms();
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
