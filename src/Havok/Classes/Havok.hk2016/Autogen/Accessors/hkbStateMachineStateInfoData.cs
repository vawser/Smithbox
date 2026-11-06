// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbStateMachineStateInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbStateMachine.StateInfo>
{
    public hkbStateMachineStateInfoData(Havok.hk2016.hkbStateMachine.StateInfo instance) : base(instance) { }

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
            case "m_listeners":
            case "listeners":
            {
                value = instance.m_listeners;
                return true;
            }
            case "m_enterNotifyEvents":
            case "enterNotifyEvents":
            {
                value = instance.m_enterNotifyEvents;
                return true;
            }
            case "m_exitNotifyEvents":
            case "exitNotifyEvents":
            {
                value = instance.m_exitNotifyEvents;
                return true;
            }
            case "m_transitions":
            case "transitions":
            {
                value = instance.m_transitions;
                return true;
            }
            case "m_generator":
            case "generator":
            {
                value = instance.m_generator;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_stateId":
            case "stateId":
            {
                value = instance.m_stateId;
                return true;
            }
            case "m_probability":
            case "probability":
            {
                value = instance.m_probability;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
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
            case "m_listeners":
            case "listeners":
            {
                if (value is not List<Havok.hk2016.hkbStateListener?> castValue) return false;
                instance.m_listeners = castValue;
                return true;
            }
            case "m_enterNotifyEvents":
            case "enterNotifyEvents":
            {
                if (value is null)
                {
                    instance.m_enterNotifyEvents = default;
                    return true;
                }
                if (value is Havok.hk2016.hkbStateMachine.EventPropertyArray castValue)
                {
                    instance.m_enterNotifyEvents = castValue;
                    return true;
                }
                return false;
            }
            case "m_exitNotifyEvents":
            case "exitNotifyEvents":
            {
                if (value is null)
                {
                    instance.m_exitNotifyEvents = default;
                    return true;
                }
                if (value is Havok.hk2016.hkbStateMachine.EventPropertyArray castValue)
                {
                    instance.m_exitNotifyEvents = castValue;
                    return true;
                }
                return false;
            }
            case "m_transitions":
            case "transitions":
            {
                if (value is null)
                {
                    instance.m_transitions = default;
                    return true;
                }
                if (value is Havok.hk2016.hkbStateMachine.TransitionInfoArray castValue)
                {
                    instance.m_transitions = castValue;
                    return true;
                }
                return false;
            }
            case "m_generator":
            case "generator":
            {
                if (value is null)
                {
                    instance.m_generator = default;
                    return true;
                }
                if (value is Havok.hk2016.hkbGenerator castValue)
                {
                    instance.m_generator = castValue;
                    return true;
                }
                return false;
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
            case "m_stateId":
            case "stateId":
            {
                if (value is not int castValue) return false;
                instance.m_stateId = castValue;
                return true;
            }
            case "m_probability":
            case "probability":
            {
                if (value is not float castValue) return false;
                instance.m_probability = castValue;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
