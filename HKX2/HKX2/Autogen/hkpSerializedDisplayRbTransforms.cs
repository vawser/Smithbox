using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpSerializedDisplayRbTransforms : hkReferencedObject
    {
        public override uint Signature { get => 3318981991; }
        
        public List<hkpSerializedDisplayRbTransformsDisplayTransformPair> m_transforms;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_transforms = des.ReadClassArray<hkpSerializedDisplayRbTransformsDisplayTransformPair>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkpSerializedDisplayRbTransformsDisplayTransformPair>(bw, m_transforms);
        }
    }
}
