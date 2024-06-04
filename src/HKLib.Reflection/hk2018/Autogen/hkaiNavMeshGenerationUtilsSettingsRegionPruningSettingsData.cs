// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshGenerationUtilsSettingsRegionPruningSettingsData : HavokData<hkaiNavMeshGenerationUtilsSettings.RegionPruningSettings> 
{
    public hkaiNavMeshGenerationUtilsSettingsRegionPruningSettingsData(HavokType type, hkaiNavMeshGenerationUtilsSettings.RegionPruningSettings instance) : base(type, instance) {}

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
            case "m_borderPreservationTolerance":
            case "borderPreservationTolerance":
            {
                if (instance.m_borderPreservationTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_preserveVerticalBorderRegions":
            case "preserveVerticalBorderRegions":
            {
                if (instance.m_preserveVerticalBorderRegions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pruneBeforeTriangulation":
            case "pruneBeforeTriangulation":
            {
                if (instance.m_pruneBeforeTriangulation is not TGet castValue) return false;
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
            case "m_borderPreservationTolerance":
            case "borderPreservationTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_borderPreservationTolerance = castValue;
                return true;
            }
            case "m_preserveVerticalBorderRegions":
            case "preserveVerticalBorderRegions":
            {
                if (value is not bool castValue) return false;
                instance.m_preserveVerticalBorderRegions = castValue;
                return true;
            }
            case "m_pruneBeforeTriangulation":
            case "pruneBeforeTriangulation":
            {
                if (value is not bool castValue) return false;
                instance.m_pruneBeforeTriangulation = castValue;
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
                if (value is not List<hkaiNavMeshGenerationUtilsSettings.RegionPruningSettings.RegionConnection> castValue) return false;
                instance.m_regionConnections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
