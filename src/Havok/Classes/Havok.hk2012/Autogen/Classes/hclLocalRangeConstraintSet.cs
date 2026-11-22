// Automatically Generated

namespace Havok.hk2012;

public class hclLocalRangeConstraintSet : hclConstraintSet
{
    public List<hclLocalRangeConstraintSet.LocalConstraint> m_localConstraints = new();

    public uint m_referenceMeshBufferIdx;

    public bool m_applyNormalComponent;

    public float m_stiffness;


    public class LocalConstraint : IHavokObject
    {
        public ushort m_particleIndex;

        public ushort m_referenceVertex;

        public float m_maximumDistance;

        public float m_maxNormalDistance;

        public float m_minNormalDistance;

    }

}
