// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationRegionPruningSettingsData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.RegionPruningSettings> 
{
    public hkaiNavMeshGenerationRegionPruningSettingsData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.RegionPruningSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minRegionArea":
            case "minRegionArea":
            {
                if (instance.m_minRegionArea is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minDistanceToSeedPoints":
            case "minDistanceToSeedPoints":
            {
                if (instance.m_minDistanceToSeedPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_regionSeedPoints":
            case "regionSeedPoints":
            {
                if (instance.m_regionSeedPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_regionConnections":
            case "regionConnections":
            {
                if (instance.m_regionConnections is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pruningAabb":
            case "pruningAabb":
            {
                if (instance.m_pruningAabb is not TGet castValue) return false;
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
                if (value is not List<HKLib.hk2019.hkaiNavMeshGeneration.RegionPruningSettings.RegionConnection> castValue) return false;
                instance.m_regionConnections = castValue;
                return true;
            }
            case "m_pruningAabb":
            case "pruningAabb":
            {
                if (value is not HKLib.hk2019.hkAabb castValue) return false;
                instance.m_pruningAabb = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
