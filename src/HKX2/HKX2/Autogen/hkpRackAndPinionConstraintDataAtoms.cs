using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpRackAndPinionConstraintDataAtoms : IHavokObject
    {
        public virtual uint Signature { get => 26925652; }
        
        public hkpSetLocalTransformsConstraintAtom m_transforms;
        public hkpRackAndPinionConstraintAtom m_rackAndPinion;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transforms = new hkpSetLocalTransformsConstraintAtom();
            m_transforms.Read(des, br);
            m_rackAndPinion = new hkpRackAndPinionConstraintAtom();
            m_rackAndPinion.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_transforms.Write(s, bw);
            m_rackAndPinion.Write(s, bw);
        }
    }
}
