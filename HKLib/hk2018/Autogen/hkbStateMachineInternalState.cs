// Automatically Generated

namespace HKLib.hk2018;

public class hkbStateMachineInternalState : hkReferencedObject
{
    public List<hkbStateMachine.ActiveTransitionInfo> m_activeTransitions = new();

    public List<byte> m_transitionFlags = new();

    public List<byte> m_wildcardTransitionFlags = new();

    public List<hkbStateMachine.DelayedTransitionInfo> m_delayedTransitions = new();

    public float m_timeInState;

    public float m_lastLocalTime;

    public int m_currentStateId;

    public int m_previousStateId;

    public int m_nextStartStateIndexOverride;

    public bool m_stateOrTransitionChanged;

    public bool m_echoNextUpdate;

    public bool m_hasEventlessWildcardTransitions;

}

