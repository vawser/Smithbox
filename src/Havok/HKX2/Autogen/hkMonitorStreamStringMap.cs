using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMonitorStreamStringMap : IHavokObject
    {
        public virtual uint Signature { get => 3302205620; }
        
        public List<hkMonitorStreamStringMapStringMap> m_map;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_map = des.ReadClassArray<hkMonitorStreamStringMapStringMap>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hkMonitorStreamStringMapStringMap>(bw, m_map);
        }
    }
}
