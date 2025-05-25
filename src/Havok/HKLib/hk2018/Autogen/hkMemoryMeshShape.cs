// Automatically Generated

namespace HKLib.hk2018;

public class hkMemoryMeshShape : hkMeshShape
{
    public List<hkMemoryMeshShape.Section> m_sections = new();

    public List<ushort> m_indices16 = new();

    public List<uint> m_indices32 = new();

    public string? m_name;


    public class Section : IHavokObject
    {
        public hkMeshVertexBuffer? m_vertexBuffer;

        public hkMeshMaterial? m_material;

        public hkMeshBoneIndexMapping m_boneMatrixMap = new();

        public hkMeshSection.PrimitiveType m_primitiveType;

        public int m_numPrimitives;

        public hkMeshSection.MeshSectionIndexType m_indexType;

        public int m_vertexStartIndex;

        public int m_transformIndex;

        public int m_indexBufferOffset;

    }


}

