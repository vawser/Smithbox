// Automatically Generated

namespace HKLib.hk2018;

public class hclObjectSpaceMeshMeshDeformOperator : hclOperator
{
    public uint m_inputBufferIdx;

    public uint m_outputBufferIdx;

    public hclObjectSpaceMeshMeshDeformOperator.ScaleNormalBehaviour m_scaleNormalBehaviour;

    public List<ushort> m_inputTrianglesSubset = new();

    public List<Matrix4x4> m_triangleFromMeshTransforms = new();

    public hclObjectSpaceDeformer m_objectSpaceDeformer = new();

    public bool m_customSkinDeform;


    public enum ScaleNormalBehaviour : int
    {
        SCALE_NORMAL_IGNORE = 0,
        SCALE_NORMAL_APPLY = 1,
        SCALE_NORMAL_INVERT = 2
    }

}

