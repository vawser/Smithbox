using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBoneSpaceSkinPNTOperator : hclBoneSpaceSkinOperator
    {
        public override uint Signature { get => 2379233727; }
        
        public List<hclBoneSpaceDeformerLocalBlockPNT> m_localPNTs;
        public List<hclBoneSpaceDeformerLocalBlockUnpackedPNT> m_localUnpackedPNTs;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_localPNTs = des.ReadClassArray<hclBoneSpaceDeformerLocalBlockPNT>(br);
            m_localUnpackedPNTs = des.ReadClassArray<hclBoneSpaceDeformerLocalBlockUnpackedPNT>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclBoneSpaceDeformerLocalBlockPNT>(bw, m_localPNTs);
            s.WriteClassArray<hclBoneSpaceDeformerLocalBlockUnpackedPNT>(bw, m_localUnpackedPNTs);
        }
    }
}
