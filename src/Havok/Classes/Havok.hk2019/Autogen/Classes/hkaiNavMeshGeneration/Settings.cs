// Automatically Generated

namespace Havok.hk2019.hkaiNavMeshGeneration;

public class Settings : IHavokObject
{
    public float m_quantum;

    public Vector4 m_up = new();

    public float m_minCharacterRadius;

    public float m_maxCharacterRadius;

    public float m_maxStepHeight;

    public float m_maxGapWidth;

    public bool m_canonicalize;

    public int m_faceUserdataStriding;

    public bool m_precalculateFaceIterator;

    public bool m_simplify;

    public bool m_pruneRegions;

    public hkaiNavMeshGeneration.RegionPruningSettings m_regionPruningSettings = new();

    public hkaiNavMeshSimplificationUtils.Settings m_simplificationSettings = new();

    public List<hkaiNavMeshGeneration.Settings.SimplificationOverride> m_simplificationOverrides = new();

    public hkAabb m_bounds = new();

    public hkaiNavMeshGeneration.Settings.OutputTypes m_outputTypes;

    public string? m_snapshotFilename;


    [Flags]
    public enum OutputTypes : int
    {
        NAVMESH = 1,
        TRAVERSAL_ANALYSIS_INPUT = 2,
        INTERMEDIATE_RESULTS = 4,
        SNAPSHOT = 8
    }

    public class SimplificationOverride : IHavokObject
    {
        public hkaiVolume? m_volume;

        public int m_data;

        public hkaiNavMeshSimplificationUtils.Settings m_simplificationSettings = new();

    }

}
