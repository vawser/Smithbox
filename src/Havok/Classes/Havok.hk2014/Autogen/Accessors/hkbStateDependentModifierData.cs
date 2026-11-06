// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbStateDependentModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbStateDependentModifier>
{
    public hkbStateDependentModifierData(Havok.hk2014.hkbStateDependentModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_applyModifierDuringTransition":
            case "applyModifierDuringTransition":
            {
                value = instance.m_applyModifierDuringTransition;
                return true;
            }
            case "m_stateIds":
            case "stateIds":
            {
                value = instance.m_stateIds;
                return true;
            }
            case "m_modifier":
            case "modifier":
            {
                value = instance.m_modifier;
                return true;
            }
            case "m_isActive":
            case "isActive":
            {
                value = instance.m_isActive;
                return true;
            }
            case "m_stateMachine":
            case "stateMachine":
            {
                value = instance.m_stateMachine;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
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
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_applyModifierDuringTransition":
            case "applyModifierDuringTransition":
            {
                if (value is not bool castValue) return false;
                instance.m_applyModifierDuringTransition = castValue;
                return true;
            }
            case "m_stateIds":
            case "stateIds":
            {
                if (value is not List<int> castValue) return false;
                instance.m_stateIds = castValue;
                return true;
            }
            case "m_modifier":
            case "modifier":
            {
                if (value is null)
                {
                    instance.m_modifier = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbModifier castValue)
                {
                    instance.m_modifier = castValue;
                    return true;
                }
                return false;
            }
            case "m_isActive":
            case "isActive":
            {
                if (value is not bool castValue) return false;
                instance.m_isActive = castValue;
                return true;
            }
            case "m_stateMachine":
            case "stateMachine":
            {
                if (value is null)
                {
                    instance.m_stateMachine = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbStateMachine castValue)
                {
                    instance.m_stateMachine = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
