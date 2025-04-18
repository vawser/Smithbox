// Automatically Generated

namespace HKLib.hk2018;

public class hkbTransitionEffect : hkbGenerator, hkbVerifiable
{
    public hkbTransitionEffect.SelfTransitionMode m_selfTransitionMode;

    public hkbTransitionEffect.EventMode m_eventMode;


    public enum EventMode : int
    {
        EVENT_MODE_DEFAULT = 0,
        EVENT_MODE_PROCESS_ALL = 1,
        EVENT_MODE_IGNORE_FROM_GENERATOR = 2,
        EVENT_MODE_IGNORE_TO_GENERATOR = 3
    }

    public enum SelfTransitionMode : int
    {
        SELF_TRANSITION_MODE_CONTINUE_IF_CYCLIC_BLEND_IF_ACYCLIC = 0,
        SELF_TRANSITION_MODE_CONTINUE = 1,
        SELF_TRANSITION_MODE_RESET = 2,
        SELF_TRANSITION_MODE_BLEND = 3
    }

}

