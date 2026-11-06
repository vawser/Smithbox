// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshDeprecatedGenerationUtilsSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings>
{
    public hkaiNavMeshDeprecatedGenerationUtilsSettingsData(Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_characterHeight":
            case "characterHeight":
            {
                value = instance.m_characterHeight;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_quantizationGridSize":
            case "quantizationGridSize":
            {
                value = instance.m_quantizationGridSize;
                return true;
            }
            case "m_maxWalkableSlope":
            case "maxWalkableSlope":
            {
                value = instance.m_maxWalkableSlope;
                return true;
            }
            case "m_triangleWinding":
            case "triangleWinding":
            {
                value = instance.m_triangleWinding;
                return true;
            }
            case "m_degenerateAreaThreshold":
            case "degenerateAreaThreshold":
            {
                value = instance.m_degenerateAreaThreshold;
                return true;
            }
            case "m_degenerateWidthThreshold":
            case "degenerateWidthThreshold":
            {
                value = instance.m_degenerateWidthThreshold;
                return true;
            }
            case "m_convexThreshold":
            case "convexThreshold":
            {
                value = instance.m_convexThreshold;
                return true;
            }
            case "m_maxNumEdgesPerFace":
            case "maxNumEdgesPerFace":
            {
                value = instance.m_maxNumEdgesPerFace;
                return true;
            }
            case "m_edgeMatchingParams":
            case "edgeMatchingParams":
            {
                value = instance.m_edgeMatchingParams;
                return true;
            }
            case "m_edgeMatchingMetric":
            case "edgeMatchingMetric":
            {
                value = instance.m_edgeMatchingMetric;
                return true;
            }
            case "m_edgeConnectionIterations":
            case "edgeConnectionIterations":
            {
                value = instance.m_edgeConnectionIterations;
                return true;
            }
            case "m_smallBoundaryEdgeGroupRemoval":
            case "smallBoundaryEdgeGroupRemoval":
            {
                value = instance.m_smallBoundaryEdgeGroupRemoval;
                return true;
            }
            case "m_regionPruningSettings":
            case "regionPruningSettings":
            {
                value = instance.m_regionPruningSettings;
                return true;
            }
            case "m_wallClimbingSettings":
            case "wallClimbingSettings":
            {
                value = instance.m_wallClimbingSettings;
                return true;
            }
            case "m_boundsAabb":
            case "boundsAabb":
            {
                value = instance.m_boundsAabb;
                return true;
            }
            case "m_carvers":
            case "carvers":
            {
                value = instance.m_carvers;
                return true;
            }
            case "m_painters":
            case "painters":
            {
                value = instance.m_painters;
                return true;
            }
            case "m_defaultConstructionProperties":
            case "defaultConstructionProperties":
            {
                value = instance.m_defaultConstructionProperties;
                return true;
            }
            case "m_materialMap":
            case "materialMap":
            {
                value = instance.m_materialMap;
                return true;
            }
            case "m_fixupOverlappingTriangles":
            case "fixupOverlappingTriangles":
            {
                value = instance.m_fixupOverlappingTriangles;
                return true;
            }
            case "m_overlappingTrianglesSettings":
            case "overlappingTrianglesSettings":
            {
                value = instance.m_overlappingTrianglesSettings;
                return true;
            }
            case "m_swapOverlappingAndQuantization":
            case "swapOverlappingAndQuantization":
            {
                value = instance.m_swapOverlappingAndQuantization;
                return true;
            }
            case "m_weldInputVertices":
            case "weldInputVertices":
            {
                value = instance.m_weldInputVertices;
                return true;
            }
            case "m_weldThreshold":
            case "weldThreshold":
            {
                value = instance.m_weldThreshold;
                return true;
            }
            case "m_minCharacterWidth":
            case "minCharacterWidth":
            {
                value = instance.m_minCharacterWidth;
                return true;
            }
            case "m_characterWidthUsage":
            case "characterWidthUsage":
            {
                value = instance.m_characterWidthUsage;
                return true;
            }
            case "m_maxCharacterWidth":
            case "maxCharacterWidth":
            {
                value = instance.m_maxCharacterWidth;
                return true;
            }
            case "m_precalculateClearanceSeedingData":
            case "precalculateClearanceSeedingData":
            {
                value = instance.m_precalculateClearanceSeedingData;
                return true;
            }
            case "m_precalculateFaceIterator":
            case "precalculateFaceIterator":
            {
                value = instance.m_precalculateFaceIterator;
                return true;
            }
            case "m_enableSimplification":
            case "enableSimplification":
            {
                value = instance.m_enableSimplification;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                value = instance.m_simplificationSettings;
                return true;
            }
            case "m_carvedMaterialDeprecated":
            case "carvedMaterialDeprecated":
            {
                value = instance.m_carvedMaterialDeprecated;
                return true;
            }
            case "m_carvedCuttingMaterialDeprecated":
            case "carvedCuttingMaterialDeprecated":
            {
                value = instance.m_carvedCuttingMaterialDeprecated;
                return true;
            }
            case "m_checkEdgeGeometryConsistency":
            case "checkEdgeGeometryConsistency":
            {
                value = instance.m_checkEdgeGeometryConsistency;
                return true;
            }
            case "m_saveInputSnapshot":
            case "saveInputSnapshot":
            {
                value = instance.m_saveInputSnapshot;
                return true;
            }
            case "m_snapshotFilename":
            case "snapshotFilename":
            {
                value = instance.m_snapshotFilename;
                return true;
            }
            case "m_overrideSettings":
            case "overrideSettings":
            {
                value = instance.m_overrideSettings;
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
            case "m_characterHeight":
            case "characterHeight":
            {
                if (value is not float castValue) return false;
                instance.m_characterHeight = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_quantizationGridSize":
            case "quantizationGridSize":
            {
                if (value is not float castValue) return false;
                instance.m_quantizationGridSize = castValue;
                return true;
            }
            case "m_maxWalkableSlope":
            case "maxWalkableSlope":
            {
                if (value is not float castValue) return false;
                instance.m_maxWalkableSlope = castValue;
                return true;
            }
            case "m_triangleWinding":
            case "triangleWinding":
            {
                if (value is Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.TriangleWinding castValue)
                {
                    instance.m_triangleWinding = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_triangleWinding = (Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.TriangleWinding)byteValue;
                    return true;
                }
                return false;
            }
            case "m_degenerateAreaThreshold":
            case "degenerateAreaThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_degenerateAreaThreshold = castValue;
                return true;
            }
            case "m_degenerateWidthThreshold":
            case "degenerateWidthThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_degenerateWidthThreshold = castValue;
                return true;
            }
            case "m_convexThreshold":
            case "convexThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_convexThreshold = castValue;
                return true;
            }
            case "m_maxNumEdgesPerFace":
            case "maxNumEdgesPerFace":
            {
                if (value is not int castValue) return false;
                instance.m_maxNumEdgesPerFace = castValue;
                return true;
            }
            case "m_edgeMatchingParams":
            case "edgeMatchingParams":
            {
                if (value is not Havok.hk2019.hkaiNavMeshEdgeMatchingParameters castValue) return false;
                instance.m_edgeMatchingParams = castValue;
                return true;
            }
            case "m_edgeMatchingMetric":
            case "edgeMatchingMetric":
            {
                if (value is Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.EdgeMatchingMetric castValue)
                {
                    instance.m_edgeMatchingMetric = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_edgeMatchingMetric = (Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.EdgeMatchingMetric)uintValue;
                    return true;
                }
                return false;
            }
            case "m_edgeConnectionIterations":
            case "edgeConnectionIterations":
            {
                if (value is not int castValue) return false;
                instance.m_edgeConnectionIterations = castValue;
                return true;
            }
            case "m_smallBoundaryEdgeGroupRemoval":
            case "smallBoundaryEdgeGroupRemoval":
            {
                if (value is not bool castValue) return false;
                instance.m_smallBoundaryEdgeGroupRemoval = castValue;
                return true;
            }
            case "m_regionPruningSettings":
            case "regionPruningSettings":
            {
                if (value is not Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.RegionPruningSettings_2 castValue) return false;
                instance.m_regionPruningSettings = castValue;
                return true;
            }
            case "m_wallClimbingSettings":
            case "wallClimbingSettings":
            {
                if (value is not Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.WallClimbingSettings castValue) return false;
                instance.m_wallClimbingSettings = castValue;
                return true;
            }
            case "m_boundsAabb":
            case "boundsAabb":
            {
                if (value is not Havok.hk2019.hkAabb castValue) return false;
                instance.m_boundsAabb = castValue;
                return true;
            }
            case "m_carvers":
            case "carvers":
            {
                if (value is not List<Havok.hk2019.hkaiCarver?> castValue) return false;
                instance.m_carvers = castValue;
                return true;
            }
            case "m_painters":
            case "painters":
            {
                if (value is not List<Havok.hk2019.hkaiMaterialPainter?> castValue) return false;
                instance.m_painters = castValue;
                return true;
            }
            case "m_defaultConstructionProperties":
            case "defaultConstructionProperties":
            {
                if (value is Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.ConstructionFlagsBits castValue)
                {
                    instance.m_defaultConstructionProperties = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_defaultConstructionProperties = (Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.ConstructionFlagsBits)uintValue;
                    return true;
                }
                return false;
            }
            case "m_materialMap":
            case "materialMap":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.MaterialConstructionPair> castValue) return false;
                instance.m_materialMap = castValue;
                return true;
            }
            case "m_fixupOverlappingTriangles":
            case "fixupOverlappingTriangles":
            {
                if (value is not bool castValue) return false;
                instance.m_fixupOverlappingTriangles = castValue;
                return true;
            }
            case "m_overlappingTrianglesSettings":
            case "overlappingTrianglesSettings":
            {
                if (value is not Havok.hk2019.hkaiOverlappingTriangles.Settings castValue) return false;
                instance.m_overlappingTrianglesSettings = castValue;
                return true;
            }
            case "m_swapOverlappingAndQuantization":
            case "swapOverlappingAndQuantization":
            {
                if (value is not bool castValue) return false;
                instance.m_swapOverlappingAndQuantization = castValue;
                return true;
            }
            case "m_weldInputVertices":
            case "weldInputVertices":
            {
                if (value is not bool castValue) return false;
                instance.m_weldInputVertices = castValue;
                return true;
            }
            case "m_weldThreshold":
            case "weldThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_weldThreshold = castValue;
                return true;
            }
            case "m_minCharacterWidth":
            case "minCharacterWidth":
            {
                if (value is not float castValue) return false;
                instance.m_minCharacterWidth = castValue;
                return true;
            }
            case "m_characterWidthUsage":
            case "characterWidthUsage":
            {
                if (value is Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.CharacterWidthUsage castValue)
                {
                    instance.m_characterWidthUsage = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_characterWidthUsage = (Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.CharacterWidthUsage)byteValue;
                    return true;
                }
                return false;
            }
            case "m_maxCharacterWidth":
            case "maxCharacterWidth":
            {
                if (value is not float castValue) return false;
                instance.m_maxCharacterWidth = castValue;
                return true;
            }
            case "m_precalculateClearanceSeedingData":
            case "precalculateClearanceSeedingData":
            {
                if (value is not bool castValue) return false;
                instance.m_precalculateClearanceSeedingData = castValue;
                return true;
            }
            case "m_precalculateFaceIterator":
            case "precalculateFaceIterator":
            {
                if (value is not bool castValue) return false;
                instance.m_precalculateFaceIterator = castValue;
                return true;
            }
            case "m_enableSimplification":
            case "enableSimplification":
            {
                if (value is not bool castValue) return false;
                instance.m_enableSimplification = castValue;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (value is not Havok.hk2019.hkaiNavMeshSimplificationUtils.Settings castValue) return false;
                instance.m_simplificationSettings = castValue;
                return true;
            }
            case "m_carvedMaterialDeprecated":
            case "carvedMaterialDeprecated":
            {
                if (value is not int castValue) return false;
                instance.m_carvedMaterialDeprecated = castValue;
                return true;
            }
            case "m_carvedCuttingMaterialDeprecated":
            case "carvedCuttingMaterialDeprecated":
            {
                if (value is not int castValue) return false;
                instance.m_carvedCuttingMaterialDeprecated = castValue;
                return true;
            }
            case "m_checkEdgeGeometryConsistency":
            case "checkEdgeGeometryConsistency":
            {
                if (value is not bool castValue) return false;
                instance.m_checkEdgeGeometryConsistency = castValue;
                return true;
            }
            case "m_saveInputSnapshot":
            case "saveInputSnapshot":
            {
                if (value is not bool castValue) return false;
                instance.m_saveInputSnapshot = castValue;
                return true;
            }
            case "m_snapshotFilename":
            case "snapshotFilename":
            {
                if (value is null)
                {
                    instance.m_snapshotFilename = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_snapshotFilename = castValue;
                    return true;
                }
                return false;
            }
            case "m_overrideSettings":
            case "overrideSettings":
            {
                if (value is not List<Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.OverrideSettings> castValue) return false;
                instance.m_overrideSettings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
