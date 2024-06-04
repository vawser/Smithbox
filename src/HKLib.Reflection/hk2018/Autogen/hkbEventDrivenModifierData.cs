// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbEventDrivenModifierData : HavokData<hkbEventDrivenModifier> 
{
    public hkbEventDrivenModifierData(HavokType type, hkbEventDrivenModifier instance) : base(type, instance) {}

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
            case "m_enable":
            case "enable":
            {
                if (instance.m_enable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_modifier":
            case "modifier":
            {
                if (instance.m_modifier is null)
                {
                    return true;
                }
                if (instance.m_modifier is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_activateEventId":
            case "activateEventId":
            {
                if (instance.m_activateEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_deactivateEventId":
            case "deactivateEventId":
            {
                if (instance.m_deactivateEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_activeByDefault":
            case "activeByDefault":
            {
                if (instance.m_activeByDefault is not TGet castValue) return false;
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
                if (value is hkbModifier castValue)
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
