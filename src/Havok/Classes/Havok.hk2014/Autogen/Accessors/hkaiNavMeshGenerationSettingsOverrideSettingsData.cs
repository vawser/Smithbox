// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiNavMeshGenerationSettingsOverrideSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiNavMeshGenerationSettingsOverrideSettings>
{
    public hkaiNavMeshGenerationSettingsOverrideSettingsData(Havok.hk2014.hkaiNavMeshGenerationSettingsOverrideSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_volume":
            case "volume":
            {
                value = instance.m_volume;
                return true;
            }
            case "m_material":
            case "material":
            {
                value = instance.m_material;
                return true;
            }
            case "m_characterWidthUsage":
            case "characterWidthUsage":
            {
                value = instance.m_characterWidthUsage;
                return true;
            }
            case "m_maxWalkableSlope":
            case "maxWalkableSlope":
            {
                value = instance.m_maxWalkableSlope;
                return true;
            }
            case "m_edgeMatchingParams":
            case "edgeMatchingParams":
            {
                value = instance.m_edgeMatchingParams;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                value = instance.m_simplificationSettings;
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
            case "m_volume":
            case "volume":
            {
                if (value is null)
                {
                    instance.m_volume = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiVolume castValue)
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
                if (value is Havok.hk2014.hkaiNavMeshGeneration.Settings.CharacterWidthUsage castValue)
                {
                    instance.m_characterWidthUsage = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_characterWidthUsage = (Havok.hk2014.hkaiNavMeshGeneration.Settings.CharacterWidthUsage)byteValue;
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
                if (value is not Havok.hk2014.hkaiNavMeshEdgeMatchingParameters castValue) return false;
                instance.m_edgeMatchingParams = castValue;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (value is not Havok.hk2014.hkaiNavMeshSimplificationUtils.Settings castValue) return false;
                instance.m_simplificationSettings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
