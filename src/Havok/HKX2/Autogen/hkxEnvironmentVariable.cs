using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxEnvironmentVariable : IHavokObject
    {
        public virtual uint Signature { get => 2793492757; }
        
        public string m_name;
        public string m_value;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            m_value = des.ReadStringPointer(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            s.WriteStringPointer(bw, m_value);
        }
    }
}
