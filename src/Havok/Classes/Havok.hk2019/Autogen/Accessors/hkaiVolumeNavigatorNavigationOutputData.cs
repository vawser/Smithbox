// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiVolumeNavigatorNavigationOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiVolumeNavigator.NavigationOutput>
{
    public hkaiVolumeNavigatorNavigationOutputData(Havok.hk2019.hkaiVolumeNavigator.NavigationOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_result":
            case "result":
            {
                value = instance.m_result;
                return true;
            }
            case "m_pathQuality":
            case "pathQuality":
            {
                value = instance.m_pathQuality;
                return true;
            }
            case "m_isPathStale":
            case "isPathStale":
            {
                value = instance.m_isPathStale;
                return true;
            }
            case "m_isOldInput":
            case "isOldInput":
            {
                value = instance.m_isOldInput;
                return true;
            }
            case "m_cellKey":
            case "cellKey":
            {
                value = instance.m_cellKey;
                return true;
            }
            case "m_correctedPosition":
            case "correctedPosition":
            {
                value = instance.m_correctedPosition;
                return true;
            }
            case "m_correctedGoal":
            case "correctedGoal":
            {
                value = instance.m_correctedGoal;
                return true;
            }
            case "m_idealDirection":
            case "idealDirection":
            {
                value = instance.m_idealDirection;
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
            case "m_result":
            case "result":
            {
                if (value is Havok.hk2019.hkaiVolumeNavigator.NavigationOutput.Result castValue)
                {
                    instance.m_result = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_result = (Havok.hk2019.hkaiVolumeNavigator.NavigationOutput.Result)intValue;
                    return true;
                }
                return false;
            }
            case "m_pathQuality":
            case "pathQuality":
            {
                if (value is Havok.hk2019.hkaiVolumeNavigator.PathQualityBits castValue)
                {
                    instance.m_pathQuality = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_pathQuality = (Havok.hk2019.hkaiVolumeNavigator.PathQualityBits)intValue;
                    return true;
                }
                return false;
            }
            case "m_isPathStale":
            case "isPathStale":
            {
                if (value is not bool castValue) return false;
                instance.m_isPathStale = castValue;
                return true;
            }
            case "m_isOldInput":
            case "isOldInput":
            {
                if (value is not bool castValue) return false;
                instance.m_isOldInput = castValue;
                return true;
            }
            case "m_cellKey":
            case "cellKey":
            {
                if (value is not uint castValue) return false;
                instance.m_cellKey = castValue;
                return true;
            }
            case "m_correctedPosition":
            case "correctedPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_correctedPosition = castValue;
                return true;
            }
            case "m_correctedGoal":
            case "correctedGoal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_correctedGoal = castValue;
                return true;
            }
            case "m_idealDirection":
            case "idealDirection":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_idealDirection = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
