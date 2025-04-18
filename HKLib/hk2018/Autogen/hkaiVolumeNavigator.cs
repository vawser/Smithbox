// Automatically Generated

namespace HKLib.hk2018;

public class hkaiVolumeNavigator : hkReferencedObject
{
    public ulong m_userData;

    public List<hkaiVolumeNavigator.Goal> m_currentGoals = new();

    public Vector4 m_rawPosition = new();

    public Vector4 m_lastIdealDirection = new();

    public bool m_hasLastIdealDirection;

    public hkaiVolumeNavigator.NavigatorSettings m_settings = new();

    public bool m_hasPosition;

    public hkaiWorld? m_world;

    public hkaiVolumeNavigator.NavigationOutput m_lastNavigationOutput = new();

    public hkaiVolumeNavigator.PathRequest? m_pathRequest;

    public bool m_goalReached;

    public hkaiGatePath? m_gatePath;

    public hkaiVolumeNavigatorStalenessChecker? m_stalenessChecker;

    public hkaiGatePath.TraversalState m_traversalState = new();

    public int m_highestReportedUserEdgeEntry;

    public int m_worldIndex;

    public hkHandle<byte> m_pathRequestQueueId = new();

    public hkaiVolumeNavigator.PathQualityBits m_acceptablePathQualities;


    [Flags]
    public enum PathQualityBits : int
    {
        PATHQUALITY_NONE = 0,
        PATHQUALITY_PRELIMINARY = 1,
        PATHQUALITY_PARTIAL = 2,
        PATHQUALITY_FAILED = 4,
        PATHQUALITY_FULL = 8,
        PATHQUALITY_ALL = 15
    }

    public class PathRequest : hkaiNavVolumePathRequest
    {
        public hkaiVolumeNavigator? m_navigator;

        public hkaiVolumeNavigator.PathRequestInput? m_pathRequestInput;

    }


    public class PathRequestInput : hkReferencedObject
    {
        public Vector4 m_startPoint = new();

        public List<hkaiVolumeNavigator.Goal> m_goals = new();

        public hkaiVolumeNavigator.NavigatorSettings? m_settings;

    }


    public class NavigatorSettings : hkReferencedObject
    {
        public Vector4 m_up = new();

        public int m_iterationLimit;

        public int m_layerIndex;

        public float m_searchRadius;

        public float m_goalReachedDistance;

        public float m_userEdgeEnteredDistance;

        public hkaiAgentTraversalInfo m_agentTraversalInfo = new();

        public hkaiNavVolumePathSearchParameters m_searchParameters = new();

        public hkaiAstarEdgeFilter? m_edgeFilter;

        public hkaiAstarCostModifier? m_costModifier;

    }


    public class Goal : IHavokObject
    {
        public Vector4 m_position = new();

    }


    public class NavigationOutput : IHavokObject
    {
        public hkaiVolumeNavigator.NavigationOutput.Result m_result;

        public hkaiVolumeNavigator.PathQualityBits m_pathQuality;

        public bool m_isPathStale;

        public bool m_isOldInput;

        public uint m_cellKey;

        public Vector4 m_correctedPosition = new();

        public Vector4 m_correctedGoal = new();

        public Vector4 m_idealDirection = new();


        public enum Result : int
        {
            RESULT_SUCCESS = 0,
            RESULT_NO_INPUT = 1,
            RESULT_WAITING = 2,
            RESULT_AT_GOAL = 3
        }

    }


}

