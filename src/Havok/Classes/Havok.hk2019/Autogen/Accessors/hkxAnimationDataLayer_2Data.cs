// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxAnimationDataLayer_2Data : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxAnimationData.Layer_2>
{
    public hkxAnimationDataLayer_2Data(Havok.hk2019.hkxAnimationData.Layer_2 instance) : base(instance) { }

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
            case "m_curves":
            case "curves":
            {
                value = instance.m_curves;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
                return true;
            }
            case "m_isMute":
            case "isMute":
            {
                value = instance.m_isMute;
                return true;
            }
            case "m_isSolo":
            case "isSolo":
            {
                value = instance.m_isSolo;
                return true;
            }
            case "m_isLocked":
            case "isLocked":
            {
                value = instance.m_isLocked;
                return true;
            }
            case "m_color":
            case "color":
            {
                value = instance.m_color;
                return true;
            }
            case "m_blendMode":
            case "blendMode":
            {
                value = instance.m_blendMode;
                return true;
            }
            case "m_rotationAccumulationMode":
            case "rotationAccumulationMode":
            {
                value = instance.m_rotationAccumulationMode;
                return true;
            }
            case "m_scaleAccumulationMode":
            case "scaleAccumulationMode":
            {
                value = instance.m_scaleAccumulationMode;
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
            case "m_curves":
            case "curves":
            {
                if (value is not List<Havok.hk2019.hkxAnimationData.Curve> castValue) return false;
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
                if (value is Havok.hk2019.hkxAnimationData.Layer.BlendMode castValue)
                {
                    instance.m_blendMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_blendMode = (Havok.hk2019.hkxAnimationData.Layer.BlendMode)uintValue;
                    return true;
                }
                return false;
            }
            case "m_rotationAccumulationMode":
            case "rotationAccumulationMode":
            {
                if (value is Havok.hk2019.hkxAnimationData.Layer.RotationAccumulationMode castValue)
                {
                    instance.m_rotationAccumulationMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_rotationAccumulationMode = (Havok.hk2019.hkxAnimationData.Layer.RotationAccumulationMode)uintValue;
                    return true;
                }
                return false;
            }
            case "m_scaleAccumulationMode":
            case "scaleAccumulationMode":
            {
                if (value is Havok.hk2019.hkxAnimationData.Layer.ScaleAccumulationMode castValue)
                {
                    instance.m_scaleAccumulationMode = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_scaleAccumulationMode = (Havok.hk2019.hkxAnimationData.Layer.ScaleAccumulationMode)uintValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
