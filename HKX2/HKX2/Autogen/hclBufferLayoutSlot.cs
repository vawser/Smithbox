using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBufferLayoutSlot : IHavokObject
    {
        public virtual uint Signature { get => 3297114736; }
        
        public SlotFlags m_flags;
        public byte m_stride;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_flags = (SlotFlags)br.ReadByte();
            m_stride = br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte((byte)m_flags);
            bw.WriteByte(m_stride);
        }
    }
}
