using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxNodeSelectionSet : hkxAttributeHolder
    {
        public override uint Signature { get => 2568320674; }
        
        public List<hkxNode> m_selectedNodes;
        public string m_name;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_selectedNodes = des.ReadClassPointerArray<hkxNode>(br);
            m_name = des.ReadStringPointer(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkxNode>(bw, m_selectedNodes);
            s.WriteStringPointer(bw, m_name);
        }
    }
}
