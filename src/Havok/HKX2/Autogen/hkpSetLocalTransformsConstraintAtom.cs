using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpSetLocalTransformsConstraintAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 332208161; }
        
        public Matrix4x4 m_transformA;
        public Matrix4x4 m_transformB;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            m_transformA = des.ReadTransform(br);
            m_transformB = des.ReadTransform(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            s.WriteTransform(bw, m_transformA);
            s.WriteTransform(bw, m_transformB);
        }
    }
}
