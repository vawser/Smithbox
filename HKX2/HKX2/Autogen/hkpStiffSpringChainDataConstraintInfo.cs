using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpStiffSpringChainDataConstraintInfo : IHavokObject
    {
        public virtual uint Signature { get => 3324289408; }
        
        public Vector4 m_pivotInA;
        public Vector4 m_pivotInB;
        public float m_springLength;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_pivotInA = des.ReadVector4(br);
            m_pivotInB = des.ReadVector4(br);
            m_springLength = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_pivotInA);
            s.WriteVector4(bw, m_pivotInB);
            bw.WriteSingle(m_springLength);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
