using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaSkeletonMapperDataPartitionMappingRange : IHavokObject
    {
        public virtual uint Signature { get => 4260712071; }
        
        public int m_startMappingIndex;
        public int m_numMappings;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_startMappingIndex = br.ReadInt32();
            m_numMappings = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_startMappingIndex);
            bw.WriteInt32(m_numMappings);
        }
    }
}
