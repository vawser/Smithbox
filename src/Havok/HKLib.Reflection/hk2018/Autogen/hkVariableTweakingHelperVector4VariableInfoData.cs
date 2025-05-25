// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkVariableTweakingHelperVector4VariableInfoData : HavokData<hkVariableTweakingHelper.Vector4VariableInfo> 
{
    public hkVariableTweakingHelperVector4VariableInfoData(HavokType type, hkVariableTweakingHelper.Vector4VariableInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_x":
            case "x":
            {
                if (instance.m_x is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_y":
            case "y":
            {
                if (instance.m_y is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_z":
            case "z":
            {
                if (instance.m_z is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_w":
            case "w":
            {
                if (instance.m_w is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tweakOn":
            case "tweakOn":
            {
                if (instance.m_tweakOn is not TGet castValue) return false;
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
