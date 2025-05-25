using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBendLinkConstraintSet : hclConstraintSet
    {
        public override uint Signature { get => 646072151; }
        
        public List<hclBendLinkConstraintSetLink> m_links;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_links = des.ReadClassArray<hclBendLinkConstraintSetLink>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclBendLinkConstraintSetLink>(bw, m_links);
        }
    }
}
