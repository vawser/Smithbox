// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbJigglerModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbJigglerModifier>
{
    public hkbJigglerModifierData(Havok.hk2018.hkbJigglerModifier instance) : base(instance) { }

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
            case "m_jigglerGroups":
            case "jigglerGroups":
            {
                value = instance.m_jigglerGroups;
                return true;
            }
            case "m_jiggleCoordinates":
            case "jiggleCoordinates":
            {
                value = instance.m_jiggleCoordinates;
                return true;
            }
            case "m_offGain":
            case "offGain":
            {
                value = instance.m_offGain;
                return true;
            }
            case "m_onGain":
            case "onGain":
            {
                value = instance.m_onGain;
                return true;
            }
            case "m_isOn":
            case "isOn":
            {
                value = instance.m_isOn;
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
            case "m_jigglerGroups":
            case "jigglerGroups":
            {
                if (value is not List<Havok.hk2018.hkbJigglerGroup?> castValue) return false;
                instance.m_jigglerGroups = castValue;
                return true;
            }
            case "m_jiggleCoordinates":
            case "jiggleCoordinates":
            {
                if (value is Havok.hk2018.hkbJigglerModifier.JiggleCoordinates castValue)
                {
                    instance.m_jiggleCoordinates = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_jiggleCoordinates = (Havok.hk2018.hkbJigglerModifier.JiggleCoordinates)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_offGain":
            case "offGain":
            {
                if (value is not float castValue) return false;
                instance.m_offGain = castValue;
                return true;
            }
            case "m_onGain":
            case "onGain":
            {
                if (value is not float castValue) return false;
                instance.m_onGain = castValue;
                return true;
            }
            case "m_isOn":
            case "isOn":
            {
                if (value is not bool castValue) return false;
                instance.m_isOn = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
