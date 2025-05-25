// Automatically Generated

namespace HKLib.hk2018;

public class hclSkinOperator : hclOperator
{
    public List<hclSkinOperator.BoneInfluence> m_boneInfluences = new();

    public List<ushort> m_boneInfluenceStartPerVertex = new();

    public List<Matrix4x4> m_boneFromSkinMeshTransforms = new();

    public List<ushort> m_usedBoneGroupIds = new();

    public bool m_skinPositions;

    public bool m_skinNormals;

    public bool m_skinTangents;

    public bool m_skinBiTangents;

    public uint m_inputBufferIndex;

    public uint m_outputBufferIndex;

    public uint m_transformSetIndex;

    public ushort m_startVertex;

    public ushort m_endVertex;

    public bool m_partialSkinning;

    public bool m_dualQuaternionSkinning;

    public byte m_boneGroupSize;


    public class BoneInfluence : IHavokObject
    {
        public byte m_boneIndex;

        public byte m_weight;

    }


}

