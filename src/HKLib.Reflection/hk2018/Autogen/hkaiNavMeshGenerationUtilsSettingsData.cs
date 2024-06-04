// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiNavMeshSimplificationUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshGenerationUtilsSettingsData : HavokData<hkaiNavMeshGenerationUtilsSettings> 
{
    public hkaiNavMeshGenerationUtilsSettingsData(HavokType type, hkaiNavMeshGenerationUtilsSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterHeight":
            case "characterHeight":
            {
                if (instance.m_characterHeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_quantizationGridSize":
            case "quantizationGridSize":
            {
                if (instance.m_quantizationGridSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxWalkableSlope":
            case "maxWalkableSlope":
            {
                if (instance.m_maxWalkableSlope is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleWinding":
            case "triangleWinding":
            {
                if (instance.m_triangleWinding is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_triangleWinding is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_degenerateAreaThreshold":
            case "degenerateAreaThreshold":
            {
                if (instance.m_degenerateAreaThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_degenerateWidthThreshold":
            case "degenerateWidthThreshold":
            {
                if (instance.m_degenerateWidthThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_convexThreshold":
            case "convexThreshold":
            {
                if (instance.m_convexThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxNumEdgesPerFace":
            case "maxNumEdgesPerFace":
            {
                if (instance.m_maxNumEdgesPerFace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeMatchingParams":
            case "edgeMatchingParams":
            {
                if (instance.m_edgeMatchingParams is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeMatchingMetric":
            case "edgeMatchingMetric":
            {
                if (instance.m_edgeMatchingMetric is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_edgeMatchingMetric is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_edgeConnectionIterations":
            case "edgeConnectionIterations":
            {
                if (instance.m_edgeConnectionIterations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_smallBoundaryEdgeGroupRemoval":
            case "smallBoundaryEdgeGroupRemoval":
            {
                if (instance.m_smallBoundaryEdgeGroupRemoval is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_regionPruningSettings":
            case "regionPruningSettings":
            {
                if (instance.m_regionPruningSettings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wallClimbingSettings":
            case "wallClimbingSettings":
            {
                if (instance.m_wallClimbingSettings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boundsAabb":
            case "boundsAabb":
            {
                if (instance.m_boundsAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_carvers":
            case "carvers":
            {
                if (instance.m_carvers is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_painters":
            case "painters":
            {
                if (instance.m_painters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_defaultConstructionProperties":
            case "defaultConstructionProperties":
            {
                if (instance.m_defaultConstructionProperties is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_defaultConstructionProperties is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_materialMap":
            case "materialMap":
            {
                if (instance.m_materialMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fixupOverlappingTriangles":
            case "fixupOverlappingTriangles":
            {
                if (instance.m_fixupOverlappingTriangles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_overlappingTrianglesSettings":
            case "overlappingTrianglesSettings":
            {
                if (instance.m_overlappingTrianglesSettings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_swapOverlappingAndQuantization":
            case "swapOverlappingAndQuantization":
            {
                if (instance.m_swapOverlappingAndQuantization is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weldInputVertices":
            case "weldInputVertices":
            {
                if (instance.m_weldInputVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weldThreshold":
            case "weldThreshold":
            {
                if (instance.m_weldThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minCharacterWidth":
            case "minCharacterWidth":
            {
                if (instance.m_minCharacterWidth is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterWidthUsage":
            case "characterWidthUsage":
            {
                if (instance.m_characterWidthUsage is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_characterWidthUsage is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_maxCharacterWidth":
            case "maxCharacterWidth":
            {
                if (instance.m_maxCharacterWidth is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_precalculateClearanceSeedingData":
            case "precalculateClearanceSeedingData":
            {
                if (instance.m_precalculateClearanceSeedingData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableSimplification":
            case "enableSimplification":
            {
                if (instance.m_enableSimplification is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simplificationSettings":
            case "simplificationSettings":
            {
                if (instance.m_simplificationSettings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_carvedMaterialDeprecated":
            case "carvedMaterialDeprecated":
            {
                if (instance.m_carvedMaterialDeprecated is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_carvedCuttingMaterialDeprecated":
            case "carvedCuttingMaterialDeprecated":
            {
                if (instance.m_carvedCuttingMaterialDeprecated is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_checkEdgeGeometryConsistency":
            case "checkEdgeGeometryConsistency":
            {
                if (instance.m_checkEdgeGeometryConsistency is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_saveInputSnapshot":
            case "saveInputSnapshot":
            {
                if (instance.m_saveInputSnapshot is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_snapshotFilename":
            case "snapshotFilename":
            {
                if (instance.m_snapshotFilename is null)
                {
                    return true;
                }
                if (instance.m_snapshotFilename is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_overrideSettings":
            case "overrideSettings":
            {
                if (instance.m_overrideSettings is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
                if (value is hkaiNavMeshGenerationUtilsSettings.TriangleWinding castValue)
                {
                    instance.m_triangleWinding = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_triangleWinding = (hkaiNavMeshGenerationUtilsSettings.TriangleWinding)byteValue;
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
                if (value is not hkaiNavMeshEdgeMatchingParameters castValue) return false;
                instance.m_edgeMatchingParams = castValue;
                return true;
            }
            case "m_edgeMatchingMetric":
            case "edgeMatchingMetric":
            {
                if (value is hkaiNavMeshGenerationUtilsSettings.EdgeMatchingMetric castValue)
                {
                    instance.m_edgeMatchingMetric = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_edgeMatchingMetric = (hkaiNavMeshGenerationUtilsSettings.EdgeMatchingMetric)uintValue;
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
                if (value is not hkaiNavMeshGenerationUtilsSettings.RegionPruningSettings castValue) return false;
                instance.m_regionPruningSettings = castValue;
                return true;
            }
            case "m_wallClimbingSettings":
            case "wallClimbingSettings":
            {
                if (value is not hkaiNavMeshGenerationUtilsSettings.WallClimbingSettings castValue) return false;
                instance.m_wallClimbingSettings = castValue;
                return true;
            }
            case "m_boundsAabb":
            case "boundsAabb":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_boundsAabb = castValue;
                return true;
            }
            case "m_carvers":
            case "carvers":
            {
                if (value is not List<hkaiCarver?> castValue) return false;
                instance.m_carvers = castValue;
                return true;
            }
            case "m_painters":
            case "painters":
            {
                if (value is not List<hkaiMaterialPainter?> castValue) return false;
                instance.m_painters = castValue;
                return true;
            }
            case "m_defaultConstructionProperties":
            case "defaultConstructionProperties":
            {
                if (value is hkaiNavMeshGenerationUtilsSettings.ConstructionFlagsBits castValue)
                {
                    instance.m_defaultConstructionProperties = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_defaultConstructionProperties = (hkaiNavMeshGenerationUtilsSettings.ConstructionFlagsBits)uintValue;
                    return true;
                }
                return false;
            }
            case "m_materialMap":
            case "materialMap":
            {
                if (value is not List<hkaiNavMeshGenerationUtilsSettings.MaterialConstructionPair> castValue) return false;
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
                if (value is not hkaiOverlappingTriangles.Settings castValue) return false;
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
                if (value is hkaiNavMeshGenerationUtilsSettings.CharacterWidthUsage castValue)
                {
                    instance.m_characterWidthUsage = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_characterWidthUsage = (hkaiNavMeshGenerationUtilsSettings.CharacterWidthUsage)byteValue;
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
                if (value is not Settings castValue) return false;
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
                if (value is not List<hkaiNavMeshGenerationUtilsSettings.OverrideSettings> castValue) return false;
                instance.m_overrideSettings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
