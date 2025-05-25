using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpBreakableConstraintData : hkpWrappedConstraintData
    {
        public override uint Signature { get => 3288630727; }
        
        public float m_threshold;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_threshold = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_threshold);
            bw.WriteUInt32(0);
        }
    }
}
