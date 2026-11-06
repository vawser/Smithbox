// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshGenerationUtilsSettingsWallClimbingSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshGenerationUtilsSettings.WallClimbingSettings>
{
    public hkaiNavMeshGenerationUtilsSettingsWallClimbingSettingsData(Havok.hk2018.hkaiNavMeshGenerationUtilsSettings.WallClimbingSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_enableWallClimbing":
            case "enableWallClimbing":
            {
                value = instance.m_enableWallClimbing;
                return true;
            }
            case "m_excludeWalkableFaces":
            case "excludeWalkableFaces":
            {
                value = instance.m_excludeWalkableFaces;
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
            case "m_enableWallClimbing":
            case "enableWallClimbing":
            {
                if (value is not bool castValue) return false;
                instance.m_enableWallClimbing = castValue;
                return true;
            }
            case "m_excludeWalkableFaces":
            case "excludeWalkableFaces":
            {
                if (value is not bool castValue) return false;
                instance.m_excludeWalkableFaces = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
