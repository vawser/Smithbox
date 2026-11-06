// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkVariableTweakingHelperVector4VariableInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkVariableTweakingHelper.Vector4VariableInfo>
{
    public hkVariableTweakingHelperVector4VariableInfoData(Havok.hk2014.hkVariableTweakingHelper.Vector4VariableInfo instance) : base(instance) { }

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
            case "m_x":
            case "x":
            {
                value = instance.m_x;
                return true;
            }
            case "m_y":
            case "y":
            {
                value = instance.m_y;
                return true;
            }
            case "m_z":
            case "z":
            {
                value = instance.m_z;
                return true;
            }
            case "m_w":
            case "w":
            {
                value = instance.m_w;
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
            case "m_x":
            case "x":
            {
                if (value is not float castValue) return false;
                instance.m_x = castValue;
                return true;
            }
            case "m_y":
            case "y":
            {
                if (value is not float castValue) return false;
                instance.m_y = castValue;
                return true;
            }
            case "m_z":
            case "z":
            {
                if (value is not float castValue) return false;
                instance.m_z = castValue;
                return true;
            }
            case "m_w":
            case "w":
            {
                if (value is not float castValue) return false;
                instance.m_w = castValue;
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
