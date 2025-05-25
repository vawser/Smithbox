using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticTreeCodecRaw : IHavokObject
    {
        public virtual uint Signature { get => 761653748; }
        
        public hkAabb m_aabb;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_aabb = new hkAabb();
            m_aabb.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_aabb.Write(s, bw);
        }
    }
}
