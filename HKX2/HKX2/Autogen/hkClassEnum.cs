using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkClassEnum : IHavokObject
    {
        public virtual uint Signature { get => 2318797263; }
        
        public enum FlagValues
        {
            FLAGS_NONE = 0,
        }
        
        public string m_name;
        public List<hkClassEnumItem> m_items;
        public uint m_flags;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            // Read TYPE_SIMPLEARRAY
            br.ReadUInt64();
            m_flags = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            // Read TYPE_SIMPLEARRAY
            bw.WriteUInt64(0);
            bw.WriteUInt32(m_flags);
            bw.WriteUInt32(0);
        }
    }
}
