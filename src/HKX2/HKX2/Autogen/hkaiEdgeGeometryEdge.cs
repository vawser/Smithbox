using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiEdgeGeometryEdge : IHavokObject
    {
        public virtual uint Signature { get => 365804318; }
        
        public uint m_a;
        public uint m_b;
        public uint m_face;
        public uint m_data;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_a = br.ReadUInt32();
            m_b = br.ReadUInt32();
            m_face = br.ReadUInt32();
            m_data = br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_a);
            bw.WriteUInt32(m_b);
            bw.WriteUInt32(m_face);
            bw.WriteUInt32(m_data);
        }
    }
}
