using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticMeshTreehkcdStaticMeshTreeCommonConfigunsignedintunsignedlonglong1121hknpCompressedMeshShapeTreeDataRun : hkcdStaticMeshTreeBase
    {
        public override uint Signature { get => 497573378; }
        
        public enum TriangleMaterial
        {
            TM_SET_FROM_TRIANGLE_DATA_TYPE = 0,
            TM_SET_FROM_PRIMITIVE_KEY = 1,
        }
        
        public List<uint> m_packedVertices;
        public List<ulong> m_sharedVertices;
        public List<hknpCompressedMeshShapeTreeDataRun> m_primitiveDataRuns;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_packedVertices = des.ReadUInt32Array(br);
            m_sharedVertices = des.ReadUInt64Array(br);
            m_primitiveDataRuns = des.ReadClassArray<hknpCompressedMeshShapeTreeDataRun>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteUInt32Array(bw, m_packedVertices);
            s.WriteUInt64Array(bw, m_sharedVertices);
            s.WriteClassArray<hknpCompressedMeshShapeTreeDataRun>(bw, m_primitiveDataRuns);
        }
    }
}
