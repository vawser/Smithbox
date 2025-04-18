using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBoneSpaceSkinPNOperator : hclBoneSpaceSkinOperator
    {
        public override uint Signature { get => 2110964385; }
        
        public List<hclBoneSpaceDeformerLocalBlockPN> m_localPNs;
        public List<hclBoneSpaceDeformerLocalBlockUnpackedPN> m_localUnpackedPNs;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_localPNs = des.ReadClassArray<hclBoneSpaceDeformerLocalBlockPN>(br);
            m_localUnpackedPNs = des.ReadClassArray<hclBoneSpaceDeformerLocalBlockUnpackedPN>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclBoneSpaceDeformerLocalBlockPN>(bw, m_localPNs);
            s.WriteClassArray<hclBoneSpaceDeformerLocalBlockUnpackedPN>(bw, m_localUnpackedPNs);
        }
    }
}
