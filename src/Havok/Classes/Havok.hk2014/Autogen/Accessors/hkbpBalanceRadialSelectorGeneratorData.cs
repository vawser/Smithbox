// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpBalanceRadialSelectorGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpBalanceRadialSelectorGenerator>
{
    public hkbpBalanceRadialSelectorGeneratorData(Havok.hk2014.hkbpBalanceRadialSelectorGenerator instance) : base(instance) { }

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
            case "m_generatorPairs":
            case "generatorPairs":
            {
                value = instance.m_generatorPairs;
                return true;
            }
            case "m_angle":
            case "angle":
            {
                value = instance.m_angle;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_xAxisMS":
            case "xAxisMS":
            {
                value = instance.m_xAxisMS;
                return true;
            }
            case "m_yAxisMS":
            case "yAxisMS":
            {
                value = instance.m_yAxisMS;
                return true;
            }
            case "m_checkBalanceModifier":
            case "checkBalanceModifier":
            {
                value = instance.m_checkBalanceModifier;
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
            case "m_generatorPairs":
            case "generatorPairs":
            {
                if (value is not List<Havok.hk2014.hkbRadialSelectorGenerator.GeneratorPair> castValue) return false;
                instance.m_generatorPairs = castValue;
                return true;
            }
            case "m_angle":
            case "angle":
            {
                if (value is not float castValue) return false;
                instance.m_angle = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_xAxisMS":
            case "xAxisMS":
            {
                if (value is not int castValue) return false;
                instance.m_xAxisMS = castValue;
                return true;
            }
            case "m_yAxisMS":
            case "yAxisMS":
            {
                if (value is not int castValue) return false;
                instance.m_yAxisMS = castValue;
                return true;
            }
            case "m_checkBalanceModifier":
            case "checkBalanceModifier":
            {
                if (value is null)
                {
                    instance.m_checkBalanceModifier = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbpCheckBalanceModifier castValue)
                {
                    instance.m_checkBalanceModifier = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
