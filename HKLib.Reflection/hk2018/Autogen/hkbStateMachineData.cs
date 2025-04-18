// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbStateMachineData : HavokData<hkbStateMachine> 
{
    public hkbStateMachineData(HavokType type, hkbStateMachine instance) : base(type, instance) {}

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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_eventToSendWhenStateOrTransitionChanges":
            case "eventToSendWhenStateOrTransitionChanges":
            {
                if (instance.m_eventToSendWhenStateOrTransitionChanges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startStateIdSelector":
            case "startStateIdSelector":
            {
                if (instance.m_startStateIdSelector is null)
                {
                    return true;
                }
                if (instance.m_startStateIdSelector is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_startStateId":
            case "startStateId":
            {
                if (instance.m_startStateId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_returnToPreviousStateEventId":
            case "returnToPreviousStateEventId":
            {
                if (instance.m_returnToPreviousStateEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_randomTransitionEventId":
            case "randomTransitionEventId":
            {
                if (instance.m_randomTransitionEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transitionToNextHigherStateEventId":
            case "transitionToNextHigherStateEventId":
            {
                if (instance.m_transitionToNextHigherStateEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transitionToNextLowerStateEventId":
            case "transitionToNextLowerStateEventId":
            {
                if (instance.m_transitionToNextLowerStateEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_syncVariableIndex":
            case "syncVariableIndex":
            {
                if (instance.m_syncVariableIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wrapAroundStateId":
            case "wrapAroundStateId":
            {
                if (instance.m_wrapAroundStateId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxSimultaneousTransitions":
            case "maxSimultaneousTransitions":
            {
                if (instance.m_maxSimultaneousTransitions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startStateMode":
            case "startStateMode":
            {
                if (instance.m_startStateMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_startStateMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_selfTransitionMode":
            case "selfTransitionMode":
            {
                if (instance.m_selfTransitionMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_selfTransitionMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_states":
            case "states":
            {
                if (instance.m_states is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wildcardTransitions":
            case "wildcardTransitions":
            {
                if (instance.m_wildcardTransitions is null)
                {
                    return true;
                }
                if (instance.m_wildcardTransitions is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is hkbVariableBindingSet castValue)
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
                if (value is not hkbEvent castValue) return false;
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
                if (value is hkbCustomIdSelector castValue)
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
                if (value is hkbStateMachine.StartStateMode castValue)
                {
                    instance.m_startStateMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_startStateMode = (hkbStateMachine.StartStateMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_selfTransitionMode":
            case "selfTransitionMode":
            {
                if (value is hkbStateMachine.StateMachineSelfTransitionMode castValue)
                {
                    instance.m_selfTransitionMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_selfTransitionMode = (hkbStateMachine.StateMachineSelfTransitionMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_states":
            case "states":
            {
                if (value is not List<hkbStateMachine.StateInfo?> castValue) return false;
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
                if (value is hkbStateMachine.TransitionInfoArray castValue)
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
