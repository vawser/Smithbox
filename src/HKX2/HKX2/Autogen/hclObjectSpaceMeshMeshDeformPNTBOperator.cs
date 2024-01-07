using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclObjectSpaceMeshMeshDeformPNTBOperator : hclObjectSpaceMeshMeshDeformOperator
    {
        public override uint Signature { get => 2213112367; }
        
        public List<hclObjectSpaceDeformerLocalBlockPNTB> m_localPNTBs;
        public List<hclObjectSpaceDeformerLocalBlockUnpackedPNTB> m_localUnpackedPNTBs;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_localPNTBs = des.ReadClassArray<hclObjectSpaceDeformerLocalBlockPNTB>(br);
            m_localUnpackedPNTBs = des.ReadClassArray<hclObjectSpaceDeformerLocalBlockUnpackedPNTB>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclObjectSpaceDeformerLocalBlockPNTB>(bw, m_localPNTBs);
            s.WriteClassArray<hclObjectSpaceDeformerLocalBlockUnpackedPNTB>(bw, m_localUnpackedPNTBs);
        }
    }
}
