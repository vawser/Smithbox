using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxMaterialProperty : IHavokObject
    {
        public virtual uint Signature { get => 3532989261; }
        
        public uint m_key;
        public uint m_value;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_key = br.ReadUInt32();
            m_value = br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_key);
            bw.WriteUInt32(m_value);
        }
    }
}
