using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpConvexTransformShape : hkpConvexTransformShapeBase
    {
        public override uint Signature { get => 988457571; }
        
        public Matrix4x4 m_transform;
        public Vector4 m_extraScale;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_transform = des.ReadQSTransform(br);
            m_extraScale = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteQSTransform(bw, m_transform);
            s.WriteVector4(bw, m_extraScale);
        }
    }
}
