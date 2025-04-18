// Automatically Generated

namespace HKLib.hk2018;

public class hclMeshBoneDeformSetupObject : hclOperatorSetupObject
{
    public string? m_name;

    public hclBufferSetupObject? m_inputBufferSetupObject;

    public hclTriangleSelectionInput m_inputTriangleSelection = new();

    public hclTransformSetSetupObject? m_outputTransformSetSetupObject;

    public List<string?> m_deformedBones = new();

    public uint m_maxTrianglesPerBone;

    public float m_minimumTriangleWeight;

}

