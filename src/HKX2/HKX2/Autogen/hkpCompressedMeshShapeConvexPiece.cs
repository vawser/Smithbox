using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpCompressedMeshShapeConvexPiece : IHavokObject
    {
        public virtual uint Signature { get => 1114430838; }
        
        public Vector4 m_offset;
        public List<ushort> m_vertices;
        public ushort m_reference;
        public ushort m_transformIndex;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_offset = des.ReadVector4(br);
            m_vertices = des.ReadUInt16Array(br);
            m_reference = br.ReadUInt16();
            m_transformIndex = br.ReadUInt16();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_offset);
            s.WriteUInt16Array(bw, m_vertices);
            bw.WriteUInt16(m_reference);
            bw.WriteUInt16(m_transformIndex);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
