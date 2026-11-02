// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxAnimationDataLayer_2Data : HavokData<HKLib.hk2019.hkxAnimationData.Layer_2> 
{
    public hkxAnimationDataLayer_2Data(HavokType type, HKLib.hk2019.hkxAnimationData.Layer_2 instance) : base(type, instance) {}

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
            case "m_curves":
            case "curves":
            {
                if (instance.m_curves is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (instance.m_weight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isMute":
            case "isMute":
            {
                if (instance.m_isMute is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isSolo":
            case "isSolo":
            {
                if (instance.m_isSolo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isLocked":
            case "isLocked":
            {
                if (instance.m_isLocked is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_color":
            case "color":
            {
                if (instance.m_color is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendMode":
            case "blendMode":
            {
                if (instance.m_blendMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_blendMode is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_rotationAccumulationMode":
            case "rotationAccumulationMode":
            {
                if (instance.m_rotationAccumulationMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_rotationAccumulationMode is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_scaleAccumulationMode":
            case "scaleAccumulationMode":
            {
                if (instance.m_scaleAccumulationMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_scaleAccumulationMode is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
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
            case "m_curves":
            case "curves":
            {
                if (value is not List<HKLib.hk2019.hkxAnimationData.Curve> castValue) return false;
                instance.m_curves = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (value is not double castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            case "m_isMute":
            case "isMute":
            {
                if (value is not bool castValue) return false;
                instance.m_isMute = castValue;
                return true;
            }
            case "m_isSolo":
            case "isSolo":
            {
                if (value is not bool castValue) return false;
                instance.m_isSolo = castValue;
                return true;
            }
            case "m_isLocked":
            case "isLocked":
            {
                if (value is not bool castValue) return false;
                instance.m_isLocked = castValue;
                return true;
            }
            case "m_color":
            case "color":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_color = castValue;
                return true;
            }
            case "m_blendMode":
            case "blendMode":
            {
                if (value is HKLib.hk2019.hkxAnimationData.Layer.BlendMode castValue)
                {
                    instance.m_blendMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_blendMode = (HKLib.hk2019.hkxAnimationData.Layer.BlendMode)uintValue;
                    return true;
                }
                return false;
            }
            case "m_rotationAccumulationMode":
            case "rotationAccumulationMode":
            {
                if (value is HKLib.hk2019.hkxAnimationData.Layer.RotationAccumulationMode castValue)
                {
                    instance.m_rotationAccumulationMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_rotationAccumulationMode = (HKLib.hk2019.hkxAnimationData.Layer.RotationAccumulationMode)uintValue;
                    return true;
                }
                return false;
            }
            case "m_scaleAccumulationMode":
            case "scaleAccumulationMode":
            {
                if (value is HKLib.hk2019.hkxAnimationData.Layer.ScaleAccumulationMode castValue)
                {
                    instance.m_scaleAccumulationMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_scaleAccumulationMode = (HKLib.hk2019.hkxAnimationData.Layer.ScaleAccumulationMode)uintValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
