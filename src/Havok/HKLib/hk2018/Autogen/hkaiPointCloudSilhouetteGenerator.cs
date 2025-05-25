// Automatically Generated

namespace HKLib.hk2018;

public class hkaiPointCloudSilhouetteGenerator : hkaiSilhouetteGenerator
{
    public hkAabb m_localAabb = new();

    public List<Vector4> m_localPoints = new();

    public List<int> m_silhouetteSizes = new();

    public float m_weldTolerance;

    public hkaiPointCloudSilhouetteGenerator.DetailLevel m_silhouetteDetailLevel;

    public hkaiPointCloudSilhouetteGenerator.PointCloudFlagBits m_flags;

    public bool m_localPointsChanged;

    public bool m_isEnabledChanged;

    public bool m_isEnabled;


    [Flags]
    public enum PointCloudFlagBits : int
    {
        LOCAL_POINTS_CHANGED = 1,
        ENABLED_CHANGED = 2
    }

    public enum DetailLevel : int
    {
        DETAIL_INVALID = 0,
        DETAIL_FULL = 1,
        DETAIL_OBB = 2,
        DETAIL_CONVEX_HULL = 3
    }

}

