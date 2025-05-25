using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum RayHitType
    {
        HIT_CAP0 = 0,
        HIT_CAP1 = 1,
        HIT_BODY = 2,
    }
    
    public partial class hkpCapsuleShape : hkpConvexShape
    {
        public override uint Signature { get => 4255218163; }
        
        public Vector4 m_vertexA;
        public Vector4 m_vertexB;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_vertexA = des.ReadVector4(br);
            m_vertexB = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_vertexA);
            s.WriteVector4(bw, m_vertexB);
        }
    }
}
