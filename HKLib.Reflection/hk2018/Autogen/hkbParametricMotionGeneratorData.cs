// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbParametricMotionGeneratorData : HavokData<hkbParametricMotionGenerator> 
{
    public hkbParametricMotionGeneratorData(HavokType type, hkbParametricMotionGenerator instance) : base(type, instance) {}

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
            case "m_motionSpace":
            case "motionSpace":
            {
                if (instance.m_motionSpace is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_motionSpace is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_generators":
            case "generators":
            {
                if (instance.m_generators is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_xAxisParameterValue":
            case "xAxisParameterValue":
            {
                if (instance.m_xAxisParameterValue is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_yAxisParameterValue":
            case "yAxisParameterValue":
            {
                if (instance.m_yAxisParameterValue is not TGet castValue) return false;
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
            case "m_motionSpace":
            case "motionSpace":
            {
                if (value is hkbParametricMotionGenerator.MotionSpaceType castValue)
                {
                    instance.m_motionSpace = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_motionSpace = (hkbParametricMotionGenerator.MotionSpaceType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_generators":
            case "generators":
            {
                if (value is not List<hkbGenerator?> castValue) return false;
                instance.m_generators = castValue;
                return true;
            }
            case "m_xAxisParameterValue":
            case "xAxisParameterValue":
            {
                if (value is not float castValue) return false;
                instance.m_xAxisParameterValue = castValue;
                return true;
            }
            case "m_yAxisParameterValue":
            case "yAxisParameterValue":
            {
                if (value is not float castValue) return false;
                instance.m_yAxisParameterValue = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
