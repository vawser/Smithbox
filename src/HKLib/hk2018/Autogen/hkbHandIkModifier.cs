// Automatically Generated

namespace HKLib.hk2018;

public class hkbHandIkModifier : hkbModifier, hkbVerifiable
{
    public List<hkbHandIkModifier.Hand> m_hands = new();

    public hkbBlendCurveUtils.BlendCurve m_fadeInOutCurve;


    public class Hand : IHavokObject
    {
        public Vector4 m_elbowAxisLS = new();

        public Vector4 m_backHandNormalLS = new();

        public Vector4 m_handOffsetLS = new();

        public Quaternion m_handOrientationOffsetLS = new();

        public float m_maxElbowAngleDegrees;

        public float m_minElbowAngleDegrees;

        public short m_shoulderIndex;

        public short m_shoulderSiblingIndex;

        public short m_elbowIndex;

        public short m_elbowSiblingIndex;

        public short m_wristIndex;

        public bool m_enforceEndPosition;

        public bool m_enforceEndRotation;

        public string? m_localFrameName;

    }


}

