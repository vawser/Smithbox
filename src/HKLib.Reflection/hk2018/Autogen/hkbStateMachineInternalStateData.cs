// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbStateMachineInternalStateData : HavokData<hkbStateMachineInternalState> 
{
    public hkbStateMachineInternalStateData(HavokType type, hkbStateMachineInternalState instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_activeTransitions":
            case "activeTransitions":
            {
                if (instance.m_activeTransitions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transitionFlags":
            case "transitionFlags":
            {
                if (instance.m_transitionFlags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wildcardTransitionFlags":
            case "wildcardTransitionFlags":
            {
                if (instance.m_wildcardTransitionFlags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_delayedTransitions":
            case "delayedTransitions":
            {
                if (instance.m_delayedTransitions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timeInState":
            case "timeInState":
            {
                if (instance.m_timeInState is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lastLocalTime":
            case "lastLocalTime":
            {
                if (instance.m_lastLocalTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentStateId":
            case "currentStateId":
            {
                if (instance.m_currentStateId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_previousStateId":
            case "previousStateId":
            {
                if (instance.m_previousStateId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextStartStateIndexOverride":
            case "nextStartStateIndexOverride":
            {
                if (instance.m_nextStartStateIndexOverride is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stateOrTransitionChanged":
            case "stateOrTransitionChanged":
            {
                if (instance.m_stateOrTransitionChanged is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_echoNextUpdate":
            case "echoNextUpdate":
            {
                if (instance.m_echoNextUpdate is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hasEventlessWildcardTransitions":
            case "hasEventlessWildcardTransitions":
            {
                if (instance.m_hasEventlessWildcardTransitions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_activeTransitions":
            case "activeTransitions":
            {
                if (value is not List<hkbStateMachine.ActiveTransitionInfo> castValue) return false;
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
                if (value is not List<hkbStateMachine.DelayedTransitionInfo> castValue) return false;
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
