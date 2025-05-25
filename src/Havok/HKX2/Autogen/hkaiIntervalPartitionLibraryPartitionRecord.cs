using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiIntervalPartitionLibraryPartitionRecord : IHavokObject
    {
        public virtual uint Signature { get => 2435020000; }
        
        public uint m_intervalDataOffset;
        public ushort m_numIntervals;
        public bool m_hasYData;
        public bool m_hasIntData;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_intervalDataOffset = br.ReadUInt32();
            m_numIntervals = br.ReadUInt16();
            m_hasYData = br.ReadBoolean();
            m_hasIntData = br.ReadBoolean();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_intervalDataOffset);
            bw.WriteUInt16(m_numIntervals);
            bw.WriteBoolean(m_hasYData);
            bw.WriteBoolean(m_hasIntData);
        }
    }
}
