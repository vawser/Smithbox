using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpCogWheelConstraintDataAtoms : IHavokObject
    {
        public virtual uint Signature { get => 1533852857; }
        
        public hkpSetLocalTransformsConstraintAtom m_transforms;
        public hkpCogWheelConstraintAtom m_cogWheels;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transforms = new hkpSetLocalTransformsConstraintAtom();
            m_transforms.Read(des, br);
            m_cogWheels = new hkpCogWheelConstraintAtom();
            m_cogWheels.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_transforms.Write(s, bw);
            m_cogWheels.Write(s, bw);
        }
    }
}
