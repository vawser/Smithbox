// Automatically Generated

namespace HKLib.hk2018;

public class hkbStateMachine : hkbGenerator, hkbVerifiable
{
    public hkbEvent m_eventToSendWhenStateOrTransitionChanges = new();

    public hkbCustomIdSelector? m_startStateIdSelector;

    public int m_startStateId;

    public int m_returnToPreviousStateEventId;

    public int m_randomTransitionEventId;

    public int m_transitionToNextHigherStateEventId;

    public int m_transitionToNextLowerStateEventId;

    public int m_syncVariableIndex;

    public bool m_wrapAroundStateId;

    public sbyte m_maxSimultaneousTransitions;

    public hkbStateMachine.StartStateMode m_startStateMode;

    public hkbStateMachine.StateMachineSelfTransitionMode m_selfTransitionMode;

    public List<hkbStateMachine.StateInfo?> m_states = new();

    public hkbStateMachine.TransitionInfoArray? m_wildcardTransitions;


    public enum StateMachineSelfTransitionMode : int
    {
        SELF_TRANSITION_MODE_NO_TRANSITION = 0,
        SELF_TRANSITION_MODE_TRANSITION_TO_START_STATE = 1,
        SELF_TRANSITION_MODE_FORCE_TRANSITION_TO_START_STATE = 2
    }

    public enum StartStateMode : int
    {
        START_STATE_MODE_DEFAULT = 0,
        START_STATE_MODE_SYNC = 1,
        START_STATE_MODE_RANDOM = 2,
        START_STATE_MODE_CHOOSER = 3
    }

    public class DelayedTransitionInfo : IHavokObject
    {
        public hkbStateMachine.ProspectiveTransitionInfo m_delayedTransition = new();

        public float m_timeDelayed;

        public bool m_isDelayedTransitionReturnToPreviousState;

        public bool m_wasInAbutRangeLastFrame;

    }


    public class NestedStateMachineData : IHavokObject
    {
    }


    public class StateInfo : hkbBindable
    {
        public List<hkbStateListener?> m_listeners = new();

        public hkbStateMachine.EventPropertyArray? m_enterNotifyEvents;

        public hkbStateMachine.EventPropertyArray? m_exitNotifyEvents;

        public hkbStateMachine.TransitionInfoArray? m_transitions;

        public hkbGenerator? m_generator;

        public string? m_name;

        public int m_stateId;

        public float m_probability;

        public bool m_enable;

    }


    public class EventPropertyArray : hkReferencedObject
    {
        public List<hkbEventProperty> m_events = new();

    }


    public class TransitionInfoArray : hkReferencedObject
    {
        public List<hkbStateMachine.TransitionInfo> m_transitions = new();

    }


    public class ProspectiveTransitionInfo : IHavokObject
    {
        public hkbStateMachine.TransitionInfoReference m_transitionInfoReference = new();

        public hkbStateMachine.TransitionInfoReference m_transitionInfoReferenceForTE = new();

        public int m_toStateId;

    }


    public class ActiveTransitionInfo : IHavokObject
    {
        public hkbNodeInternalStateInfo? m_transitionEffectInternalStateInfo;

        public hkbStateMachine.TransitionInfoReference m_transitionInfoReference = new();

        public hkbStateMachine.TransitionInfoReference m_transitionInfoReferenceForTE = new();

        public int m_fromStateId;

        public int m_toStateId;

        public bool m_isReturnToPreviousState;

    }


    public class TransitionInfoReference : IHavokObject
    {
        public short m_fromStateIndex;

        public short m_transitionIndex;

        public short m_stateMachineId;

    }


    public class TransitionInfo : IHavokObject
    {
        public hkbStateMachine.TimeInterval m_triggerInterval = new();

        public hkbStateMachine.TimeInterval m_initiateInterval = new();

        public hkbTransitionEffect? m_transition;

        public hkbCondition? m_condition;

        public int m_eventId;

        public int m_toStateId;

        public int m_fromNestedStateId;

        public int m_toNestedStateId;

        public short m_priority;

        public hkbStateMachine.TransitionInfo.TransitionFlags m_flags;


        [Flags]
        public enum TransitionFlags : int
        {
            FLAG_USE_TRIGGER_INTERVAL = 1,
            FLAG_USE_INITIATE_INTERVAL = 2,
            FLAG_UNINTERRUPTIBLE_WHILE_PLAYING = 4,
            FLAG_UNINTERRUPTIBLE_WHILE_DELAYED = 8,
            FLAG_DELAY_STATE_CHANGE = 16,
            FLAG_DISABLED = 32,
            FLAG_DISALLOW_RETURN_TO_PREVIOUS_STATE = 64,
            FLAG_DISALLOW_RANDOM_TRANSITION = 128,
            FLAG_DISABLE_CONDITION = 256,
            FLAG_ALLOW_SELF_TRANSITION_BY_TRANSITION_FROM_ANY_STATE = 512,
            FLAG_IS_GLOBAL_WILDCARD = 1024,
            FLAG_IS_LOCAL_WILDCARD = 2048,
            FLAG_FROM_NESTED_STATE_ID_IS_VALID = 4096,
            FLAG_TO_NESTED_STATE_ID_IS_VALID = 8192,
            FLAG_ABUT_AT_END_OF_FROM_GENERATOR = 16384
        }

    }


    public class TimeInterval : IHavokObject
    {
        public int m_enterEventId;

        public int m_exitEventId;

        public float m_enterTime;

        public float m_exitTime;

    }


}

