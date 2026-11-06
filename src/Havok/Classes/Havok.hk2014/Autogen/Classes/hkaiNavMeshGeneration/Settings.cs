// Automatically Generated

namespace Havok.hk2014.hkaiNavMeshGeneration;

public class Settings : hkReferencedObject
{
    public float m_characterHeight;

    public Vector4 m_up = new();

    public float m_quantizationGridSize;

    public float m_maxWalkableSlope;

    public hkaiNavMeshGeneration.Settings.TriangleWinding m_triangleWinding;

    public float m_degenerateAreaThreshold;

    public float m_degenerateWidthThreshold;

    public float m_convexThreshold;

    public int m_maxNumEdgesPerFace;

    public hkaiNavMeshEdgeMatchingParameters m_edgeMatchingParams = new();

    public hkaiNavMeshGeneration.Settings.EdgeMatchingMetric m_edgeMatchingMetric;

    public int m_edgeConnectionIterations;

    public hkaiNavMeshGenerationSettingsRegionPruningSettings m_regionPruningSettings = new();

    public hkaiNavMeshGenerationSettingsWallClimbingSettings m_wallClimbingSettings = new();

    public hkAabb m_boundsAabb = new();

    public List<hkaiCarver?> m_carvers = new();

    public List<hkaiMaterialPainter?> m_painters = new();

    public hkaiNavMeshGeneration.Settings.ConstructionFlagsBits m_defaultConstructionProperties;

    public List<hkaiNavMeshGenerationSettingsMaterialConstructionPair> m_materialMap = new();

    public bool m_fixupOverlappingTriangles;

    public hkaiOverlappingTriangles.Settings m_overlappingTrianglesSettings = new();

    public bool m_weldInputVertices;

    public float m_weldThreshold;

    public float m_minCharacterWidth;

    public hkaiNavMeshGeneration.Settings.CharacterWidthUsage m_characterWidthUsage;

    public bool m_enableSimplification;

    public hkaiNavMeshSimplificationUtils.Settings m_simplificationSettings = new();

    public int m_carvedMaterialDeprecated;

    public int m_carvedCuttingMaterialDeprecated;

    public bool m_setBestFaceCenters;

    public bool m_checkEdgeGeometryConsistency;

    public bool m_saveInputSnapshot;

    public string? m_snapshotFilename;

    public List<hkaiNavMeshGenerationSettingsOverrideSettings> m_overrideSettings = new();


    public enum TriangleWinding : int
    {
        WINDING_CCW = 0,
        WINDING_CW = 1
    }

    public enum EdgeMatchingMetric : int
    {
        ORDER_BY_OVERLAP = 1,
        ORDER_BY_DISTANCE = 2
    }

    [Flags]
    public enum ConstructionFlagsBits : int
    {
        MATERIAL_WALKABLE = 1,
        MATERIAL_CUTTING = 2,
        MATERIAL_WALKABLE_AND_CUTTING = 3
    }

    public enum CharacterWidthUsage : int
    {
        NONE = 0,
        BLOCK_EDGES = 1,
        SHRINK_NAV_MESH = 2
    }

}
