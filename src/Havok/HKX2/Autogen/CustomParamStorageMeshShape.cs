using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class CustomParamStorageMeshShape : hkpStorageMeshShape
    {
        public override uint Signature { get => 1536200629; }
        
        public List<CustomMeshParameter> m_materialArray;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_materialArray = des.ReadClassPointerArray<CustomMeshParameter>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<CustomMeshParameter>(bw, m_materialArray);
        }
    }
}
