// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbStateMachineInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbStateMachineInternalState>
{
    public hkbStateMachineInternalStateData(Havok.hk2014.hkbStateMachineInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_activeTransitions":
            case "activeTransitions":
            {
                value = instance.m_activeTransitions;
                return true;
            }
            case "m_transitionFlags":
            case "transitionFlags":
            {
                value = instance.m_transitionFlags;
                return true;
            }
            case "m_wildcardTransitionFlags":
            case "wildcardTransitionFlags":
            {
                value = instance.m_wildcardTransitionFlags;
                return true;
            }
            case "m_delayedTransitions":
            case "delayedTransitions":
            {
                value = instance.m_delayedTransitions;
                return true;
            }
            case "m_timeInState":
            case "timeInState":
            {
                value = instance.m_timeInState;
                return true;
            }
            case "m_lastLocalTime":
            case "lastLocalTime":
            {
                value = instance.m_lastLocalTime;
                return true;
            }
            case "m_currentStateId":
            case "currentStateId":
            {
                value = instance.m_currentStateId;
                return true;
            }
            case "m_previousStateId":
            case "previousStateId":
            {
                value = instance.m_previousStateId;
                return true;
            }
            case "m_nextStartStateIndexOverride":
            case "nextStartStateIndexOverride":
            {
                value = instance.m_nextStartStateIndexOverride;
                return true;
            }
            case "m_stateOrTransitionChanged":
            case "stateOrTransitionChanged":
            {
                value = instance.m_stateOrTransitionChanged;
                return true;
            }
            case "m_echoNextUpdate":
            case "echoNextUpdate":
            {
                value = instance.m_echoNextUpdate;
                return true;
            }
            case "m_hasEventlessWildcardTransitions":
            case "hasEventlessWildcardTransitions":
            {
                value = instance.m_hasEventlessWildcardTransitions;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_activeTransitions":
            case "activeTransitions":
            {
                if (value is not List<Havok.hk2014.hkbStateMachine.ActiveTransitionInfo> castValue) return false;
                instance.m_activeTransitions = castValue;
                return true;
            }
            case "m_transitionFlags":
            case "transitionFlags":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_transitionFlags = castValue;
                return true;
            }
            case "m_wildcardTransitionFlags":
            case "wildcardTransitionFlags":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_wildcardTransitionFlags = castValue;
                return true;
            }
            case "m_delayedTransitions":
            case "delayedTransitions":
            {
                if (value is not List<Havok.hk2014.hkbStateMachine.DelayedTransitionInfo> castValue) return false;
                instance.m_delayedTransitions = castValue;
                return true;
            }
            case "m_timeInState":
            case "timeInState":
            {
                if (value is not float castValue) return false;
                instance.m_timeInState = castValue;
                return true;
            }
            case "m_lastLocalTime":
            case "lastLocalTime":
            {
                if (value is not float castValue) return false;
                instance.m_lastLocalTime = castValue;
                return true;
            }
            case "m_currentStateId":
            case "currentStateId":
            {
                if (value is not int castValue) return false;
                instance.m_currentStateId = castValue;
                return true;
            }
            case "m_previousStateId":
            case "previousStateId":
            {
                if (value is not int castValue) return false;
                instance.m_previousStateId = castValue;
                return true;
            }
            case "m_nextStartStateIndexOverride":
            case "nextStartStateIndexOverride":
            {
                if (value is not int castValue) return false;
                instance.m_nextStartStateIndexOverride = castValue;
                return true;
            }
            case "m_stateOrTransitionChanged":
            case "stateOrTransitionChanged":
            {
                if (value is not bool castValue) return false;
                instance.m_stateOrTransitionChanged = castValue;
                return true;
            }
            case "m_echoNextUpdate":
            case "echoNextUpdate":
            {
                if (value is not bool castValue) return false;
                instance.m_echoNextUpdate = castValue;
                return true;
            }
            case "m_hasEventlessWildcardTransitions":
            case "hasEventlessWildcardTransitions":
            {
                if (value is not bool castValue) return false;
                instance.m_hasEventlessWildcardTransitions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
