// Automatically Generated

namespace HKLib.hk2018;

public class hkaiVolumeNavigatorStalenessChecker : hkReferencedObject
{
    public hkaiVolumeNavigator? m_navigator;

    public Vector4 m_startPoint = new();

    public List<hkaiVolumeNavigator.Goal> m_goals = new();

    public hkaiVolumeNavigator.NavigatorSettings? m_settings;

    public List<uint> m_pathCellKeys = new();

    public int m_trailingCellIdx;

    public hkAtomic.Variable<hkaiVolumeNavigatorStalenessChecker.State> m_state = new();

    public hkaiVolumeNavigator.PathRequest? m_pathRequest;


    public enum State : byte
    {
        CHECKING = 0,
        TRIGGERED = 1,
        CHECKING_INVALIDATE_ONLY = 2,
        TRIGGERED_INVALIDATE_ONLY = 3,
        CANCELED = 4
    }

}

