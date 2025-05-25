using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiIntervalPartitionLibrary : IHavokObject
    {
        public virtual uint Signature { get => 181298268; }
        
        public List<float> m_data;
        public List<hkaiIntervalPartitionLibraryPartitionRecord> m_partitionRecords;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_data = des.ReadSingleArray(br);
            m_partitionRecords = des.ReadClassArray<hkaiIntervalPartitionLibraryPartitionRecord>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteSingleArray(bw, m_data);
            s.WriteClassArray<hkaiIntervalPartitionLibraryPartitionRecord>(bw, m_partitionRecords);
        }
    }
}
