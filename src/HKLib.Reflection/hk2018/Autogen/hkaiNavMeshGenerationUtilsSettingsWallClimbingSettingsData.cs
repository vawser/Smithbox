// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshGenerationUtilsSettingsWallClimbingSettingsData : HavokData<hkaiNavMeshGenerationUtilsSettings.WallClimbingSettings> 
{
    public hkaiNavMeshGenerationUtilsSettingsWallClimbingSettingsData(HavokType type, hkaiNavMeshGenerationUtilsSettings.WallClimbingSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_enableWallClimbing":
            case "enableWallClimbing":
            {
                if (instance.m_enableWallClimbing is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_excludeWalkableFaces":
            case "excludeWalkableFaces":
            {
                if (instance.m_excludeWalkableFaces is not TGet castValue) return false;
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
