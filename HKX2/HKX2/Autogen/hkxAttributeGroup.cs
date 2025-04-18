using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxAttributeGroup : IHavokObject
    {
        public virtual uint Signature { get => 878487901; }
        
        public string m_name;
        public List<hkxAttribute> m_attributes;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            m_attributes = des.ReadClassArray<hkxAttribute>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            s.WriteClassArray<hkxAttribute>(bw, m_attributes);
        }
    }
}
