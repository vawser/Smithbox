// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxAnimationDataCurveKeyData : HavokData<HKLib.hk2019.hkxAnimationData.CurveKey> 
{
    private static readonly System.Reflection.FieldInfo _dataInfo = typeof(HKLib.hk2019.hkxAnimationData.CurveKey).GetField("m_data")!;
    public hkxAnimationDataCurveKeyData(HavokType type, HKLib.hk2019.hkxAnimationData.CurveKey instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_value":
            case "value":
            {
                if (instance.m_value is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_time":
            case "time":
            {
                if (instance.m_time is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tangentMode":
            case "tangentMode":
            {
                if (instance.m_tangentMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_tangentMode is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_interpolationType":
            case "interpolationType":
            {
                if (instance.m_interpolationType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_interpolationType is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_weightedMode":
            case "weightedMode":
            {
                if (instance.m_weightedMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_weightedMode is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_constantMode":
            case "constantMode":
            {
                if (instance.m_constantMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_constantMode is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_velocityMode":
            case "velocityMode":
            {
                if (instance.m_velocityMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_velocityMode is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_tangentBreak":
            case "tangentBreak":
            {
                if (instance.m_tangentBreak is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tangentVisibility":
            case "tangentVisibility":
            {
                if (instance.m_tangentVisibility is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_tangentVisibility is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkxAnimationData.TangentMode castValue)
                {
                    instance.m_tangentMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_tangentMode = (HKLib.hk2019.hkxAnimationData.TangentMode)uintValue;
                    return true;
                }
                return false;
            }
            case "m_interpolationType":
            case "interpolationType":
            {
                if (value is HKLib.hk2019.hkxAnimationData.InterpolationType castValue)
                {
                    instance.m_interpolationType = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_interpolationType = (HKLib.hk2019.hkxAnimationData.InterpolationType)uintValue;
                    return true;
                }
                return false;
            }
            case "m_weightedMode":
            case "weightedMode":
            {
                if (value is HKLib.hk2019.hkxAnimationData.WeightedMode castValue)
                {
                    instance.m_weightedMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_weightedMode = (HKLib.hk2019.hkxAnimationData.WeightedMode)uintValue;
                    return true;
                }
                return false;
            }
            case "m_constantMode":
            case "constantMode":
            {
                if (value is HKLib.hk2019.hkxAnimationData.ConstantMode castValue)
                {
                    instance.m_constantMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_constantMode = (HKLib.hk2019.hkxAnimationData.ConstantMode)uintValue;
                    return true;
                }
                return false;
            }
            case "m_velocityMode":
            case "velocityMode":
            {
                if (value is HKLib.hk2019.hkxAnimationData.VelocityMode castValue)
                {
                    instance.m_velocityMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_velocityMode = (HKLib.hk2019.hkxAnimationData.VelocityMode)uintValue;
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
                if (value is HKLib.hk2019.hkxAnimationData.TangentVisibility castValue)
                {
                    instance.m_tangentVisibility = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_tangentVisibility = (HKLib.hk2019.hkxAnimationData.TangentVisibility)uintValue;
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
