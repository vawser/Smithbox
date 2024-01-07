using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum RebuildFlagsEnum
    {
        REBUILD_USE_DOUBLE_BUFFERING = 1,
        REBUILD_REFIT_ONLY = 2,
    }
    
    public partial class hknpExternMeshShape : hknpCompositeShape
    {
        public override uint Signature { get => 1146675861; }
        
        public hknpExternMeshShapeGeometry m_geometry;
        public hknpExternMeshShapeData m_boundingVolumeData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_geometry = des.ReadClassPointer<hknpExternMeshShapeGeometry>(br);
            m_boundingVolumeData = des.ReadClassPointer<hknpExternMeshShapeData>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hknpExternMeshShapeGeometry>(bw, m_geometry);
            s.WriteClassPointer<hknpExternMeshShapeData>(bw, m_boundingVolumeData);
        }
    }
}
