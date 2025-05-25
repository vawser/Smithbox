using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpLimitedForceConstraintMotor : hkpConstraintMotor
    {
        public override uint Signature { get => 278399229; }
        
        public float m_minForce;
        public float m_maxForce;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_minForce = br.ReadSingle();
            m_maxForce = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_minForce);
            bw.WriteSingle(m_maxForce);
        }
    }
}
