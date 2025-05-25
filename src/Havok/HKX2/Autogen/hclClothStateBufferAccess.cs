using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclClothStateBufferAccess : IHavokObject
    {
        public virtual uint Signature { get => 111790339; }
        
        public uint m_bufferIndex;
        public hclBufferUsage m_bufferUsage;
        public uint m_shadowBufferIndex;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_bufferIndex = br.ReadUInt32();
            m_bufferUsage = new hclBufferUsage();
            m_bufferUsage.Read(des, br);
            br.ReadUInt16();
            br.ReadByte();
            m_shadowBufferIndex = br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_bufferIndex);
            m_bufferUsage.Write(s, bw);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteUInt32(m_shadowBufferIndex);
        }
    }
}
