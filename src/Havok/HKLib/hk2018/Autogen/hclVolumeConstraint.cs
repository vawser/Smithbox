// Automatically Generated

namespace HKLib.hk2018;

public class hclVolumeConstraint : hclConstraintSet
{
    public List<hclVolumeConstraint.FrameData> m_frameDatas = new();

    public List<hclVolumeConstraint.ApplyData> m_applyDatas = new();


    public class ApplyData : IHavokObject
    {
        public Vector4 m_frameVector = new();

        public ushort m_particleIndex;

        public float m_stiffness;

    }


    public class FrameData : IHavokObject
    {
        public Vector4 m_frameVector = new();

        public ushort m_particleIndex;

        public float m_weight;

    }


}

