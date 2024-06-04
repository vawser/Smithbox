// Automatically Generated

namespace HKLib.hk2018;

public class hclCompressibleLinkConstraintSet : hclConstraintSet
{
    public List<hclCompressibleLinkConstraintSet.Link> m_links = new();


    public class Link : IHavokObject
    {
        public ushort m_particleA;

        public ushort m_particleB;

        public float m_restLength;

        public float m_compressionLength;

        public float m_stiffness;

    }


}

