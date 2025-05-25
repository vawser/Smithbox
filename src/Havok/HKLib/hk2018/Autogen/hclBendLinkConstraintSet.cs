// Automatically Generated

namespace HKLib.hk2018;

public class hclBendLinkConstraintSet : hclConstraintSet
{
    public List<hclBendLinkConstraintSet.Link> m_links = new();


    public class Link : IHavokObject
    {
        public ushort m_particleA;

        public ushort m_particleB;

        public float m_bendMinLength;

        public float m_stretchMaxLength;

        public float m_bendStiffness;

        public float m_stretchStiffness;

    }


}

