// Automatically Generated

namespace Havok.hk2014;

public class hkbFootIkDriverInfo : hkReferencedObject
{
    public List<hkbFootIkDriverInfo.Leg> m_legs = new();

    public float m_raycastDistanceUp;

    public float m_raycastDistanceDown;

    public float m_originalGroundHeightMS;

    public float m_verticalOffset;

    public uint m_collisionFilterInfo;

    public float m_forwardAlignFraction;

    public float m_sidewaysAlignFraction;

    public float m_sidewaysSampleWidth;

    public bool m_lockFeetWhenPlanted;

    public bool m_useCharacterUpVector;

    public bool m_isQuadrupedNarrow;

    public bool m_keepSourceFootEndAboveGround;


    public class Leg : IHavokObject
    {
        public Vector4 m_kneeAxisLS = new();

        public Vector4 m_footEndLS = new();

        public float m_footPlantedAnkleHeightMS;

        public float m_footRaisedAnkleHeightMS;

        public float m_maxAnkleHeightMS;

        public float m_minAnkleHeightMS;

        public float m_maxKneeAngleDegrees;

        public float m_minKneeAngleDegrees;

        public short m_hipIndex;

        public short m_hipSiblingIndex;

        public short m_kneeIndex;

        public short m_kneeSiblingIndex;

        public short m_ankleIndex;

        public bool m_ForceFootGround;

    }

}
