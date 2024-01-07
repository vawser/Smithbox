using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpBallSocketChainDataConstraintInfo : IHavokObject
    {
        public virtual uint Signature { get => 3385585138; }
        
        public Vector4 m_pivotInA;
        public Vector4 m_pivotInB;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_pivotInA = des.ReadVector4(br);
            m_pivotInB = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_pivotInA);
            s.WriteVector4(bw, m_pivotInB);
        }
    }
}
