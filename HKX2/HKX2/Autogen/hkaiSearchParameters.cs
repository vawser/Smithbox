using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ElementSizes
    {
        OPENSET_ELEMENT_SIZE = 8,
        SEARCH_STATE_ELEMENT_SIZE = 18,
        SEARCH_STATE_OVERHEAD = 512,
    }
    
    public enum MemoryDefaultsSingleThreaded
    {
        OPEN_SET_SIZE_SINGLE_THREADED = 131072,
        SEARCH_STATE_SIZE_SINGLE_THREADED = 590336,
        HIERARCHY_OPEN_SET_SIZE_SINGLE_THREADED = 32768,
        HIERARCHY_SEARCH_STATE_SIZE_SINGLE_THREADED = 147968,
    }
    
    public enum MemoryDefaultsMultiThreaded
    {
        OPEN_SET_SIZE_MULTI_THREADED = 8192,
        SEARCH_STATE_SIZE_MULTI_THREADED = 37376,
        HIERARCHY_OPEN_SET_SIZE_MULTI_THREADED = 2048,
        HIERARCHY_SEARCH_STATE_SIZE_MULTI_THREADED = 9728,
    }
    
    public partial class hkaiSearchParameters : IHavokObject
    {
        public virtual uint Signature { get => 1877815828; }
        
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte(0);
        }
    }
}
