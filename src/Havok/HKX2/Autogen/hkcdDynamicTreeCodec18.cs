using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdDynamicTreeCodec18 : IHavokObject
    {
        public virtual uint Signature { get => 1888002393; }
        
        public hkAabbHalf m_aabb;
        public ushort m_parent;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_aabb = new hkAabbHalf();
            m_aabb.Read(des, br);
            m_parent = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_aabb.Write(s, bw);
            bw.WriteUInt16(m_parent);
        }
    }
}
