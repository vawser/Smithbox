// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbStateMachineStateInfoData : HavokData<hkbStateMachine.StateInfo> 
{
    public hkbStateMachineStateInfoData(HavokType type, hkbStateMachine.StateInfo instance) : base(type, instance) {}

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
            case "m_listeners":
            case "listeners":
            {
                if (instance.m_listeners is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enterNotifyEvents":
            case "enterNotifyEvents":
            {
                if (instance.m_enterNotifyEvents is null)
                {
                    return true;
                }
                if (instance.m_enterNotifyEvents is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_exitNotifyEvents":
            case "exitNotifyEvents":
            {
                if (instance.m_exitNotifyEvents is null)
                {
                    return true;
                }
                if (instance.m_exitNotifyEvents is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_transitions":
            case "transitions":
            {
                if (instance.m_transitions is null)
                {
                    return true;
                }
                if (instance.m_transitions is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_generator":
            case "generator":
            {
                if (instance.m_generator is null)
                {
                    return true;
                }
                if (instance.m_generator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_stateId":
            case "stateId":
            {
                if (instance.m_stateId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_probability":
            case "probability":
            {
                if (instance.m_probability is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                if (instance.m_enable is not TGet castValue) return false;
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
            case "m_listeners":
            case "listeners":
            {
                if (value is not List<hkbStateListener?> castValue) return false;
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
                if (value is hkbStateMachine.EventPropertyArray castValue)
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
                if (value is hkbStateMachine.EventPropertyArray castValue)
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
                if (value is hkbStateMachine.TransitionInfoArray castValue)
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
                if (value is hkbGenerator castValue)
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
