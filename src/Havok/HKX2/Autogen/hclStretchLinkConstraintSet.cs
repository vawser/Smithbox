using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStretchLinkConstraintSet : hclConstraintSet
    {
        public override uint Signature { get => 1114321748; }
        
        public List<hclStretchLinkConstraintSetLink> m_links;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_links = des.ReadClassArray<hclStretchLinkConstraintSetLink>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclStretchLinkConstraintSetLink>(bw, m_links);
        }
    }
}
