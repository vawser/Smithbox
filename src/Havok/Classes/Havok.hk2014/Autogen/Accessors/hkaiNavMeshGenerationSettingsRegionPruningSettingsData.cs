// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiNavMeshGenerationSettingsRegionPruningSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiNavMeshGenerationSettingsRegionPruningSettings>
{
    public hkaiNavMeshGenerationSettingsRegionPruningSettingsData(Havok.hk2014.hkaiNavMeshGenerationSettingsRegionPruningSettings instance) : base(instance) { }

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
            case "m_borderPreservationTolerance":
            case "borderPreservationTolerance":
            {
                value = instance.m_borderPreservationTolerance;
                return true;
            }
            case "m_preserveVerticalBorderRegions":
            case "preserveVerticalBorderRegions":
            {
                value = instance.m_preserveVerticalBorderRegions;
                return true;
            }
            case "m_pruneBeforeTriangulation":
            case "pruneBeforeTriangulation":
            {
                value = instance.m_pruneBeforeTriangulation;
                return true;
            }
            case "m_regionSeedPoints":
            case "regionSeedPoints":
            {
                value = instance.m_regionSeedPoints;
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
            default:
            return false;
        }
    }

}
