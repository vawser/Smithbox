using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpSpringDamperConstraintMotor : hkpLimitedForceConstraintMotor
    {
        public override uint Signature { get => 111867549; }
        
        public float m_springConstant;
        public float m_springDamping;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_springConstant = br.ReadSingle();
            m_springDamping = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_springConstant);
            bw.WriteSingle(m_springDamping);
        }
    }
}
