using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum IndexType
    {
        INDEX_TYPE_INVALID = 0,
        INDEX_TYPE_TRI_LIST = 1,
        INDEX_TYPE_TRI_STRIP = 2,
        INDEX_TYPE_TRI_FAN = 3,
        INDEX_TYPE_MAX_ID = 4,
    }
    
    public partial class hkxIndexBuffer : hkReferencedObject
    {
        public override uint Signature { get => 2178010478; }
        
        public IndexType m_indexType;
        public List<ushort> m_indices16;
        public List<uint> m_indices32;
        public uint m_vertexBaseOffset;
        public uint m_length;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_indexType = (IndexType)br.ReadSByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_indices16 = des.ReadUInt16Array(br);
            m_indices32 = des.ReadUInt32Array(br);
            m_vertexBaseOffset = br.ReadUInt32();
            m_length = br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSByte((sbyte)m_indexType);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteUInt16Array(bw, m_indices16);
            s.WriteUInt32Array(bw, m_indices32);
            bw.WriteUInt32(m_vertexBaseOffset);
            bw.WriteUInt32(m_length);
        }
    }
}
