// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkTimerVolumeUtilsSphereData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkTimerVolumeUtils.Sphere>
{
    public hkTimerVolumeUtilsSphereData(Havok.hk2019.hkTimerVolumeUtils.Sphere instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
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
            case "m_position":
            case "position":
            {
                if (value is not Havok.hk2019.hkFloat3 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
