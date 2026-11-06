// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkxLightData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkxLight>
{
    public hkxLightData(Havok.hk55.hkxLight instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_angle":
            case "angle":
            {
                value = instance.m_angle;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk55.hkxLight.LightType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk55.hkxLight.LightType)sbyteValue;
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
                if (value is not uint castValue) return false;
                instance.m_color = castValue;
                return true;
            }
            case "m_angle":
            case "angle":
            {
                if (value is not float castValue) return false;
                instance.m_angle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
