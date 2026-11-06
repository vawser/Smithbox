// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiNavMeshGenerationSettingsWallClimbingSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiNavMeshGenerationSettingsWallClimbingSettings>
{
    public hkaiNavMeshGenerationSettingsWallClimbingSettingsData(Havok.hk2014.hkaiNavMeshGenerationSettingsWallClimbingSettings instance) : base(instance) { }

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
