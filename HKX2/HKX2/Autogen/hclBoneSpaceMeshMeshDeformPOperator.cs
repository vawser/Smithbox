using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBoneSpaceMeshMeshDeformPOperator : hclBoneSpaceMeshMeshDeformOperator
    {
        public override uint Signature { get => 3375439037; }
        
        public List<hclBoneSpaceDeformerLocalBlockP> m_localPs;
        public List<hclBoneSpaceDeformerLocalBlockUnpackedP> m_localUnpackedPs;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_localPs = des.ReadClassArray<hclBoneSpaceDeformerLocalBlockP>(br);
            m_localUnpackedPs = des.ReadClassArray<hclBoneSpaceDeformerLocalBlockUnpackedP>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclBoneSpaceDeformerLocalBlockP>(bw, m_localPs);
            s.WriteClassArray<hclBoneSpaceDeformerLocalBlockUnpackedP>(bw, m_localUnpackedPs);
        }
    }
}
