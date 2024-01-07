using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticPvsBlockHeader : IHavokObject
    {
        public virtual uint Signature { get => 3220399684; }
        
        public uint m_offset;
        public uint m_length;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_offset = br.ReadUInt32();
            m_length = br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_offset);
            bw.WriteUInt32(m_length);
        }
    }
}
