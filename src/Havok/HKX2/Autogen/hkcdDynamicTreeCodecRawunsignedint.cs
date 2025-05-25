using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdDynamicTreeCodecRawunsignedint : IHavokObject
    {
        public virtual uint Signature { get => 645676932; }
        
        public hkAabb m_aabb;
        public uint m_parent;
        public uint m_children_0;
        public uint m_children_1;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_aabb = new hkAabb();
            m_aabb.Read(des, br);
            m_parent = br.ReadUInt32();
            m_children_0 = br.ReadUInt32();
            m_children_1 = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_aabb.Write(s, bw);
            bw.WriteUInt32(m_parent);
            bw.WriteUInt32(m_children_0);
            bw.WriteUInt32(m_children_1);
            bw.WriteUInt32(0);
        }
    }
}
