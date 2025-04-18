using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclObjectSpaceSkinPNOperator : hclObjectSpaceSkinOperator
    {
        public override uint Signature { get => 2219346838; }
        
        public List<hclObjectSpaceDeformerLocalBlockPN> m_localPNs;
        public List<hclObjectSpaceDeformerLocalBlockUnpackedPN> m_localUnpackedPNs;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_localPNs = des.ReadClassArray<hclObjectSpaceDeformerLocalBlockPN>(br);
            m_localUnpackedPNs = des.ReadClassArray<hclObjectSpaceDeformerLocalBlockUnpackedPN>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclObjectSpaceDeformerLocalBlockPN>(bw, m_localPNs);
            s.WriteClassArray<hclObjectSpaceDeformerLocalBlockUnpackedPN>(bw, m_localUnpackedPNs);
        }
    }
}
