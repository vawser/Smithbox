// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiNavMeshSimplificationUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshGenerationUtilsSettingsOverrideSettingsData : HavokData<hkaiNavMeshGenerationUtilsSettings.OverrideSettings> 
{
    public hkaiNavMeshGenerationUtilsSettingsOverrideSettingsData(HavokType type, hkaiNavMeshGenerationUtilsSettings.OverrideSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_volume":
            case "volume":
            {
                if (instance.m_volume is null)
                {
                    return true;
                }
                if (instance.m_volume is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_material":
            case "material":
            {
                if (instance.m_material is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterWidthUsage":
            case "characterWidthUsage":
            {
                if (instance.m_characterWidthUsage is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_characterWidthUsage is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_maxWalkableSlope":
            case "maxWalkableSlope":
            {
                if (instance.m_maxWalkableSlope is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeMatchingParams":
            case "edgeMatchingParams":
            {
                if (instance.m_edgeMatchingParams is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (instance.m_simplificationSettings is not TGet castValue) return false;
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
            case "m_volume":
            case "volume":
            {
                if (value is null)
                {
                    instance.m_volume = default;
                    return true;
                }
                if (value is hkaiVolume castValue)
                {
                    instance.m_volume = castValue;
                    return true;
                }
                return false;
            }
            case "m_material":
            case "material":
            {
                if (value is not int castValue) return false;
                instance.m_material = castValue;
                return true;
            }
            case "m_characterWidthUsage":
            case "characterWidthUsage":
            {
                if (value is hkaiNavMeshGenerationUtilsSettings.CharacterWidthUsage castValue)
                {
                    instance.m_characterWidthUsage = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_characterWidthUsage = (hkaiNavMeshGenerationUtilsSettings.CharacterWidthUsage)byteValue;
                    return true;
                }
                return false;
            }
            case "m_maxWalkableSlope":
            case "maxWalkableSlope":
            {
                if (value is not float castValue) return false;
                instance.m_maxWalkableSlope = castValue;
                return true;
            }
            case "m_edgeMatchingParams":
            case "edgeMatchingParams":
            {
                if (value is not hkaiNavMeshEdgeMatchingParameters castValue) return false;
                instance.m_edgeMatchingParams = castValue;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (value is not Settings castValue) return false;
                instance.m_simplificationSettings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
