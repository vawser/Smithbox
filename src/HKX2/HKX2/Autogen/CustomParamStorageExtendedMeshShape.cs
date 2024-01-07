using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class CustomParamStorageExtendedMeshShape : hkpStorageExtendedMeshShape
    {
        public override uint Signature { get => 375778053; }
        
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
