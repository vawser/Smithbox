using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdPlanarSolidNode : IHavokObject
    {
        public virtual uint Signature { get => 4005511420; }
        
        public uint m_parent;
        public uint m_left;
        public uint m_right;
        public uint m_nextFreeNodeId;
        public uint m_planeId;
        public uint m_data;
        public uint m_typeAndFlags;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_parent = br.ReadUInt32();
            m_left = br.ReadUInt32();
            m_right = br.ReadUInt32();
            m_nextFreeNodeId = br.ReadUInt32();
            m_planeId = br.ReadUInt32();
            m_data = br.ReadUInt32();
            m_typeAndFlags = br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_parent);
            bw.WriteUInt32(m_left);
            bw.WriteUInt32(m_right);
            bw.WriteUInt32(m_nextFreeNodeId);
            bw.WriteUInt32(m_planeId);
            bw.WriteUInt32(m_data);
            bw.WriteUInt32(m_typeAndFlags);
        }
    }
}
