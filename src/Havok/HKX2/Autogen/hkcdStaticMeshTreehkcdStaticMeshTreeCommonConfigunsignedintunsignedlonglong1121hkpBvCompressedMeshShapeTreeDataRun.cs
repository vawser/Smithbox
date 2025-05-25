using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticMeshTreehkcdStaticMeshTreeCommonConfigunsignedintunsignedlonglong1121hkpBvCompressedMeshShapeTreeDataRun : hkcdStaticMeshTreeBase
    {
        public override uint Signature { get => 573674918; }
        
        public enum TriangleMaterial
        {
            TM_SET_FROM_TRIANGLE_DATA_TYPE = 0,
            TM_SET_FROM_PRIMITIVE_KEY = 1,
        }
        
        public List<uint> m_packedVertices;
        public List<ulong> m_sharedVertices;
        public List<hkpBvCompressedMeshShapeTreeDataRun> m_primitiveDataRuns;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_packedVertices = des.ReadUInt32Array(br);
            m_sharedVertices = des.ReadUInt64Array(br);
            m_primitiveDataRuns = des.ReadClassArray<hkpBvCompressedMeshShapeTreeDataRun>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteUInt32Array(bw, m_packedVertices);
            s.WriteUInt64Array(bw, m_sharedVertices);
            s.WriteClassArray<hkpBvCompressedMeshShapeTreeDataRun>(bw, m_primitiveDataRuns);
        }
    }
}
