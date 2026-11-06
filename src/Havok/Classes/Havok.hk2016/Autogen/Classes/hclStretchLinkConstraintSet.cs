// Automatically Generated

namespace Havok.hk2016;

public class hclStretchLinkConstraintSet : hclConstraintSet
{
    public List<hclStretchLinkConstraintSet.Link> m_links = new();


    public class Link : IHavokObject
    {
        public ushort m_particleA;

        public ushort m_particleB;

        public float m_restLength;

        public float m_stiffness;

    }

}
