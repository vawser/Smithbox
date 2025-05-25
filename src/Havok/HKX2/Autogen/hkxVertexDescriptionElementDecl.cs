using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxVertexDescriptionElementDecl : IHavokObject
    {
        public virtual uint Signature { get => 2254172831; }
        
        public uint m_byteOffset;
        public DataType m_type;
        public DataUsage m_usage;
        public uint m_byteStride;
        public byte m_numElements;
        public string m_channelID;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_byteOffset = br.ReadUInt32();
            m_type = (DataType)br.ReadUInt16();
            m_usage = (DataUsage)br.ReadUInt16();
            m_byteStride = br.ReadUInt32();
            m_numElements = br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_channelID = des.ReadStringPointer(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_byteOffset);
            bw.WriteUInt16((ushort)m_type);
            bw.WriteUInt16((ushort)m_usage);
            bw.WriteUInt32(m_byteStride);
            bw.WriteByte(m_numElements);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteStringPointer(bw, m_channelID);
        }
    }
}
