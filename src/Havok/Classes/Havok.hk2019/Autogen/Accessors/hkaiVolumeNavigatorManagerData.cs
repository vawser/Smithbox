// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiVolumeNavigatorManagerData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiVolumeNavigatorManager>
{
    public hkaiVolumeNavigatorManagerData(Havok.hk2019.hkaiVolumeNavigatorManager instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_world":
            case "world":
            {
                value = instance.m_world;
                return true;
            }
            case "m_navigators":
            case "navigators":
            {
                value = instance.m_navigators;
                return true;
            }
            case "m_modifiedSections":
            case "modifiedSections":
            {
                value = instance.m_modifiedSections;
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
            case "m_world":
            case "world":
            {
                if (value is null)
                {
                    instance.m_world = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiWorld castValue)
                {
                    instance.m_world = castValue;
                    return true;
                }
                return false;
            }
            case "m_navigators":
            case "navigators":
            {
                if (value is not List<Havok.hk2019.hkaiVolumeNavigator?> castValue) return false;
                instance.m_navigators = castValue;
                return true;
            }
            case "m_modifiedSections":
            case "modifiedSections":
            {
                if (value is null)
                {
                    instance.m_modifiedSections = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiModifiedSections castValue)
                {
                    instance.m_modifiedSections = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
