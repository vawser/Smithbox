// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkVariableTweakingHelperData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkVariableTweakingHelper>
{
    public hkVariableTweakingHelperData(Havok.hk2018.hkVariableTweakingHelper instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boolVariableInfo":
            case "boolVariableInfo":
            {
                value = instance.m_boolVariableInfo;
                return true;
            }
            case "m_intVariableInfo":
            case "intVariableInfo":
            {
                value = instance.m_intVariableInfo;
                return true;
            }
            case "m_realVariableInfo":
            case "realVariableInfo":
            {
                value = instance.m_realVariableInfo;
                return true;
            }
            case "m_vector4VariableInfo":
            case "vector4VariableInfo":
            {
                value = instance.m_vector4VariableInfo;
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
            case "m_boolVariableInfo":
            case "boolVariableInfo":
            {
                if (value is not List<Havok.hk2018.hkVariableTweakingHelper.BoolVariableInfo> castValue) return false;
                instance.m_boolVariableInfo = castValue;
                return true;
            }
            case "m_intVariableInfo":
            case "intVariableInfo":
            {
                if (value is not List<Havok.hk2018.hkVariableTweakingHelper.IntVariableInfo> castValue) return false;
                instance.m_intVariableInfo = castValue;
                return true;
            }
            case "m_realVariableInfo":
            case "realVariableInfo":
            {
                if (value is not List<Havok.hk2018.hkVariableTweakingHelper.RealVariableInfo> castValue) return false;
                instance.m_realVariableInfo = castValue;
                return true;
            }
            case "m_vector4VariableInfo":
            case "vector4VariableInfo":
            {
                if (value is not List<Havok.hk2018.hkVariableTweakingHelper.Vector4VariableInfo> castValue) return false;
                instance.m_vector4VariableInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
