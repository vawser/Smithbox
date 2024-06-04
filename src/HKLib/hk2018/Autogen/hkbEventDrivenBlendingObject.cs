// Automatically Generated

namespace HKLib.hk2018;

public class hkbEventDrivenBlendingObject : IHavokObject
{
    public float m_weight;

    public float m_fadeInDuration;

    public float m_fadeOutDuration;

    public int m_onEventId;

    public int m_offEventId;

    public bool m_onByDefault;

    public bool m_forceFullFadeDurations;

    public hkbBlendCurveUtils.BlendCurve m_fadeInOutCurve;


    public class InternalState : IHavokObject
    {
        public float m_weight;

        public float m_timeElapsed;

        public float m_onFraction;

        public float m_onFractionOffset;

        public hkbEventDrivenBlendingObject.InternalState.FadingState m_fadingState;


        public enum FadingState : int
        {
            FADING_STATE_NONE = 0,
            FADING_STATE_IN = 1,
            FADING_STATE_OUT = -1
        }

    }


}

