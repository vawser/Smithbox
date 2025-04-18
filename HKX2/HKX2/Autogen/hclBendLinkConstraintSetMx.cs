using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBendLinkConstraintSetMx : hclConstraintSet
    {
        public override uint Signature { get => 587950772; }
        
        public List<hclBendLinkConstraintSetMxBatch> m_batches;
        public List<hclBendLinkConstraintSetMxSingle> m_singles;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_batches = des.ReadClassArray<hclBendLinkConstraintSetMxBatch>(br);
            m_singles = des.ReadClassArray<hclBendLinkConstraintSetMxSingle>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclBendLinkConstraintSetMxBatch>(bw, m_batches);
            s.WriteClassArray<hclBendLinkConstraintSetMxSingle>(bw, m_singles);
        }
    }
}
