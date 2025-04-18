// Automatically Generated

namespace HKLib.hk2018;

public class hclMeshBoneDeformOperator : hclOperator
{
    public uint m_inputBufferIdx;

    public uint m_outputTransformSetIdx;

    public List<hclMeshBoneDeformOperator.TriangleBonePair> m_triangleBonePairs = new();

    public List<ushort> m_triangleBoneStartForBone = new();


    public class TriangleBonePair : IHavokObject
    {
        public Matrix4x4 m_localBoneTransform = new();

        public float m_weight;

        public ushort m_triangleIndex;

    }


}

