using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclObjectSpaceMeshMeshDeformPNTOperator : hclObjectSpaceMeshMeshDeformOperator
    {
        public override uint Signature { get => 3792457022; }
        
        public List<hclObjectSpaceDeformerLocalBlockPNT> m_localPNTs;
        public List<hclObjectSpaceDeformerLocalBlockUnpackedPNT> m_localUnpackedPNTs;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_localPNTs = des.ReadClassArray<hclObjectSpaceDeformerLocalBlockPNT>(br);
            m_localUnpackedPNTs = des.ReadClassArray<hclObjectSpaceDeformerLocalBlockUnpackedPNT>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclObjectSpaceDeformerLocalBlockPNT>(bw, m_localPNTs);
            s.WriteClassArray<hclObjectSpaceDeformerLocalBlockUnpackedPNT>(bw, m_localUnpackedPNTs);
        }
    }
}
