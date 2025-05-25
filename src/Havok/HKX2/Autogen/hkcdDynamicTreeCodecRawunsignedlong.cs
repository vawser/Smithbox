using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdDynamicTreeCodecRawunsignedlong : IHavokObject
    {
        public virtual uint Signature { get => 3686443014; }
        
        public hkAabb m_aabb;
        public ulong m_parent;
        public ulong m_children_0;
        public ulong m_children_1;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_aabb = new hkAabb();
            m_aabb.Read(des, br);
            m_parent = br.ReadUInt64();
            m_children_0 = br.ReadUInt64();
            m_children_1 = br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_aabb.Write(s, bw);
            bw.WriteUInt64(m_parent);
            bw.WriteUInt64(m_children_0);
            bw.WriteUInt64(m_children_1);
            bw.WriteUInt64(0);
        }
    }
}
