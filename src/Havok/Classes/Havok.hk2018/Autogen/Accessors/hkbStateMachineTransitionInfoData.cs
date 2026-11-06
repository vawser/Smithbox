// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbStateMachineTransitionInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbStateMachine.TransitionInfo>
{
    public hkbStateMachineTransitionInfoData(Havok.hk2018.hkbStateMachine.TransitionInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_triggerInterval":
            case "triggerInterval":
            {
                value = instance.m_triggerInterval;
                return true;
            }
            case "m_initiateInterval":
            case "initiateInterval":
            {
                value = instance.m_initiateInterval;
                return true;
            }
            case "m_transition":
            case "transition":
            {
                value = instance.m_transition;
                return true;
            }
            case "m_condition":
            case "condition":
            {
                value = instance.m_condition;
                return true;
            }
            case "m_eventId":
            case "eventId":
            {
                value = instance.m_eventId;
                return true;
            }
            case "m_toStateId":
            case "toStateId":
            {
                value = instance.m_toStateId;
                return true;
            }
            case "m_fromNestedStateId":
            case "fromNestedStateId":
            {
                value = instance.m_fromNestedStateId;
                return true;
            }
            case "m_toNestedStateId":
            case "toNestedStateId":
            {
                value = instance.m_toNestedStateId;
                return true;
            }
            case "m_priority":
            case "priority":
            {
                value = instance.m_priority;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
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
            case "m_triggerInterval":
            case "triggerInterval":
            {
                if (value is not Havok.hk2018.hkbStateMachine.TimeInterval castValue) return false;
                instance.m_triggerInterval = castValue;
                return true;
            }
            case "m_initiateInterval":
            case "initiateInterval":
            {
                if (value is not Havok.hk2018.hkbStateMachine.TimeInterval castValue) return false;
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
                if (value is Havok.hk2018.hkbTransitionEffect castValue)
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
                if (value is Havok.hk2018.hkbCondition castValue)
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
                if (value is Havok.hk2018.hkbStateMachine.TransitionInfo.TransitionFlags castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is short shortValue)
                {
                    instance.m_flags = (Havok.hk2018.hkbStateMachine.TransitionInfo.TransitionFlags)shortValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
