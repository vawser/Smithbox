// Automatically Generated

namespace HKLib.hk2018;

public interface hkaiLineOfSightUtil : IHavokObject
{

    [Flags]
    public enum UserEdgeFlagBits : int
    {
        USER_EDGE_IGNORE = 0,
        USER_EDGE_UNTRAVERSABLE_AS_BOUNDARY = 1,
        USER_EDGE_ALL_AS_BOUNDARY = 2,
        USER_EDGE_FOLLOW = 4,
        USER_EDGE_PRIORITIZE_REGULAR_EDGE = 8,
        USER_EDGE_IGNORE_WINDING = 16,
        USER_EDGE_IGNORE_BOUNDARIES_WHEN_TRAVERSING = 32,
        USER_EDGE_ROTATE_DIRECTION = 64,
        USER_EDGE_DISABLED_DEFAULT = 0,
        USER_EDGE_ENABLED_DEFAULT = 124
    }

    public class LineOfSightOutput : IHavokObject
    {
        public List<uint> m_visitedEdgesOut = new();

        public List<float> m_distancesOut = new();

        public List<Vector4> m_pointsOut = new();

        public bool m_doNotExceedArrayCapacity;

        public int m_numIterationsOut;

        public uint m_finalFaceKey;

        public float m_accumulatedDistance;

        public Vector4 m_finalPoint = new();

    }


    public class DirectPathInput : hkaiLineOfSightUtil.InputBase
    {
        public Vector4 m_direction = new();

    }


    public class LineOfSightInput : hkaiLineOfSightUtil.InputBase
    {
        public Vector4 m_goalPoint = new();

        public uint m_goalFaceKey;

    }


    public class InputBase : IHavokObject
    {
        public Vector4 m_startPoint = new();

        public Vector4 m_up = new();

        public uint m_startFaceKey;

        public int m_maxNumberOfIterations;

        public hkaiAgentTraversalInfo m_agentInfo = new();

        public float m_searchRadius;

        public float m_maximumPathLength;

        public hkaiAstarCostModifier? m_costModifier;

        public hkaiAstarEdgeFilter? m_edgeFilter;

        public bool m_outputEdgesOnFailure;

        public bool m_projectedRadiusCheck;

        public bool m_exactInternalVertexHandling;

        public bool m_isWallClimbing;

        public hkaiLineOfSightUtil.InputBase.QueryMode m_mode;

        public hkaiLineOfSightUtil.UserEdgeFlagBits m_userEdgeHandling;

        public bool m_ignoreBackfacingEdges;


        public enum QueryMode : int
        {
            MODE_LINE_OF_SIGHT = 0,
            MODE_DIRECT_PATH = 1
        }

    }


}

