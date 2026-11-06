// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkVariableTweakingHelperIntVariableInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkVariableTweakingHelper.IntVariableInfo>
{
    public hkVariableTweakingHelperIntVariableInfoData(Havok.hk2014.hkVariableTweakingHelper.IntVariableInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_value":
            case "value":
            {
                value = instance.m_value;
                return true;
            }
            case "m_tweakOn":
            case "tweakOn":
            {
                value = instance.m_tweakOn;
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
            case "m_value":
            case "value":
            {
                if (value is not int castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            case "m_tweakOn":
            case "tweakOn":
            {
                if (value is not bool castValue) return false;
                instance.m_tweakOn = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
