// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiVolumeNavigatorStalenessCheckerData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiVolumeNavigatorStalenessChecker>
{
    public hkaiVolumeNavigatorStalenessCheckerData(Havok.hk2019.hkaiVolumeNavigatorStalenessChecker instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_navigator":
            case "navigator":
            {
                value = instance.m_navigator;
                return true;
            }
            case "m_startPoint":
            case "startPoint":
            {
                value = instance.m_startPoint;
                return true;
            }
            case "m_goals":
            case "goals":
            {
                value = instance.m_goals;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                value = instance.m_settings;
                return true;
            }
            case "m_pathCellKeys":
            case "pathCellKeys":
            {
                value = instance.m_pathCellKeys;
                return true;
            }
            case "m_trailingCellIdx":
            case "trailingCellIdx":
            {
                value = instance.m_trailingCellIdx;
                return true;
            }
            case "m_state":
            case "state":
            {
                value = instance.m_state;
                return true;
            }
            case "m_pathRequest":
            case "pathRequest":
            {
                value = instance.m_pathRequest;
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
            case "m_navigator":
            case "navigator":
            {
                if (value is null)
                {
                    instance.m_navigator = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiVolumeNavigator castValue)
                {
                    instance.m_navigator = castValue;
                    return true;
                }
                return false;
            }
            case "m_startPoint":
            case "startPoint":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_startPoint = castValue;
                return true;
            }
            case "m_goals":
            case "goals":
            {
                if (value is not List<Havok.hk2019.hkaiVolumeNavigator.Goal> castValue) return false;
                instance.m_goals = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (value is null)
                {
                    instance.m_settings = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiVolumeNavigator.NavigatorSettings castValue)
                {
                    instance.m_settings = castValue;
                    return true;
                }
                return false;
            }
            case "m_pathCellKeys":
            case "pathCellKeys":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_pathCellKeys = castValue;
                return true;
            }
            case "m_trailingCellIdx":
            case "trailingCellIdx":
            {
                if (value is not int castValue) return false;
                instance.m_trailingCellIdx = castValue;
                return true;
            }
            case "m_state":
            case "state":
            {
                if (value is not Havok.hk2019.hkAtomic.Variable<Havok.hk2019.hkaiVolumeNavigatorStalenessChecker.State> castValue) return false;
                instance.m_state = castValue;
                return true;
            }
            case "m_pathRequest":
            case "pathRequest":
            {
                if (value is null)
                {
                    instance.m_pathRequest = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiVolumeNavigator.PathRequest castValue)
                {
                    instance.m_pathRequest = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
