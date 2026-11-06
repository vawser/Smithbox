// Automatically Generated

namespace Havok.hk2014;

public class hkaiPointCloudSilhouetteGenerator : hkaiSilhouetteGenerator
{
    public hkAabb m_localAabb = new();

    public List<Vector4> m_localPoints = new();

    public List<int> m_silhouetteSizes = new();

    public float m_weldTolerance;

    public hkaiPointCloudSilhouetteGenerator.DetailLevel m_silhouetteDetailLevel;

    public hkaiPointCloudSilhouetteGenerator.PointCloudFlagBits m_flags;

    public bool m_localPointsChanged;

    public bool m_isEnabled;


    public enum DetailLevel : int
    {
        DETAIL_INVALID = 0,
        DETAIL_FULL = 1,
        DETAIL_OBB = 2,
        DETAIL_CONVEX_HULL = 3
    }

    [Flags]
    public enum PointCloudFlagBits : int
    {
        LOCAL_POINTS_CHANGED = 1
    }

}
