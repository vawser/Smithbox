// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpMotionPropertiesFullCastSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpMotionProperties.FullCastSettings>
{
    public hknpMotionPropertiesFullCastSettingsData(Havok.hk2019.hknpMotionProperties.FullCastSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minSeparation":
            case "minSeparation":
            {
                value = instance.m_minSeparation;
                return true;
            }
            case "m_minExtraSeparation":
            case "minExtraSeparation":
            {
                value = instance.m_minExtraSeparation;
                return true;
            }
            case "m_toiSeparation":
            case "toiSeparation":
            {
                value = instance.m_toiSeparation;
                return true;
            }
            case "m_toiExtraSeparation":
            case "toiExtraSeparation":
            {
                value = instance.m_toiExtraSeparation;
                return true;
            }
            case "m_toiAccuracy":
            case "toiAccuracy":
            {
                value = instance.m_toiAccuracy;
                return true;
            }
            case "m_relativeSafeDeltaTime":
            case "relativeSafeDeltaTime":
            {
                value = instance.m_relativeSafeDeltaTime;
                return true;
            }
            case "m_absoluteSafeDeltaTime":
            case "absoluteSafeDeltaTime":
            {
                value = instance.m_absoluteSafeDeltaTime;
                return true;
            }
            case "m_keepTime":
            case "keepTime":
            {
                value = instance.m_keepTime;
                return true;
            }
            case "m_keepDistance":
            case "keepDistance":
            {
                value = instance.m_keepDistance;
                return true;
            }
            case "m_maxIterations":
            case "maxIterations":
            {
                value = instance.m_maxIterations;
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
            case "m_minSeparation":
            case "minSeparation":
            {
                if (value is not float castValue) return false;
                instance.m_minSeparation = castValue;
                return true;
            }
            case "m_minExtraSeparation":
            case "minExtraSeparation":
            {
                if (value is not float castValue) return false;
                instance.m_minExtraSeparation = castValue;
                return true;
            }
            case "m_toiSeparation":
            case "toiSeparation":
            {
                if (value is not float castValue) return false;
                instance.m_toiSeparation = castValue;
                return true;
            }
            case "m_toiExtraSeparation":
            case "toiExtraSeparation":
            {
                if (value is not float castValue) return false;
                instance.m_toiExtraSeparation = castValue;
                return true;
            }
            case "m_toiAccuracy":
            case "toiAccuracy":
            {
                if (value is not float castValue) return false;
                instance.m_toiAccuracy = castValue;
                return true;
            }
            case "m_relativeSafeDeltaTime":
            case "relativeSafeDeltaTime":
            {
                if (value is not float castValue) return false;
                instance.m_relativeSafeDeltaTime = castValue;
                return true;
            }
            case "m_absoluteSafeDeltaTime":
            case "absoluteSafeDeltaTime":
            {
                if (value is not float castValue) return false;
                instance.m_absoluteSafeDeltaTime = castValue;
                return true;
            }
            case "m_keepTime":
            case "keepTime":
            {
                if (value is not float castValue) return false;
                instance.m_keepTime = castValue;
                return true;
            }
            case "m_keepDistance":
            case "keepDistance":
            {
                if (value is not float castValue) return false;
                instance.m_keepDistance = castValue;
                return true;
            }
            case "m_maxIterations":
            case "maxIterations":
            {
                if (value is not int castValue) return false;
                instance.m_maxIterations = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
