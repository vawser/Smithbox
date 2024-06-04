// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbStateMachineTransitionInfoData : HavokData<hkbStateMachine.TransitionInfo> 
{
    public hkbStateMachineTransitionInfoData(HavokType type, hkbStateMachine.TransitionInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_triggerInterval":
            case "triggerInterval":
            {
                if (instance.m_triggerInterval is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_initiateInterval":
            case "initiateInterval":
            {
                if (instance.m_initiateInterval is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transition":
            case "transition":
            {
                if (instance.m_transition is null)
                {
                    return true;
                }
                if (instance.m_transition is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_condition":
            case "condition":
            {
                if (instance.m_condition is null)
                {
                    return true;
                }
                if (instance.m_condition is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_eventId":
            case "eventId":
            {
                if (instance.m_eventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toStateId":
            case "toStateId":
            {
                if (instance.m_toStateId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fromNestedStateId":
            case "fromNestedStateId":
            {
                if (instance.m_fromNestedStateId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toNestedStateId":
            case "toNestedStateId":
            {
                if (instance.m_toNestedStateId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_priority":
            case "priority":
            {
                if (instance.m_priority is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((short)instance.m_flags is TGet shortValue)
                {
                    value = shortValue;
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
            case "m_triggerInterval":
            case "triggerInterval":
            {
                if (value is not hkbStateMachine.TimeInterval castValue) return false;
                instance.m_triggerInterval = castValue;
                return true;
            }
            case "m_initiateInterval":
            case "initiateInterval":
            {
                if (value is not hkbStateMachine.TimeInterval castValue) return false;
                instance.m_initiateInterval = castValue;
                return true;
            }
            case "m_transition":
            case "transition":
            {
                if (value is null)
                {
                    instance.m_transition = default;
                    return true;
                }
                if (value is hkbTransitionEffect castValue)
                {
                    instance.m_transition = castValue;
                    return true;
                }
                return false;
            }
            case "m_condition":
            case "condition":
            {
                if (value is null)
                {
                    instance.m_condition = default;
                    return true;
                }
                if (value is hkbCondition castValue)
                {
                    instance.m_condition = castValue;
                    return true;
                }
                return false;
            }
            case "m_eventId":
            case "eventId":
            {
                if (value is not int castValue) return false;
                instance.m_eventId = castValue;
                return true;
            }
            case "m_toStateId":
            case "toStateId":
            {
                if (value is not int castValue) return false;
                instance.m_toStateId = castValue;
                return true;
            }
            case "m_fromNestedStateId":
            case "fromNestedStateId":
            {
                if (value is not int castValue) return false;
                instance.m_fromNestedStateId = castValue;
                return true;
            }
            case "m_toNestedStateId":
            case "toNestedStateId":
            {
                if (value is not int castValue) return false;
                instance.m_toNestedStateId = castValue;
                return true;
            }
            case "m_priority":
            case "priority":
            {
                if (value is not short castValue) return false;
                instance.m_priority = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is hkbStateMachine.TransitionInfo.TransitionFlags castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is short shortValue)
                {
                    instance.m_flags = (hkbStateMachine.TransitionInfo.TransitionFlags)shortValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
