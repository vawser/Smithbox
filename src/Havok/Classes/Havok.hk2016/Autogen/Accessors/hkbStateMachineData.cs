// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbStateMachineData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbStateMachine>
{
    public hkbStateMachineData(Havok.hk2016.hkbStateMachine instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_eventToSendWhenStateOrTransitionChanges":
            case "eventToSendWhenStateOrTransitionChanges":
            {
                value = instance.m_eventToSendWhenStateOrTransitionChanges;
                return true;
            }
            case "m_startStateIdSelector":
            case "startStateIdSelector":
            {
                value = instance.m_startStateIdSelector;
                return true;
            }
            case "m_startStateId":
            case "startStateId":
            {
                value = instance.m_startStateId;
                return true;
            }
            case "m_returnToPreviousStateEventId":
            case "returnToPreviousStateEventId":
            {
                value = instance.m_returnToPreviousStateEventId;
                return true;
            }
            case "m_randomTransitionEventId":
            case "randomTransitionEventId":
            {
                value = instance.m_randomTransitionEventId;
                return true;
            }
            case "m_transitionToNextHigherStateEventId":
            case "transitionToNextHigherStateEventId":
            {
                value = instance.m_transitionToNextHigherStateEventId;
                return true;
            }
            case "m_transitionToNextLowerStateEventId":
            case "transitionToNextLowerStateEventId":
            {
                value = instance.m_transitionToNextLowerStateEventId;
                return true;
            }
            case "m_syncVariableIndex":
            case "syncVariableIndex":
            {
                value = instance.m_syncVariableIndex;
                return true;
            }
            case "m_wrapAroundStateId":
            case "wrapAroundStateId":
            {
                value = instance.m_wrapAroundStateId;
                return true;
            }
            case "m_maxSimultaneousTransitions":
            case "maxSimultaneousTransitions":
            {
                value = instance.m_maxSimultaneousTransitions;
                return true;
            }
            case "m_startStateMode":
            case "startStateMode":
            {
                value = instance.m_startStateMode;
                return true;
            }
            case "m_selfTransitionMode":
            case "selfTransitionMode":
            {
                value = instance.m_selfTransitionMode;
                return true;
            }
            case "m_states":
            case "states":
            {
                value = instance.m_states;
                return true;
            }
            case "m_wildcardTransitions":
            case "wildcardTransitions":
            {
                value = instance.m_wildcardTransitions;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2016.hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_eventToSendWhenStateOrTransitionChanges":
            case "eventToSendWhenStateOrTransitionChanges":
            {
                if (value is not Havok.hk2016.hkbEvent castValue) return false;
                instance.m_eventToSendWhenStateOrTransitionChanges = castValue;
                return true;
            }
            case "m_startStateIdSelector":
            case "startStateIdSelector":
            {
                if (value is null)
                {
                    instance.m_startStateIdSelector = default;
                    return true;
                }
                if (value is Havok.hk2016.hkbCustomIdSelector castValue)
                {
                    instance.m_startStateIdSelector = castValue;
                    return true;
                }
                return false;
            }
            case "m_startStateId":
            case "startStateId":
            {
                if (value is not int castValue) return false;
                instance.m_startStateId = castValue;
                return true;
            }
            case "m_returnToPreviousStateEventId":
            case "returnToPreviousStateEventId":
            {
                if (value is not int castValue) return false;
                instance.m_returnToPreviousStateEventId = castValue;
                return true;
            }
            case "m_randomTransitionEventId":
            case "randomTransitionEventId":
            {
                if (value is not int castValue) return false;
                instance.m_randomTransitionEventId = castValue;
                return true;
            }
            case "m_transitionToNextHigherStateEventId":
            case "transitionToNextHigherStateEventId":
            {
                if (value is not int castValue) return false;
                instance.m_transitionToNextHigherStateEventId = castValue;
                return true;
            }
            case "m_transitionToNextLowerStateEventId":
            case "transitionToNextLowerStateEventId":
            {
                if (value is not int castValue) return false;
                instance.m_transitionToNextLowerStateEventId = castValue;
                return true;
            }
            case "m_syncVariableIndex":
            case "syncVariableIndex":
            {
                if (value is not int castValue) return false;
                instance.m_syncVariableIndex = castValue;
                return true;
            }
            case "m_wrapAroundStateId":
            case "wrapAroundStateId":
            {
                if (value is not bool castValue) return false;
                instance.m_wrapAroundStateId = castValue;
                return true;
            }
            case "m_maxSimultaneousTransitions":
            case "maxSimultaneousTransitions":
            {
                if (value is not sbyte castValue) return false;
                instance.m_maxSimultaneousTransitions = castValue;
                return true;
            }
            case "m_startStateMode":
            case "startStateMode":
            {
                if (value is Havok.hk2016.hkbStateMachine.StartStateMode castValue)
                {
                    instance.m_startStateMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_startStateMode = (Havok.hk2016.hkbStateMachine.StartStateMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_selfTransitionMode":
            case "selfTransitionMode":
            {
                if (value is Havok.hk2016.hkbStateMachine.StateMachineSelfTransitionMode castValue)
                {
                    instance.m_selfTransitionMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_selfTransitionMode = (Havok.hk2016.hkbStateMachine.StateMachineSelfTransitionMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_states":
            case "states":
            {
                if (value is not List<Havok.hk2016.hkbStateMachine.StateInfo?> castValue) return false;
                instance.m_states = castValue;
                return true;
            }
            case "m_wildcardTransitions":
            case "wildcardTransitions":
            {
                if (value is null)
                {
                    instance.m_wildcardTransitions = default;
                    return true;
                }
                if (value is Havok.hk2016.hkbStateMachine.TransitionInfoArray castValue)
                {
                    instance.m_wildcardTransitions = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
