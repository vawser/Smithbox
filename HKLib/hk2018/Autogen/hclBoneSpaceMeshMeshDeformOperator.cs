// Automatically Generated

namespace HKLib.hk2018;

public class hclBoneSpaceMeshMeshDeformOperator : hclOperator
{
    public uint m_inputBufferIdx;

    public uint m_outputBufferIdx;

    public hclBoneSpaceMeshMeshDeformOperator.ScaleNormalBehaviour m_scaleNormalBehaviour;

    public List<ushort> m_inputTrianglesSubset = new();

    public hclBoneSpaceDeformer m_boneSpaceDeformer = new();


    public enum ScaleNormalBehaviour : int
    {
        SCALE_NORMAL_IGNORE = 0,
        SCALE_NORMAL_APPLY = 1,
        SCALE_NORMAL_INVERT = 2
    }

}

