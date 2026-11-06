// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationRegionPruningSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.RegionPruningSettings>
{
    public hkaiNavMeshGenerationRegionPruningSettingsData(Havok.hk2019.hkaiNavMeshGeneration.RegionPruningSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minRegionArea":
            case "minRegionArea":
            {
                value = instance.m_minRegionArea;
                return true;
            }
            case "m_minDistanceToSeedPoints":
            case "minDistanceToSeedPoints":
            {
                value = instance.m_minDistanceToSeedPoints;
                return true;
            }
            case "m_regionSeedPoints":
            case "regionSeedPoints":
            {
                value = instance.m_regionSeedPoints;
                return true;
            }
            case "m_regionConnections":
            case "regionConnections":
            {
                value = instance.m_regionConnections;
                return true;
            }
            case "m_pruningAabb":
            case "pruningAabb":
            {
                value = instance.m_pruningAabb;
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
            case "m_minRegionArea":
            case "minRegionArea":
            {
                if (value is not float castValue) return false;
                instance.m_minRegionArea = castValue;
                return true;
            }
            case "m_minDistanceToSeedPoints":
            case "minDistanceToSeedPoints":
            {
                if (value is not float castValue) return false;
                instance.m_minDistanceToSeedPoints = castValue;
                return true;
            }
            case "m_regionSeedPoints":
            case "regionSeedPoints":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_regionSeedPoints = castValue;
                return true;
            }
            case "m_regionConnections":
            case "regionConnections":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshGeneration.RegionPruningSettings.RegionConnection> castValue) return false;
                instance.m_regionConnections = castValue;
                return true;
            }
            case "m_pruningAabb":
            case "pruningAabb":
            {
                if (value is not Havok.hk2019.hkAabb castValue) return false;
                instance.m_pruningAabb = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
