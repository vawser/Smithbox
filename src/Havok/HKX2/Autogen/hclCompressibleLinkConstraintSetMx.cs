using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclCompressibleLinkConstraintSetMx : hclConstraintSet
    {
        public override uint Signature { get => 1119028181; }
        
        public List<hclCompressibleLinkConstraintSetMxBatch> m_batches;
        public List<hclCompressibleLinkConstraintSetMxSingle> m_singles;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_batches = des.ReadClassArray<hclCompressibleLinkConstraintSetMxBatch>(br);
            m_singles = des.ReadClassArray<hclCompressibleLinkConstraintSetMxSingle>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclCompressibleLinkConstraintSetMxBatch>(bw, m_batches);
            s.WriteClassArray<hclCompressibleLinkConstraintSetMxSingle>(bw, m_singles);
        }
    }
}
