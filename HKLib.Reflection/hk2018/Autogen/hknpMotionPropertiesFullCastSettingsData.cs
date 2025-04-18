// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpMotionPropertiesFullCastSettingsData : HavokData<hknpMotionProperties.FullCastSettings> 
{
    public hknpMotionPropertiesFullCastSettingsData(HavokType type, hknpMotionProperties.FullCastSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minSeparation":
            case "minSeparation":
            {
                if (instance.m_minSeparation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minExtraSeparation":
            case "minExtraSeparation":
            {
                if (instance.m_minExtraSeparation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toiSeparation":
            case "toiSeparation":
            {
                if (instance.m_toiSeparation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toiExtraSeparation":
            case "toiExtraSeparation":
            {
                if (instance.m_toiExtraSeparation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toiAccuracy":
            case "toiAccuracy":
            {
                if (instance.m_toiAccuracy is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_relativeSafeDeltaTime":
            case "relativeSafeDeltaTime":
            {
                if (instance.m_relativeSafeDeltaTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_absoluteSafeDeltaTime":
            case "absoluteSafeDeltaTime":
            {
                if (instance.m_absoluteSafeDeltaTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_keepTime":
            case "keepTime":
            {
                if (instance.m_keepTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_keepDistance":
            case "keepDistance":
            {
                if (instance.m_keepDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxIterations":
            case "maxIterations":
            {
                if (instance.m_maxIterations is not TGet castValue) return false;
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
