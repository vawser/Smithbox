// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class CustomTimeActGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.CustomTimeActGenerator>
{
    public CustomTimeActGeneratorData(Havok.hk2014.CustomTimeActGenerator instance) : base(instance) { }

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
            case "m_generator":
            case "generator":
            {
                value = instance.m_generator;
                return true;
            }
            case "m_offsetType":
            case "offsetType":
            {
                value = instance.m_offsetType;
                return true;
            }
            case "m_taeId":
            case "taeId":
            {
                value = instance.m_taeId;
                return true;
            }
            case "m_valIndex":
            case "valIndex":
            {
                value = instance.m_valIndex;
                return true;
            }
            case "m_valRate":
            case "valRate":
            {
                value = instance.m_valRate;
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
            case "m_generator":
            case "generator":
            {
                if (value is null)
                {
                    instance.m_generator = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbGenerator castValue)
                {
                    instance.m_generator = castValue;
                    return true;
                }
                return false;
            }
            case "m_offsetType":
            case "offsetType":
            {
                if (value is Havok.hk2014.CustomTimeActGenerator.OffsetType castValue)
                {
                    instance.m_offsetType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_offsetType = (Havok.hk2014.CustomTimeActGenerator.OffsetType)intValue;
                    return true;
                }
                return false;
            }
            case "m_taeId":
            case "taeId":
            {
                if (value is not int castValue) return false;
                instance.m_taeId = castValue;
                return true;
            }
            case "m_valIndex":
            case "valIndex":
            {
                if (value is not int castValue) return false;
                instance.m_valIndex = castValue;
                return true;
            }
            case "m_valRate":
            case "valRate":
            {
                if (value is not float castValue) return false;
                instance.m_valRate = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
