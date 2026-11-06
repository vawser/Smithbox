// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavigatorNavigationOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavigator.NavigationOutput>
{
    public hkaiNavigatorNavigationOutputData(Havok.hk2018.hkaiNavigator.NavigationOutput instance) : base(instance) { }

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
            case "m_faceKey":
            case "faceKey":
            {
                value = instance.m_faceKey;
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
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                value = instance.m_surfaceVelocity;
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
                if (value is Havok.hk2018.hkaiNavigator.NavigationOutput.Result castValue)
                {
                    instance.m_result = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_result = (Havok.hk2018.hkaiNavigator.NavigationOutput.Result)intValue;
                    return true;
                }
                return false;
            }
            case "m_pathQuality":
            case "pathQuality":
            {
                if (value is Havok.hk2018.hkaiNavigator.PathQualityBits castValue)
                {
                    instance.m_pathQuality = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_pathQuality = (Havok.hk2018.hkaiNavigator.PathQualityBits)intValue;
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
            case "m_faceKey":
            case "faceKey":
            {
                if (value is not uint castValue) return false;
                instance.m_faceKey = castValue;
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
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_surfaceVelocity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
