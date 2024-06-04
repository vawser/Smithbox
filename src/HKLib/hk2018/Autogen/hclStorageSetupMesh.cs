// Automatically Generated

namespace HKLib.hk2018;

public class hclStorageSetupMesh : hclSetupMesh
{
    public string? m_name;

    public Matrix4x4 m_worldFromMesh = new();

    public List<hclStorageSetupMeshSection?> m_sections = new();

    public List<hclStorageSetupMesh.VertexChannel> m_vertexChannels = new();

    public List<hclStorageSetupMesh.EdgeChannel> m_edgeChannels = new();

    public List<hclStorageSetupMesh.TriangleChannel> m_triangleChannels = new();

    public List<hclStorageSetupMesh.Bone> m_bones = new();

    public bool m_isSkinned;


    public class Bone : IHavokObject
    {
        public string? m_name;

        public Matrix4x4 m_boneFromSkin = new();

    }


    public class TriangleChannel : IHavokObject
    {
        public string? m_name;

        public hclSetupMesh.TriangleChannelType m_type;

    }


    public class EdgeChannel : IHavokObject
    {
        public string? m_name;

        public hclSetupMesh.EdgeChannelType m_type;

    }


    public class VertexChannel : IHavokObject
    {
        public string? m_name;

        public hclSetupMesh.VertexChannelType m_type;

    }


}

