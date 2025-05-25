// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavMeshPathSearchParameters : IHavokObject
{
    public Vector4 m_up = new();

    public bool m_validateInputs;

    public hkaiNavMeshPathSearchParameters.OutputPathFlags m_outputPathFlags;

    public hkaiNavMeshPathSearchParameters.LineOfSightFlags m_lineOfSightFlags;

    public bool m_useHierarchicalHeuristic;

    public bool m_projectedRadiusCheck;

    public bool m_useGrandparentDistanceCalculation;

    public bool m_outputUnreachablePaths;

    public bool m_recordClearanceCacheMisses;

    public float m_heuristicWeight;

    public float m_simpleRadiusThreshold;

    public float m_maximumPathLength;

    public float m_searchSphereRadius;

    public float m_searchCapsuleRadius;

    public hkaiSearchParameters.BufferSizes m_bufferSizes = new();

    public hkaiSearchParameters.BufferSizes m_hierarchyBufferSizes = new();


    [Flags]
    public enum LineOfSightFlags : int
    {
        NO_LINE_OF_SIGHT_CHECK = 0,
        EARLY_OUT_IF_NO_COST_MODIFIER = 1,
        HANDLE_INTERNAL_VERTICES = 2,
        EARLY_OUT_ALWAYS = 4
    }

    [Flags]
    public enum OutputPathFlags : int
    {
        OUTPUT_PATH_SMOOTHED = 1,
        OUTPUT_PATH_PROJECTED = 2,
        OUTPUT_PATH_COMPUTE_NORMALS = 4
    }

}

