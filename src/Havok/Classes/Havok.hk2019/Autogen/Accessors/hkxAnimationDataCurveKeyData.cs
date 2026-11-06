// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxAnimationDataCurveKeyData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxAnimationData.CurveKey>
{
    private static readonly System.Reflection.FieldInfo _dataInfo = typeof(Havok.hk2019.hkxAnimationData.CurveKey).GetField("m_data")!;
    public hkxAnimationDataCurveKeyData(Havok.hk2019.hkxAnimationData.CurveKey instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_value":
            case "value":
            {
                value = instance.m_value;
                return true;
            }
            case "m_time":
            case "time":
            {
                value = instance.m_time;
                return true;
            }
            case "m_tangentMode":
            case "tangentMode":
            {
                value = instance.m_tangentMode;
                return true;
            }
            case "m_interpolationType":
            case "interpolationType":
            {
                value = instance.m_interpolationType;
                return true;
            }
            case "m_weightedMode":
            case "weightedMode":
            {
                value = instance.m_weightedMode;
                return true;
            }
            case "m_constantMode":
            case "constantMode":
            {
                value = instance.m_constantMode;
                return true;
            }
            case "m_velocityMode":
            case "velocityMode":
            {
                value = instance.m_velocityMode;
                return true;
            }
            case "m_tangentBreak":
            case "tangentBreak":
            {
                value = instance.m_tangentBreak;
                return true;
            }
            case "m_tangentVisibility":
            case "tangentVisibility":
            {
                value = instance.m_tangentVisibility;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
            case "m_value":
            case "value":
            {
                if (value is not double castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            case "m_time":
            case "time":
            {
                if (value is not double castValue) return false;
                instance.m_time = castValue;
                return true;
            }
            case "m_tangentMode":
            case "tangentMode":
            {
                if (value is Havok.hk2019.hkxAnimationData.TangentMode castValue)
                {
                    instance.m_tangentMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_tangentMode = (Havok.hk2019.hkxAnimationData.TangentMode)uintValue;
                    return true;
                }
                return false;
            }
            case "m_interpolationType":
            case "interpolationType":
            {
                if (value is Havok.hk2019.hkxAnimationData.InterpolationType castValue)
                {
                    instance.m_interpolationType = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_interpolationType = (Havok.hk2019.hkxAnimationData.InterpolationType)uintValue;
                    return true;
                }
                return false;
            }
            case "m_weightedMode":
            case "weightedMode":
            {
                if (value is Havok.hk2019.hkxAnimationData.WeightedMode castValue)
                {
                    instance.m_weightedMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_weightedMode = (Havok.hk2019.hkxAnimationData.WeightedMode)uintValue;
                    return true;
                }
                return false;
            }
            case "m_constantMode":
            case "constantMode":
            {
                if (value is Havok.hk2019.hkxAnimationData.ConstantMode castValue)
                {
                    instance.m_constantMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_constantMode = (Havok.hk2019.hkxAnimationData.ConstantMode)uintValue;
                    return true;
                }
                return false;
            }
            case "m_velocityMode":
            case "velocityMode":
            {
                if (value is Havok.hk2019.hkxAnimationData.VelocityMode castValue)
                {
                    instance.m_velocityMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_velocityMode = (Havok.hk2019.hkxAnimationData.VelocityMode)uintValue;
                    return true;
                }
                return false;
            }
            case "m_tangentBreak":
            case "tangentBreak":
            {
                if (value is not bool castValue) return false;
                instance.m_tangentBreak = castValue;
                return true;
            }
            case "m_tangentVisibility":
            case "tangentVisibility":
            {
                if (value is Havok.hk2019.hkxAnimationData.TangentVisibility castValue)
                {
                    instance.m_tangentVisibility = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_tangentVisibility = (Havok.hk2019.hkxAnimationData.TangentVisibility)uintValue;
                    return true;
                }
                return false;
            }
            case "m_data":
            case "data":
            {
                if (value is not double[] castValue || castValue.Length != 6) return false;
                try
                {
                    _dataInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
