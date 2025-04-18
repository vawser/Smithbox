// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavigator : hkReferencedObject
{
    public ulong m_userData;

    public List<hkaiNavigator.Goal> m_currentGoals = new();

    public Vector4 m_rawPosition = new();

    public bool m_hasPosition;

    public Vector4 m_lastIdealDirection = new();

    public bool m_lastIdealDirectionIsLocalSpace;

    public bool m_hasLastIdealDirection;

    public hkaiNavigator.NavigatorSettings m_settings = new();

    public hkaiWorld? m_world;

    public hkaiNavigator.NavigationOutput m_lastNavigationOutput = new();

    public hkaiNavigator.CornerPredictorInitInfo m_cornerPredictorInitInfo = new();

    public hkaiNavigator.PathRequest? m_pathRequest;

    public bool m_goalReached;

    public hkaiEdgePath? m_edgePath;

    public hkaiNavigatorStalenessChecker? m_stalenessChecker;

    public hkaiEdgePath.TraversalState m_traversalState = new();

    public int m_worldIndex;

    public hkHandle<byte> m_pathRequestQueueId = new();

    public hkaiNavigator.PathQualityBits m_acceptablePathQualities;


    [Flags]
    public enum PathQualityBits : int
    {
        PATHQUALITY_NONE = 0,
        PATHQUALITY_PRELIMINARY = 1,
        PATHQUALITY_PARTIAL = 2,
        PATHQUALITY_FAILED = 4,
        PATHQUALITY_FULL = 8,
        PATHQUALITY_DEFAULT = 11,
        PATHQUALITY_ALL = 15
    }

    public class PathRequest : hkaiNavMeshPathRequest
    {
        public hkaiNavigator? m_navigator;

        public hkaiNavigator.PathRequestInput? m_pathRequestInput;

    }


    public class CornerPredictorInitInfo : IHavokObject
    {
        public Vector4 m_forwardVectorLocal = new();

        public int m_nextEdgeIndex;

        public bool m_nextIsLeft;

    }


    public class PathRequestInput : hkReferencedObject
    {
        public Vector4 m_startPoint = new();

        public List<hkaiNavigator.Goal> m_goals = new();

        public hkaiNavigator.NavigatorSettings? m_settings;

    }


    public class NavigatorSettings : hkReferencedObject
    {
        public Vector4 m_up = new();

        public int m_layerIndex;

        public hkaiNavigatorCapsuleLimit m_capsuleLimit = new();

        public hkaiNavigatorLengthLimit m_lengthLimit = new();

        public int m_iterationLimit;

        public bool m_hasCapsuleLimit;

        public bool m_hasLengthLimit;

        public bool m_internalVertexPullingEnabled;

        public hkaiAgentTraversalInfo m_agentTraversalInfo = new();

        public hkaiNavMeshPathSearchParameters m_searchParameters = new();

        public hkaiAstarEdgeFilter? m_edgeFilter;

        public hkaiAstarCostModifier? m_costModifier;

        public float m_searchRadius;

        public float m_horizontalTolerance;

        public float m_verticalTolerance;

        public float m_goalReachedDistance;

        public float m_userEdgeEnteredDistance;

        public float m_leftTurningRadiusMultiplier;

        public float m_rightTurningRadiusMultiplier;

    }


    public class Goal : IHavokObject
    {
        public Vector4 m_position = new();

        public int m_section;

    }


    public class NavigationOutput : IHavokObject
    {
        public hkaiNavigator.NavigationOutput.Result m_result;

        public hkaiNavigator.PathQualityBits m_pathQuality;

        public bool m_isPathStale;

        public bool m_isOldInput;

        public uint m_faceKey;

        public Vector4 m_correctedPosition = new();

        public Vector4 m_correctedGoal = new();

        public Vector4 m_idealDirection = new();

        public Vector4 m_surfaceVelocity = new();


        public enum Result : int
        {
            RESULT_SUCCESS = 0,
            RESULT_NO_INPUT = 1,
            RESULT_WAITING = 2,
            RESULT_AT_GOAL = 3
        }

    }


}

