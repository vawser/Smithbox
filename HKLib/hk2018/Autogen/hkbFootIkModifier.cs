// Automatically Generated

namespace HKLib.hk2018;

public class hkbFootIkModifier : hkbModifier, hkbVerifiable
{
    public hkbFootIkGains m_gains = new();

    public List<hkbFootIkModifier.Leg> m_legs = new();

    public float m_raycastDistanceUp;

    public float m_raycastDistanceDown;

    public float m_originalGroundHeightMS;

    public float m_errorOut;

    public float m_verticalOffset;

    public uint m_collisionFilterInfo;

    public float m_forwardAlignFraction;

    public float m_sidewaysAlignFraction;

    public float m_sidewaysSampleWidth;

    public bool m_useTrackData;

    public bool m_lockFeetWhenPlanted;

    public bool m_useCharacterUpVector;

    public bool m_keepSourceFootEndAboveGround;

    public hkbFootIkModifier.AlignMode m_alignMode;


    public enum AlignMode : int
    {
        ALIGN_MODE_FORWARD_RIGHT = 0,
        ALIGN_MODE_FORWARD = 1
    }

    public class InternalLegData : IHavokObject
    {
        public Vector4 m_groundPosition = new();

    }


    public class Leg : IHavokObject
    {
        public hkQsTransform m_originalAnkleTransformMS = new();

        public Vector4 m_kneeAxisLS = new();

        public Vector4 m_footEndLS = new();

        public hkbEventProperty m_ungroundedEvent = new();

        public float m_footPlantedAnkleHeightMS;

        public float m_footRaisedAnkleHeightMS;

        public float m_maxAnkleHeightMS;

        public float m_minAnkleHeightMS;

        public float m_maxFootPitchDegrees;

        public float m_minFootPitchDegrees;

        public float m_maxFootRollDegrees;

        public float m_minFootRollDegrees;

        public float m_heelOffsetFromAnkle;

        public bool m_favorToeInterpenetrationOverSteepSlope;

        public bool m_favorHeelInterpenetrationOverSteepSlope;

        public float m_maxKneeAngleDegrees;

        public float m_minKneeAngleDegrees;

        public float m_verticalError;

        public short m_hipIndex;

        public short m_kneeIndex;

        public short m_ankleIndex;

        public bool m_hitSomething;

        public bool m_isPlantedMS;

        public bool m_isOriginalAnkleTransformMSSet;

    }


}

