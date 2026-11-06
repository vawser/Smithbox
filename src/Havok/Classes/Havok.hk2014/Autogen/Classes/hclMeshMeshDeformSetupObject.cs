// Automatically Generated

namespace Havok.hk2014;

public class hclMeshMeshDeformSetupObject : hclOperatorSetupObject
{
    public string? m_name;

    public hclBufferSetupObject? m_inputBufferSetupObject;

    public hclTriangleSelectionInput m_inputTriangleSelection = new();

    public hclBufferSetupObject? m_outputBufferSetupObject;

    public hclVertexSelectionInput m_outputVertexSelection = new();

    public hclVertexFloatInput m_influenceRadiusPerVertex = new();

    public uint m_scaleNormalBehaviour;

    public uint m_maxTrianglesPerVertex;

    public float m_minimumTriangleWeight;

    public bool m_deformNormals;

    public bool m_deformTangents;

    public bool m_deformBiTangents;

    public bool m_useMeshTopology;

}
