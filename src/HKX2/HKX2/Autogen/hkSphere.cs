using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkSphere : IHavokObject
    {
        public virtual uint Signature { get => 339607449; }
        
        public Vector4 m_pos;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_pos = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_pos);
        }
    }
}
