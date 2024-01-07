using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavMeshFace : IHavokObject
    {
        public virtual uint Signature { get => 2758349990; }
        
        public int m_startEdgeIndex;
        public int m_startUserEdgeIndex;
        public short m_numEdges;
        public short m_numUserEdges;
        public short m_clusterIndex;
        public ushort m_padding;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_startEdgeIndex = br.ReadInt32();
            m_startUserEdgeIndex = br.ReadInt32();
            m_numEdges = br.ReadInt16();
            m_numUserEdges = br.ReadInt16();
            m_clusterIndex = br.ReadInt16();
            m_padding = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_startEdgeIndex);
            bw.WriteInt32(m_startUserEdgeIndex);
            bw.WriteInt16(m_numEdges);
            bw.WriteInt16(m_numUserEdges);
            bw.WriteInt16(m_clusterIndex);
            bw.WriteUInt16(m_padding);
        }
    }
}
