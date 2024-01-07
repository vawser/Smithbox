using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ReferenceCountHandling
    {
        REFERENCE_COUNT_INCREMENT = 0,
        REFERENCE_COUNT_IGNORE = 1,
    }
    
    public partial class hkRefCountedProperties : IHavokObject
    {
        public virtual uint Signature { get => 2086094951; }
        
        public List<hkRefCountedPropertiesEntry> m_entries;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_entries = des.ReadClassArray<hkRefCountedPropertiesEntry>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hkRefCountedPropertiesEntry>(bw, m_entries);
        }
    }
}
