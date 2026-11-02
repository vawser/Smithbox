// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxLightData : HavokData<HKLib.hk2019.hkxLight> 
{
    public hkxLightData(HavokType type, HKLib.hk2019.hkxLight instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_type is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_direction":
            case "direction":
            {
                if (instance.m_direction is not TGet castValue) return false;
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
            case "m_innerAngle":
            case "innerAngle":
            {
                if (instance.m_innerAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_outerAngle":
            case "outerAngle":
            {
                if (instance.m_outerAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_range":
            case "range":
            {
                if (instance.m_range is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fadeStart":
            case "fadeStart":
            {
                if (instance.m_fadeStart is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fadeEnd":
            case "fadeEnd":
            {
                if (instance.m_fadeEnd is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_decayRate":
            case "decayRate":
            {
                if (instance.m_decayRate is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_intensity":
            case "intensity":
            {
                if (instance.m_intensity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shadowCaster":
            case "shadowCaster":
            {
                if (instance.m_shadowCaster is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is HKLib.hk2019.hkxLight.LightType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (HKLib.hk2019.hkxLight.LightType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_direction":
            case "direction":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_direction = castValue;
                return true;
            }
            case "m_color":
            case "color":
            {
                if (value is not Color castValue) return false;
                instance.m_color = castValue;
                return true;
            }
            case "m_innerAngle":
            case "innerAngle":
            {
                if (value is not float castValue) return false;
                instance.m_innerAngle = castValue;
                return true;
            }
            case "m_outerAngle":
            case "outerAngle":
            {
                if (value is not float castValue) return false;
                instance.m_outerAngle = castValue;
                return true;
            }
            case "m_range":
            case "range":
            {
                if (value is not float castValue) return false;
                instance.m_range = castValue;
                return true;
            }
            case "m_fadeStart":
            case "fadeStart":
            {
                if (value is not float castValue) return false;
                instance.m_fadeStart = castValue;
                return true;
            }
            case "m_fadeEnd":
            case "fadeEnd":
            {
                if (value is not float castValue) return false;
                instance.m_fadeEnd = castValue;
                return true;
            }
            case "m_decayRate":
            case "decayRate":
            {
                if (value is not short castValue) return false;
                instance.m_decayRate = castValue;
                return true;
            }
            case "m_intensity":
            case "intensity":
            {
                if (value is not float castValue) return false;
                instance.m_intensity = castValue;
                return true;
            }
            case "m_shadowCaster":
            case "shadowCaster":
            {
                if (value is not bool castValue) return false;
                instance.m_shadowCaster = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
