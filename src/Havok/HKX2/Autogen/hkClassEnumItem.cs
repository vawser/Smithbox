using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkClassEnumItem : IHavokObject
    {
        public virtual uint Signature { get => 3463416428; }
        
        public int m_value;
        public string m_name;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_value = br.ReadInt32();
            br.ReadUInt32();
            m_name = des.ReadStringPointer(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_value);
            bw.WriteUInt32(0);
            s.WriteStringPointer(bw, m_name);
        }
    }
}
