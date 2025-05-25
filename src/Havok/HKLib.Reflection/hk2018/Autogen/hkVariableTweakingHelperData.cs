// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkVariableTweakingHelperData : HavokData<hkVariableTweakingHelper> 
{
    public hkVariableTweakingHelperData(HavokType type, hkVariableTweakingHelper instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boolVariableInfo":
            case "boolVariableInfo":
            {
                if (instance.m_boolVariableInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_intVariableInfo":
            case "intVariableInfo":
            {
                if (instance.m_intVariableInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_realVariableInfo":
            case "realVariableInfo":
            {
                if (instance.m_realVariableInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vector4VariableInfo":
            case "vector4VariableInfo":
            {
                if (instance.m_vector4VariableInfo is not TGet castValue) return false;
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
            case "m_boolVariableInfo":
            case "boolVariableInfo":
            {
                if (value is not List<hkVariableTweakingHelper.BoolVariableInfo> castValue) return false;
                instance.m_boolVariableInfo = castValue;
                return true;
            }
            case "m_intVariableInfo":
            case "intVariableInfo":
            {
                if (value is not List<hkVariableTweakingHelper.IntVariableInfo> castValue) return false;
                instance.m_intVariableInfo = castValue;
                return true;
            }
            case "m_realVariableInfo":
            case "realVariableInfo":
            {
                if (value is not List<hkVariableTweakingHelper.RealVariableInfo> castValue) return false;
                instance.m_realVariableInfo = castValue;
                return true;
            }
            case "m_vector4VariableInfo":
            case "vector4VariableInfo":
            {
                if (value is not List<hkVariableTweakingHelper.Vector4VariableInfo> castValue) return false;
                instance.m_vector4VariableInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
