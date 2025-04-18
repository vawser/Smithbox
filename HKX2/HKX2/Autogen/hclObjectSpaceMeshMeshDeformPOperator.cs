using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclObjectSpaceMeshMeshDeformPOperator : hclObjectSpaceMeshMeshDeformOperator
    {
        public override uint Signature { get => 1464345807; }
        
        public List<hclObjectSpaceDeformerLocalBlockP> m_localPs;
        public List<hclObjectSpaceDeformerLocalBlockUnpackedP> m_localUnpackedPs;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_localPs = des.ReadClassArray<hclObjectSpaceDeformerLocalBlockP>(br);
            m_localUnpackedPs = des.ReadClassArray<hclObjectSpaceDeformerLocalBlockUnpackedP>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclObjectSpaceDeformerLocalBlockP>(bw, m_localPs);
            s.WriteClassArray<hclObjectSpaceDeformerLocalBlockUnpackedP>(bw, m_localUnpackedPs);
        }
    }
}
