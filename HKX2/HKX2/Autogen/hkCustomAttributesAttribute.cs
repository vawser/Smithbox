using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkCustomAttributesAttribute : IHavokObject
    {
        public virtual uint Signature { get => 327734785; }
        
        public string m_name;
        public ulong m_value;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            // Read TYPE_VARIANT
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            // Read TYPE_VARIANT
            bw.WriteUInt64(0);
        }
    }
}
