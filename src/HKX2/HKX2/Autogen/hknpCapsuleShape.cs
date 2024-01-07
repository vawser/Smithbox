using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpCapsuleShape : hknpConvexPolytopeShape
    {
        public override uint Signature { get => 1621581644; }
        
        public Vector4 m_a;
        public Vector4 m_b;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_a = des.ReadVector4(br);
            m_b = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_a);
            s.WriteVector4(bw, m_b);
        }
    }
}
