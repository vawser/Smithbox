// Automatically Generated

namespace HKLib.hk2018;

public class hclBendStiffnessConstraintSet : hclConstraintSet
{
    public List<hclBendStiffnessConstraintSet.Link> m_links = new();

    public float m_maxRestPoseHeightSq;

    public bool m_clampBendStiffness;

    public bool m_useRestPoseConfig;


    public class Link : IHavokObject
    {
        public float m_weightA;

        public float m_weightB;

        public float m_weightC;

        public float m_weightD;

        public float m_bendStiffness;

        public float m_restCurvature;

        public ushort m_particleA;

        public ushort m_particleB;

        public ushort m_particleC;

        public ushort m_particleD;

    }


}

