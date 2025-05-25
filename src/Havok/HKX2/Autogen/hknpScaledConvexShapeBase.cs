using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpScaledConvexShapeBase : hknpDecoratorShape
    {
        public override uint Signature { get => 487899736; }
        
        public Vector4 m_scale;
        public Vector4 m_translation;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_scale = des.ReadVector4(br);
            m_translation = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_scale);
            s.WriteVector4(bw, m_translation);
        }
    }
}
