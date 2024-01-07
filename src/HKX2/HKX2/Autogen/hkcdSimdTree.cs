using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdSimdTree : hkBaseObject
    {
        public override uint Signature { get => 2227452256; }
        
        public List<hkcdSimdTreeNode> m_nodes;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_nodes = des.ReadClassArray<hkcdSimdTreeNode>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkcdSimdTreeNode>(bw, m_nodes);
        }
    }
}
