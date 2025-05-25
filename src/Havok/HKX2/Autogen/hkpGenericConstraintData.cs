using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpGenericConstraintData : hkpConstraintData
    {
        public override uint Signature { get => 3729029518; }
        
        public hkpBridgeAtoms m_atoms;
        public hkpGenericConstraintDataScheme m_scheme;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_atoms = new hkpBridgeAtoms();
            m_atoms.Read(des, br);
            m_scheme = new hkpGenericConstraintDataScheme();
            m_scheme.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            m_atoms.Write(s, bw);
            m_scheme.Write(s, bw);
        }
    }
}
