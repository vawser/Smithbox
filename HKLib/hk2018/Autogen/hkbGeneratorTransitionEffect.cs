// Automatically Generated

namespace HKLib.hk2018;

public class hkbGeneratorTransitionEffect : hkbTransitionEffect, hkbVerifiable
{
    public hkbGenerator? m_transitionGenerator;

    public float m_blendInDuration;

    public float m_blendOutDuration;

    public bool m_syncToGeneratorStartTime;


    public enum Stage : int
    {
        STAGE_BLENDING_IN = 0,
        STAGE_PLAYING_TRANSITION_GENERATOR = 1,
        STAGE_BLENDING_OUT = 2
    }

    public enum ToGeneratorState : int
    {
        STATE_INACTIVE = 0,
        STATE_READY_FOR_SET_LOCAL_TIME = 1,
        STATE_READY_FOR_APPLY_SELF_TRANSITION_MODE = 2,
        STATE_ACTIVE = 3
    }

}

