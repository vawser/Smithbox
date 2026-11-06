// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbParametricMotionGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbParametricMotionGenerator>
{
    public hkbParametricMotionGeneratorData(Havok.hk2014.hkbParametricMotionGenerator instance) : base(instance) { }

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
            case "m_motionSpace":
            case "motionSpace":
            {
                value = instance.m_motionSpace;
                return true;
            }
            case "m_generators":
            case "generators":
            {
                value = instance.m_generators;
                return true;
            }
            case "m_xAxisParameterValue":
            case "xAxisParameterValue":
            {
                value = instance.m_xAxisParameterValue;
                return true;
            }
            case "m_yAxisParameterValue":
            case "yAxisParameterValue":
            {
                value = instance.m_yAxisParameterValue;
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
            case "m_motionSpace":
            case "motionSpace":
            {
                if (value is Havok.hk2014.hkbParametricMotionGenerator.MotionSpaceType castValue)
                {
                    instance.m_motionSpace = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_motionSpace = (Havok.hk2014.hkbParametricMotionGenerator.MotionSpaceType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_generators":
            case "generators":
            {
                if (value is not List<Havok.hk2014.hkbGenerator?> castValue) return false;
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
