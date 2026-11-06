// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkTimerVolumeUtilsPointData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkTimerVolumeUtils.Point>
{
    public hkTimerVolumeUtilsPointData(Havok.hk2019.hkTimerVolumeUtils.Point instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
