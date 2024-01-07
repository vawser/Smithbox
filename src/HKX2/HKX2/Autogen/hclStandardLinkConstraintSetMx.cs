using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStandardLinkConstraintSetMx : hclConstraintSet
    {
        public override uint Signature { get => 629162713; }
        
        public List<hclStandardLinkConstraintSetMxBatch> m_batches;
        public List<hclStandardLinkConstraintSetMxSingle> m_singles;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_batches = des.ReadClassArray<hclStandardLinkConstraintSetMxBatch>(br);
            m_singles = des.ReadClassArray<hclStandardLinkConstraintSetMxSingle>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclStandardLinkConstraintSetMxBatch>(bw, m_batches);
            s.WriteClassArray<hclStandardLinkConstraintSetMxSingle>(bw, m_singles);
        }
    }
}
