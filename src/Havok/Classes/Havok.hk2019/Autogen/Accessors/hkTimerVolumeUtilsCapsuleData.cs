// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkTimerVolumeUtilsCapsuleData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkTimerVolumeUtils.Capsule>
{
    public hkTimerVolumeUtilsCapsuleData(Havok.hk2019.hkTimerVolumeUtils.Capsule instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_start":
            case "start":
            {
                value = instance.m_start;
                return true;
            }
            case "m_end":
            case "end":
            {
                value = instance.m_end;
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
            case "m_start":
            case "start":
            {
                if (value is not Havok.hk2019.hkFloat3 castValue) return false;
                instance.m_start = castValue;
                return true;
            }
            case "m_end":
            case "end":
            {
                if (value is not Havok.hk2019.hkFloat3 castValue) return false;
                instance.m_end = castValue;
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
