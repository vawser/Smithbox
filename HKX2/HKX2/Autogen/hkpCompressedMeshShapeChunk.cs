using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpCompressedMeshShapeChunk : IHavokObject
    {
        public virtual uint Signature { get => 1561159613; }
        
        public Vector4 m_offset;
        public List<ushort> m_vertices;
        public List<ushort> m_indices;
        public List<ushort> m_stripLengths;
        public List<ushort> m_weldingInfo;
        public uint m_materialInfo;
        public ushort m_reference;
        public ushort m_transformIndex;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_offset = des.ReadVector4(br);
            m_vertices = des.ReadUInt16Array(br);
            m_indices = des.ReadUInt16Array(br);
            m_stripLengths = des.ReadUInt16Array(br);
            m_weldingInfo = des.ReadUInt16Array(br);
            m_materialInfo = br.ReadUInt32();
            m_reference = br.ReadUInt16();
            m_transformIndex = br.ReadUInt16();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_offset);
            s.WriteUInt16Array(bw, m_vertices);
            s.WriteUInt16Array(bw, m_indices);
            s.WriteUInt16Array(bw, m_stripLengths);
            s.WriteUInt16Array(bw, m_weldingInfo);
            bw.WriteUInt32(m_materialInfo);
            bw.WriteUInt16(m_reference);
            bw.WriteUInt16(m_transformIndex);
            bw.WriteUInt64(0);
        }
    }
}
