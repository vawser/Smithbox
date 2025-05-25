using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpMovingSurfaceModifierConstraintAtom : hkpModifierConstraintAtom
    {
        public override uint Signature { get => 3616911747; }
        
        public Vector4 m_velocity;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_velocity = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_velocity);
        }
    }
}
