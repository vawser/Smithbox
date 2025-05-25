using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMultipleVertexBufferLockedElement : IHavokObject
    {
        public virtual uint Signature { get => 2699176700; }
        
        public byte m_vertexBufferIndex;
        public byte m_elementIndex;
        public byte m_lockedBufferIndex;
        public byte m_vertexFormatIndex;
        public byte m_lockFlags;
        public byte m_outputBufferIndex;
        public sbyte m_emulatedIndex;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_vertexBufferIndex = br.ReadByte();
            m_elementIndex = br.ReadByte();
            m_lockedBufferIndex = br.ReadByte();
            m_vertexFormatIndex = br.ReadByte();
            m_lockFlags = br.ReadByte();
            m_outputBufferIndex = br.ReadByte();
            m_emulatedIndex = br.ReadSByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte(m_vertexBufferIndex);
            bw.WriteByte(m_elementIndex);
            bw.WriteByte(m_lockedBufferIndex);
            bw.WriteByte(m_vertexFormatIndex);
            bw.WriteByte(m_lockFlags);
            bw.WriteByte(m_outputBufferIndex);
            bw.WriteSByte(m_emulatedIndex);
        }
    }
}
