using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavMeshEdge : IHavokObject
    {
        public virtual uint Signature { get => 2177874386; }
        
        public int m_a;
        public int m_b;
        public uint m_oppositeEdge;
        public uint m_oppositeFace;
        public byte m_flags;
        public short m_userEdgeCost;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_a = br.ReadInt32();
            m_b = br.ReadInt32();
            m_oppositeEdge = br.ReadUInt32();
            m_oppositeFace = br.ReadUInt32();
            m_flags = br.ReadByte();
            br.ReadByte();
            m_userEdgeCost = br.ReadInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_a);
            bw.WriteInt32(m_b);
            bw.WriteUInt32(m_oppositeEdge);
            bw.WriteUInt32(m_oppositeFace);
            bw.WriteByte(m_flags);
            bw.WriteByte(0);
            bw.WriteInt16(m_userEdgeCost);
        }
    }
}
