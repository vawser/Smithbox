using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMultipleVertexBufferVertexBufferInfo : IHavokObject
    {
        public virtual uint Signature { get => 3673940198; }
        
        public hkMeshVertexBuffer m_vertexBuffer;
        public bool m_isLocked;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_vertexBuffer = des.ReadClassPointer<hkMeshVertexBuffer>(br);
            br.ReadUInt64();
            m_isLocked = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkMeshVertexBuffer>(bw, m_vertexBuffer);
            bw.WriteUInt64(0);
            bw.WriteBoolean(m_isLocked);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
