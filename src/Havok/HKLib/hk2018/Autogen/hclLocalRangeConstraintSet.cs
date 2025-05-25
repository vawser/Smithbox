// Automatically Generated

namespace HKLib.hk2018;

public class hclLocalRangeConstraintSet : hclConstraintSet
{
    public List<hclLocalRangeConstraintSet.LocalConstraint> m_localConstraints = new();

    public uint m_referenceMeshBufferIdx;

    public float m_stiffness;

    public hclLocalRangeConstraintSet.ShapeType m_shapeType;

    public bool m_applyNormalComponent;


    public enum ShapeType : int
    {
        SHAPE_SPHERE = 0,
        SHAPE_CYLINDER = 1
    }

    public class LocalConstraint : IHavokObject
    {
        public ushort m_particleIndex;

        public ushort m_referenceVertex;

        public float m_maximumDistance;

        public float m_maxNormalDistance;

        public float m_minNormalDistance;

    }


}

