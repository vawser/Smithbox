// Automatically Generated

namespace HKLib.hk2018.hkaiNavMeshSimplificationUtils;

public class Settings : IHavokObject
{
    public float m_maxBorderSimplifyArea;

    public float m_maxConcaveBorderSimplifyArea;

    public float m_minCorridorWidth;

    public float m_maxCorridorWidth;

    public float m_holeReplacementArea;

    public float m_aabbReplacementAreaFraction;

    public float m_maxLoopShrinkFraction;

    public float m_maxBorderHeightError;

    public float m_maxBorderDistanceError;

    public int m_maxPartitionSize;

    public bool m_useHeightPartitioning;

    public float m_maxPartitionHeightError;

    public bool m_useConservativeHeightPartitioning;

    public float m_hertelMehlhornHeightError;

    public float m_cosPlanarityThreshold;

    public float m_nonconvexityThreshold;

    public float m_boundaryEdgeFilterThreshold;

    public float m_maxSharedVertexHorizontalError;

    public float m_maxSharedVertexVerticalError;

    public float m_maxBoundaryVertexHorizontalError;

    public float m_maxBoundaryVertexVerticalError;

    public bool m_mergeLongestEdgesFirst;

    public hkaiNavMeshSimplificationUtils.ExtraVertexSettings m_extraVertexSettings = new();

    public bool m_saveInputSnapshot;

    public string? m_snapshotFilename;

}

