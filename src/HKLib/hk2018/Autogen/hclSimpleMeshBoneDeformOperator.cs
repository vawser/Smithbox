// Automatically Generated

namespace HKLib.hk2018;

public class hclSimpleMeshBoneDeformOperator : hclOperator
{
    public uint m_inputBufferIdx;

    public uint m_outputTransformSetIdx;

    public List<hclSimpleMeshBoneDeformOperator.TriangleBonePair> m_triangleBonePairs = new();

    public List<Matrix4x4> m_localBoneTransforms = new();


    public class TriangleBonePair : IHavokObject
    {
        public ushort m_boneOffset;

        public ushort m_triangleOffset;

    }


}

