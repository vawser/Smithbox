using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkContactPoint : IHavokObject
    {
        public virtual uint Signature { get => 2446843278; }
        
        public Vector4 m_position;
        public Vector4 m_separatingNormal;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_position = des.ReadVector4(br);
            m_separatingNormal = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_position);
            s.WriteVector4(bw, m_separatingNormal);
        }
    }
}
