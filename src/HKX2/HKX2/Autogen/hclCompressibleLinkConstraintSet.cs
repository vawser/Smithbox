using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclCompressibleLinkConstraintSet : hclConstraintSet
    {
        public override uint Signature { get => 1374147701; }
        
        public List<hclCompressibleLinkConstraintSetLink> m_links;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_links = des.ReadClassArray<hclCompressibleLinkConstraintSetLink>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclCompressibleLinkConstraintSetLink>(bw, m_links);
        }
    }
}
