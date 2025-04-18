using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpStorageExtendedMeshShape : hkpExtendedMeshShape
    {
        public override uint Signature { get => 3246683740; }
        
        public List<hkpStorageExtendedMeshShapeMeshSubpartStorage> m_meshstorage;
        public List<hkpStorageExtendedMeshShapeShapeSubpartStorage> m_shapestorage;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_meshstorage = des.ReadClassPointerArray<hkpStorageExtendedMeshShapeMeshSubpartStorage>(br);
            m_shapestorage = des.ReadClassPointerArray<hkpStorageExtendedMeshShapeShapeSubpartStorage>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkpStorageExtendedMeshShapeMeshSubpartStorage>(bw, m_meshstorage);
            s.WriteClassPointerArray<hkpStorageExtendedMeshShapeShapeSubpartStorage>(bw, m_shapestorage);
        }
    }
}
