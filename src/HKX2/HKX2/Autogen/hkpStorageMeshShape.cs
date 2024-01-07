using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpStorageMeshShape : hkpMeshShape
    {
        public override uint Signature { get => 1057590271; }
        
        public List<hkpStorageMeshShapeSubpartStorage> m_storage;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_storage = des.ReadClassPointerArray<hkpStorageMeshShapeSubpartStorage>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkpStorageMeshShapeSubpartStorage>(bw, m_storage);
        }
    }
}
