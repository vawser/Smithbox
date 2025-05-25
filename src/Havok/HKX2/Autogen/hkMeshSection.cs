using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum MeshSectionIndexType
    {
        INDEX_TYPE_NONE = 0,
        INDEX_TYPE_UINT16 = 1,
        INDEX_TYPE_UINT32 = 2,
    }
    
    public enum PrimitiveType
    {
        PRIMITIVE_TYPE_UNKNOWN = 0,
        PRIMITIVE_TYPE_POINT_LIST = 1,
        PRIMITIVE_TYPE_LINE_LIST = 2,
        PRIMITIVE_TYPE_TRIANGLE_LIST = 3,
        PRIMITIVE_TYPE_TRIANGLE_STRIP = 4,
    }
    
    public partial class hkMeshSection : IHavokObject
    {
        public virtual uint Signature { get => 1570360411; }
        
        public PrimitiveType m_primitiveType;
        public int m_numPrimitives;
        public int m_numIndices;
        public int m_vertexStartIndex;
        public int m_transformIndex;
        public MeshSectionIndexType m_indexType;
        public hkMeshVertexBuffer m_vertexBuffer;
        public hkMeshMaterial m_material;
        public hkMeshBoneIndexMapping m_boneMatrixMap;
        public int m_sectionIndex;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_primitiveType = (PrimitiveType)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_numPrimitives = br.ReadInt32();
            m_numIndices = br.ReadInt32();
            m_vertexStartIndex = br.ReadInt32();
            m_transformIndex = br.ReadInt32();
            m_indexType = (MeshSectionIndexType)br.ReadByte();
            br.ReadUInt64();
            br.ReadUInt16();
            br.ReadByte();
            m_vertexBuffer = des.ReadClassPointer<hkMeshVertexBuffer>(br);
            m_material = des.ReadClassPointer<hkMeshMaterial>(br);
            m_boneMatrixMap = des.ReadClassPointer<hkMeshBoneIndexMapping>(br);
            m_sectionIndex = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte((byte)m_primitiveType);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_numPrimitives);
            bw.WriteInt32(m_numIndices);
            bw.WriteInt32(m_vertexStartIndex);
            bw.WriteInt32(m_transformIndex);
            bw.WriteByte((byte)m_indexType);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteClassPointer<hkMeshVertexBuffer>(bw, m_vertexBuffer);
            s.WriteClassPointer<hkMeshMaterial>(bw, m_material);
            s.WriteClassPointer<hkMeshBoneIndexMapping>(bw, m_boneMatrixMap);
            bw.WriteInt32(m_sectionIndex);
            bw.WriteUInt32(0);
        }
    }
}
