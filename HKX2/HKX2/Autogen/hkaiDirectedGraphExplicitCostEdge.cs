using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiDirectedGraphExplicitCostEdge : IHavokObject
    {
        public virtual uint Signature { get => 1476261009; }
        
        public short m_cost;
        public ushort m_flags;
        public uint m_target;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_cost = br.ReadInt16();
            m_flags = br.ReadUInt16();
            m_target = br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt16(m_cost);
            bw.WriteUInt16(m_flags);
            bw.WriteUInt32(m_target);
        }
    }
}
