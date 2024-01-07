using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMeshTextureRawBufferDescriptor : IHavokObject
    {
        public virtual uint Signature { get => 1617460748; }
        
        public long m_offset;
        public uint m_stride;
        public uint m_numElements;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_offset = br.ReadInt64();
            m_stride = br.ReadUInt32();
            m_numElements = br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt64(m_offset);
            bw.WriteUInt32(m_stride);
            bw.WriteUInt32(m_numElements);
        }
    }
}
