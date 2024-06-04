// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavigatorStalenessChecker : hkReferencedObject
{
    public hkaiNavigator? m_navigator;

    public Vector4 m_startPoint = new();

    public List<hkaiNavigator.Goal> m_goals = new();

    public hkaiNavigator.NavigatorSettings? m_settings;

    public List<uint> m_pathFaceKeys = new();

    public int m_trailingEdge;

    public hkAtomic.Variable<hkaiNavigatorStalenessChecker.State> m_state = new();

    public hkaiNavigator.PathRequest? m_pathRequest;

    public List<int> m_removedGoalIndices = new();


    public enum State : byte
    {
        CHECKING = 0,
        TRIGGERED = 1,
        CHECKING_INVALIDATE_ONLY = 2,
        TRIGGERED_INVALIDATE_ONLY = 3,
        CANCELED = 4
    }

}

