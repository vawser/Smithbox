// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxLightData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxLight>
{
    public hkxLightData(Havok.hk2018.hkxLight instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_direction":
            case "direction":
            {
                value = instance.m_direction;
                return true;
            }
            case "m_color":
            case "color":
            {
                value = instance.m_color;
                return true;
            }
            case "m_innerAngle":
            case "innerAngle":
            {
                value = instance.m_innerAngle;
                return true;
            }
            case "m_outerAngle":
            case "outerAngle":
            {
                value = instance.m_outerAngle;
                return true;
            }
            case "m_range":
            case "range":
            {
                value = instance.m_range;
                return true;
            }
            case "m_fadeStart":
            case "fadeStart":
            {
                value = instance.m_fadeStart;
                return true;
            }
            case "m_fadeEnd":
            case "fadeEnd":
            {
                value = instance.m_fadeEnd;
                return true;
            }
            case "m_decayRate":
            case "decayRate":
            {
                value = instance.m_decayRate;
                return true;
            }
            case "m_intensity":
            case "intensity":
            {
                value = instance.m_intensity;
                return true;
            }
            case "m_shadowCaster":
            case "shadowCaster":
            {
                value = instance.m_shadowCaster;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2018.hkxLight.LightType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2018.hkxLight.LightType)sbyteValue;
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
