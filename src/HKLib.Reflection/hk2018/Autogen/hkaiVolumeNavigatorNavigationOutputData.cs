// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiVolumeNavigatorNavigationOutputData : HavokData<hkaiVolumeNavigator.NavigationOutput> 
{
    public hkaiVolumeNavigatorNavigationOutputData(HavokType type, hkaiVolumeNavigator.NavigationOutput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_result":
            case "result":
            {
                if (instance.m_result is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_result is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_pathQuality":
            case "pathQuality":
            {
                if (instance.m_pathQuality is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_pathQuality is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_isPathStale":
            case "isPathStale":
            {
                if (instance.m_isPathStale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isOldInput":
            case "isOldInput":
            {
                if (instance.m_isOldInput is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cellKey":
            case "cellKey":
            {
                if (instance.m_cellKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_correctedPosition":
            case "correctedPosition":
            {
                if (instance.m_correctedPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_correctedGoal":
            case "correctedGoal":
            {
                if (instance.m_correctedGoal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_idealDirection":
            case "idealDirection":
            {
                if (instance.m_idealDirection is not TGet castValue) return false;
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
            case "m_result":
            case "result":
            {
                if (value is hkaiVolumeNavigator.NavigationOutput.Result castValue)
                {
                    instance.m_result = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_result = (hkaiVolumeNavigator.NavigationOutput.Result)intValue;
                    return true;
                }
                return false;
            }
            case "m_pathQuality":
            case "pathQuality":
            {
                if (value is hkaiVolumeNavigator.PathQualityBits castValue)
                {
                    instance.m_pathQuality = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_pathQuality = (hkaiVolumeNavigator.PathQualityBits)intValue;
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
