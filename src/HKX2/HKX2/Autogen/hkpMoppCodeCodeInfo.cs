using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpMoppCodeCodeInfo : IHavokObject
    {
        public virtual uint Signature { get => 3640507144; }
        
        public Vector4 m_offset;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_offset = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_offset);
        }
    }
}
