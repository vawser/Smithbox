using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpSoftContactModifierConstraintAtom : hkpModifierConstraintAtom
    {
        public override uint Signature { get => 3775335334; }
        
        public float m_tau;
        public float m_maxAcceleration;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_tau = br.ReadSingle();
            m_maxAcceleration = br.ReadSingle();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_tau);
            bw.WriteSingle(m_maxAcceleration);
            bw.WriteUInt64(0);
        }
    }
}
