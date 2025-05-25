using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpCenterOfMassChangerModifierConstraintAtom : hkpModifierConstraintAtom
    {
        public override uint Signature { get => 2146951992; }
        
        public Vector4 m_displacementA;
        public Vector4 m_displacementB;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_displacementA = des.ReadVector4(br);
            m_displacementB = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_displacementA);
            s.WriteVector4(bw, m_displacementB);
        }
    }
}
