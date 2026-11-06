// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiVolumeNavigatorGoalData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiVolumeNavigator.Goal>
{
    public hkaiVolumeNavigatorGoalData(Havok.hk2018.hkaiVolumeNavigator.Goal instance) : base(instance) { }

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
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
