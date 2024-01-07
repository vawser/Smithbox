using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpPointToPlaneConstraintDataAtoms : IHavokObject
    {
        public virtual uint Signature { get => 2468667051; }
        
        public hkpSetLocalTransformsConstraintAtom m_transforms;
        public hkpLinConstraintAtom m_lin;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transforms = new hkpSetLocalTransformsConstraintAtom();
            m_transforms.Read(des, br);
            m_lin = new hkpLinConstraintAtom();
            m_lin.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_transforms.Write(s, bw);
            m_lin.Write(s, bw);
        }
    }
}
