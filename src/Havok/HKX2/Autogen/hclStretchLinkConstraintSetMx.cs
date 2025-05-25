using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStretchLinkConstraintSetMx : hclConstraintSet
    {
        public override uint Signature { get => 4278852118; }
        
        public List<hclStretchLinkConstraintSetMxBatch> m_batches;
        public List<hclStretchLinkConstraintSetMxSingle> m_singles;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_batches = des.ReadClassArray<hclStretchLinkConstraintSetMxBatch>(br);
            m_singles = des.ReadClassArray<hclStretchLinkConstraintSetMxSingle>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclStretchLinkConstraintSetMxBatch>(bw, m_batches);
            s.WriteClassArray<hclStretchLinkConstraintSetMxSingle>(bw, m_singles);
        }
    }
}
