using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavVolumeCell : IHavokObject
    {
        public virtual uint Signature { get => 2418669306; }
        
        public ushort m_min_0;
        public ushort m_min_1;
        public ushort m_min_2;
        public short m_numEdges;
        public ushort m_max_0;
        public ushort m_max_1;
        public ushort m_max_2;
        public int m_startEdgeIndex;
        public int m_data;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_min_0 = br.ReadUInt16();
            m_min_1 = br.ReadUInt16();
            m_min_2 = br.ReadUInt16();
            m_numEdges = br.ReadInt16();
            m_max_0 = br.ReadUInt16();
            m_max_1 = br.ReadUInt16();
            m_max_2 = br.ReadUInt16();
            br.ReadUInt16();
            m_startEdgeIndex = br.ReadInt32();
            m_data = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_min_0);
            bw.WriteUInt16(m_min_1);
            bw.WriteUInt16(m_min_2);
            bw.WriteInt16(m_numEdges);
            bw.WriteUInt16(m_max_0);
            bw.WriteUInt16(m_max_1);
            bw.WriteUInt16(m_max_2);
            bw.WriteUInt16(0);
            bw.WriteInt32(m_startEdgeIndex);
            bw.WriteInt32(m_data);
        }
    }
}
