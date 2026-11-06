// Automatically Generated

namespace Havok.hk2014;

public class hclVolumeConstraint : hclConstraintSet
{
    public List<hclVolumeConstraint.FrameData> m_frameDatas = new();

    public List<hclVolumeConstraint.ApplyData> m_applyDatas = new();


    public class FrameData : IHavokObject
    {
        public Vector4 m_frameVector = new();

        public ushort m_particleIndex;

        public float m_weight;

    }

    public class ApplyData : IHavokObject
    {
        public Vector4 m_frameVector = new();

        public ushort m_particleIndex;

        public float m_stiffness;

    }

}
