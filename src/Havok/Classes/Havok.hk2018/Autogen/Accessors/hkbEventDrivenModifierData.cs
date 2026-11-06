// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbEventDrivenModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbEventDrivenModifier>
{
    public hkbEventDrivenModifierData(Havok.hk2018.hkbEventDrivenModifier instance) : base(instance) { }

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
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_modifier":
            case "modifier":
            {
                value = instance.m_modifier;
                return true;
            }
            case "m_activateEventId":
            case "activateEventId":
            {
                value = instance.m_activateEventId;
                return true;
            }
            case "m_deactivateEventId":
            case "deactivateEventId":
            {
                value = instance.m_deactivateEventId;
                return true;
            }
            case "m_activeByDefault":
            case "activeByDefault":
            {
                value = instance.m_activeByDefault;
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
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
                if (value is Havok.hk2018.hkbVariableBindingSet castValue)
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
            case "m_modifier":
            case "modifier":
            {
                if (value is null)
                {
                    instance.m_modifier = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbModifier castValue)
                {
                    instance.m_modifier = castValue;
                    return true;
                }
                return false;
            }
            case "m_activateEventId":
            case "activateEventId":
            {
                if (value is not int castValue) return false;
                instance.m_activateEventId = castValue;
                return true;
            }
            case "m_deactivateEventId":
            case "deactivateEventId":
            {
                if (value is not int castValue) return false;
                instance.m_deactivateEventId = castValue;
                return true;
            }
            case "m_activeByDefault":
            case "activeByDefault":
            {
                if (value is not bool castValue) return false;
                instance.m_activeByDefault = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
