// Automatically Generated

namespace HKLib.hk2018;

public class hclMeshMeshDeformSetupObject : hclOperatorSetupObject
{
    public string? m_name;

    public hclBufferSetupObject? m_inputBufferSetupObject;

    public hclBufferSetupObject? m_inputBufferPrevSetupObject;

    public hclTriangleSelectionInput m_inputTriangleSelection = new();

    public hclBufferSetupObject? m_outputBufferSetupObject;

    public hclBufferSetupObject? m_outputBufferPrevSetupObject;

    public hclVertexSelectionInput m_outputVertexSelection = new();

    public hclVertexFloatInput m_influenceRadiusPerVertex = new();

    public hclMeshMeshDeformOperator.ScaleNormalBehaviour m_scaleNormalBehaviour;

    public hclMeshMeshDeformSetupObject.TriangleWeightMode m_triangleWeightMode;

    public uint m_maxTrianglesPerVertex;

    public float m_minimumTriangleWeight;

    public bool m_deformNormals;

    public bool m_deformTangents;

    public bool m_deformBiTangents;

    public bool m_useMeshTopology;


    public enum TriangleWeightMode : int
    {
        TRIANGLE_WEIGHT_CENTROID = 0,
        TRIANGLE_WEIGHT_CLOSEST_POINT = 1
    }

}

