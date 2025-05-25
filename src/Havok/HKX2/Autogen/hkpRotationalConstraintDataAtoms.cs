using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpRotationalConstraintDataAtoms : IHavokObject
    {
        public virtual uint Signature { get => 2749345; }
        
        public hkpSetLocalRotationsConstraintAtom m_rotations;
        public hkpAngConstraintAtom m_ang;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_rotations = new hkpSetLocalRotationsConstraintAtom();
            m_rotations.Read(des, br);
            m_ang = new hkpAngConstraintAtom();
            m_ang.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_rotations.Write(s, bw);
            m_ang.Write(s, bw);
        }
    }
}
