using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStandardLinkConstraintSet : hclConstraintSet
    {
        public override uint Signature { get => 1114321748; }
        
        public List<hclStandardLinkConstraintSetLink> m_links;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_links = des.ReadClassArray<hclStandardLinkConstraintSetLink>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclStandardLinkConstraintSetLink>(bw, m_links);
        }
    }
}
