using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpSerializedDisplayMarker : hkReferencedObject
    {
        public override uint Signature { get => 1278589633; }
        
        public Matrix4x4 m_transform;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_transform = des.ReadTransform(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteTransform(bw, m_transform);
        }
    }
}
