using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpWrappedConstraintData : hkpConstraintData
    {
        public override uint Signature { get => 1519608732; }
        
        public hkpConstraintData m_constraintData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_constraintData = des.ReadClassPointer<hkpConstraintData>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkpConstraintData>(bw, m_constraintData);
        }
    }
}
