using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclCapsuleShape : hclShape
    {
        public override uint Signature { get => 3708024100; }
        
        public Vector4 m_start;
        public Vector4 m_end;
        public Vector4 m_dir;
        public float m_radius;
        public float m_capLenSqrdInv;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_start = des.ReadVector4(br);
            m_end = des.ReadVector4(br);
            m_dir = des.ReadVector4(br);
            m_radius = br.ReadSingle();
            m_capLenSqrdInv = br.ReadSingle();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_start);
            s.WriteVector4(bw, m_end);
            s.WriteVector4(bw, m_dir);
            bw.WriteSingle(m_radius);
            bw.WriteSingle(m_capLenSqrdInv);
            bw.WriteUInt64(0);
        }
    }
}
