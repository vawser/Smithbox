using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiStreamingSetGraphConnection : IHavokObject
    {
        public virtual uint Signature { get => 1370206861; }
        
        public int m_nodeIndex;
        public int m_oppositeNodeIndex;
        public uint m_edgeData;
        public short m_edgeCost;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_nodeIndex = br.ReadInt32();
            m_oppositeNodeIndex = br.ReadInt32();
            m_edgeData = br.ReadUInt32();
            m_edgeCost = br.ReadInt16();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_nodeIndex);
            bw.WriteInt32(m_oppositeNodeIndex);
            bw.WriteUInt32(m_edgeData);
            bw.WriteInt16(m_edgeCost);
            bw.WriteUInt16(0);
        }
    }
}
