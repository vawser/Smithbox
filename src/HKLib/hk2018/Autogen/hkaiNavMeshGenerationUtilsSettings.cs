// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavMeshGenerationUtilsSettings : hkReferencedObject
{
    public float m_characterHeight;

    public Vector4 m_up = new();

    public float m_quantizationGridSize;

    public float m_maxWalkableSlope;

    public hkaiNavMeshGenerationUtilsSettings.TriangleWinding m_triangleWinding;

    public float m_degenerateAreaThreshold;

    public float m_degenerateWidthThreshold;

    public float m_convexThreshold;

    public int m_maxNumEdgesPerFace;

    public hkaiNavMeshEdgeMatchingParameters m_edgeMatchingParams = new();

    public hkaiNavMeshGenerationUtilsSettings.EdgeMatchingMetric m_edgeMatchingMetric;

    public int m_edgeConnectionIterations;

    public bool m_smallBoundaryEdgeGroupRemoval;

    public hkaiNavMeshGenerationUtilsSettings.RegionPruningSettings m_regionPruningSettings = new();

    public hkaiNavMeshGenerationUtilsSettings.WallClimbingSettings m_wallClimbingSettings = new();

    public hkAabb m_boundsAabb = new();

    public List<hkaiCarver?> m_carvers = new();

    public List<hkaiMaterialPainter?> m_painters = new();

    public hkaiNavMeshGenerationUtilsSettings.ConstructionFlagsBits m_defaultConstructionProperties;

    public List<hkaiNavMeshGenerationUtilsSettings.MaterialConstructionPair> m_materialMap = new();

    public bool m_fixupOverlappingTriangles;

    public hkaiOverlappingTriangles.Settings m_overlappingTrianglesSettings = new();

    public bool m_swapOverlappingAndQuantization;

    public bool m_weldInputVertices;

    public float m_weldThreshold;

    public float m_minCharacterWidth;

    public hkaiNavMeshGenerationUtilsSettings.CharacterWidthUsage m_characterWidthUsage;

    public float m_maxCharacterWidth;

    public bool m_precalculateClearanceSeedingData;

    public bool m_enableSimplification;

    public hkaiNavMeshSimplificationUtils.Settings m_simplificationSettings = new();

    public int m_carvedMaterialDeprecated;

    public int m_carvedCuttingMaterialDeprecated;

    public bool m_checkEdgeGeometryConsistency;

    public bool m_saveInputSnapshot;

    public string? m_snapshotFilename;

    public List<hkaiNavMeshGenerationUtilsSettings.OverrideSettings> m_overrideSettings = new();


    public enum EdgeMatchingMetric : int
    {
        ORDER_BY_OVERLAP = 1,
        ORDER_BY_DISTANCE = 2
    }

    public enum TriangleWinding : int
    {
        WINDING_CCW = 0,
        WINDING_CW = 1
    }

    public enum CharacterWidthUsage : int
    {
        NONE = 0,
        BLOCK_EDGES = 1,
        SHRINK_NAV_MESH = 2
    }

    [Flags]
    public enum ConstructionFlagsBits : int
    {
        MATERIAL_WALKABLE = 1,
        MATERIAL_CUTTING = 2,
        MATERIAL_WALKABLE_AND_CUTTING = 3
    }

    public class WallClimbingSettings : IHavokObject
    {
        public bool m_enableWallClimbing;

        public bool m_excludeWalkableFaces;

    }


    public class RegionPruningSettings : IHavokObject
    {
        public float m_minRegionArea;

        public float m_minDistanceToSeedPoints;

        public float m_borderPreservationTolerance;

        public bool m_preserveVerticalBorderRegions;

        public bool m_pruneBeforeTriangulation;

        public List<Vector4> m_regionSeedPoints = new();

        public List<hkaiNavMeshGenerationUtilsSettings.RegionPruningSettings.RegionConnection> m_regionConnections = new();


        public class RegionConnection : IHavokObject
        {
            public Vector4 m_a = new();

            public Vector4 m_b = new();

        }


    }


    public class OverrideSettings : IHavokObject
    {
        public hkaiVolume? m_volume;

        public int m_material;

        public hkaiNavMeshGenerationUtilsSettings.CharacterWidthUsage m_characterWidthUsage;

        public float m_maxWalkableSlope;

        public hkaiNavMeshEdgeMatchingParameters m_edgeMatchingParams = new();

        public hkaiNavMeshSimplificationUtils.Settings m_simplificationSettings = new();

    }


    public class MaterialConstructionPair : IHavokObject
    {
        public int m_materialIndex;

        public hkaiNavMeshGenerationUtilsSettings.ConstructionFlagsBits m_flags;

    }


}

