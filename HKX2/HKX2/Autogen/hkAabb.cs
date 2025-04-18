using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkAabb : IHavokObject
    {
        public virtual uint Signature { get => 1251248918; }
        
        public Vector4 m_min;
        public Vector4 m_max;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_min = des.ReadVector4(br);
            m_max = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_min);
            s.WriteVector4(bw, m_max);
        }
    }
}
