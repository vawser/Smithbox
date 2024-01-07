using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMemoryMeshShapeSection : IHavokObject
    {
        public virtual uint Signature { get => 595197493; }
        
        public hkMeshVertexBuffer m_vertexBuffer;
        public hkMeshMaterial m_material;
        public hkMeshBoneIndexMapping m_boneMatrixMap;
        public PrimitiveType m_primitiveType;
        public int m_numPrimitives;
        public MeshSectionIndexType m_indexType;
        public int m_vertexStartIndex;
        public int m_transformIndex;
        public int m_indexBufferOffset;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_vertexBuffer = des.ReadClassPointer<hkMeshVertexBuffer>(br);
            m_material = des.ReadClassPointer<hkMeshMaterial>(br);
            m_boneMatrixMap = new hkMeshBoneIndexMapping();
            m_boneMatrixMap.Read(des, br);
            m_primitiveType = (PrimitiveType)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_numPrimitives = br.ReadInt32();
            m_indexType = (MeshSectionIndexType)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_vertexStartIndex = br.ReadInt32();
            m_transformIndex = br.ReadInt32();
            m_indexBufferOffset = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkMeshVertexBuffer>(bw, m_vertexBuffer);
            s.WriteClassPointer<hkMeshMaterial>(bw, m_material);
            m_boneMatrixMap.Write(s, bw);
            bw.WriteByte((byte)m_primitiveType);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_numPrimitives);
            bw.WriteByte((byte)m_indexType);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_vertexStartIndex);
            bw.WriteInt32(m_transformIndex);
            bw.WriteInt32(m_indexBufferOffset);
        }
    }
}
