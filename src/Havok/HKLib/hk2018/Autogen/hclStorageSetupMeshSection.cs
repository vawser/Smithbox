// Automatically Generated

namespace HKLib.hk2018;

public class hclStorageSetupMeshSection : hkReferencedObject, hclSetupMeshSection
{
    public List<hclStorageSetupMeshSection.SectionVertexChannel?> m_sectionVertexChannels = new();

    public List<hclStorageSetupMeshSection.SectionEdgeSelectionChannel?> m_sectionEdgeChannels = new();

    public List<hclStorageSetupMeshSection.SectionTriangleSelectionChannel?> m_sectionTriangleChannels = new();

    public List<hclStorageSetupMeshSection.BoneInfluences?> m_boneInfluences = new();

    public hclSetupMesh? m_parentSetupMesh;

    public List<Vector4> m_vertices = new();

    public List<Vector4> m_normals = new();

    public List<Vector4> m_tangents = new();

    public List<Vector4> m_bitangents = new();

    public List<hclSetupMeshSection.Triangle> m_triangles = new();

    public List<ushort> m_normalIDs = new();


    public class BoneInfluences : hkReferencedObject
    {
        public List<uint> m_boneIndices = new();

        public List<float> m_weights = new();

    }


    public class SectionTriangleSelectionChannel : hkReferencedObject
    {
        public List<uint> m_triangleIndices = new();

    }


    public class SectionEdgeSelectionChannel : hkReferencedObject
    {
        public List<uint> m_edgeIndices = new();

    }


    public class SectionVertexFloatChannel : hclStorageSetupMeshSection.SectionVertexChannel
    {
        public List<float> m_vertexFloats = new();

    }


    public class SectionVertexSelectionChannel : hclStorageSetupMeshSection.SectionVertexChannel
    {
        public List<uint> m_vertexIndices = new();

    }


    public class SectionVertexChannel : hkReferencedObject
    {
    }


}

