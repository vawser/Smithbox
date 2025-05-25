using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ThreadingType
    {
        SINGLE_THREADED = 0,
        MULTI_THREADED = 1,
    }
    
    public enum SearchType
    {
        SEARCH_REGULAR = 0,
        SEARCH_HIERARCHICAL = 1,
    }
    
    public partial class hkaiSearchParametersBufferSizes : IHavokObject
    {
        public virtual uint Signature { get => 2032672048; }
        
        public int m_maxOpenSetSizeBytes;
        public int m_maxSearchStateSizeBytes;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_maxOpenSetSizeBytes = br.ReadInt32();
            m_maxSearchStateSizeBytes = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_maxOpenSetSizeBytes);
            bw.WriteInt32(m_maxSearchStateSizeBytes);
        }
    }
}
