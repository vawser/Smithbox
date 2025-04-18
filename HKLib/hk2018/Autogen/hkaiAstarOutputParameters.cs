// Automatically Generated

namespace HKLib.hk2018;

public class hkaiAstarOutputParameters : IHavokObject
{
    public int m_numIterations;

    public int m_goalIndex;

    public float m_pathLength;

    public hkaiAstarOutputParameters.SearchStatus m_status;

    public hkaiAstarOutputParameters.TerminationCause m_terminationCause;


    public enum TerminationCause : int
    {
        NOT_TERMINATED = 0,
        TERMINATED_ITERATION_LIMIT = 1,
        TERMINATED_OPEN_SET_FULL = 2,
        TERMINATED_SEARCH_STATE_FULL = 3,
        TERMINATED_BY_USER = 4
    }

    public enum SearchStatus : int
    {
        SEARCH_IN_PROGRESS = 0,
        SEARCH_SUCCEEDED = 1,
        SEARCH_UNREACHABLE = 2,
        SEARCH_TERMINATED = 3,
        SEARCH_SUCCEEDED_BUT_RESULTS_TRUNCATED = 4,
        SEARCH_INVALID = 5
    }

}

