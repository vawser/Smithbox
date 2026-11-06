// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSettingsSimplificationOverrideData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Settings.SimplificationOverride>
{
    public hkaiNavMeshGenerationSettingsSimplificationOverrideData(Havok.hk2019.hkaiNavMeshGeneration.Settings.SimplificationOverride instance) : base(instance) { }

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
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
                if (value is Havok.hk2019.hkaiVolume castValue)
                {
                    instance.m_volume = castValue;
                    return true;
                }
                return false;
            }
            case "m_data":
            case "data":
            {
                if (value is not int castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (value is not Havok.hk2019.hkaiNavMeshSimplificationUtils.Settings castValue) return false;
                instance.m_simplificationSettings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
