// Automatically Generated

namespace HKLib.hk2018.hkaiCollisionAvoidance;

public class SimpleObstacleGenerator : hkaiCollisionAvoidance.ObstacleGenerator
{
    public bool m_clipLineSegments;

    public hkaiReferenceFrame m_referenceFrame = new();

    public List<hkaiCollisionAvoidance.SphereObstacle> m_spheres = new();

    public List<hkaiCollisionAvoidance.LineSegmentObstacle> m_lineSegments = new();

    public hkAabb m_localAabb = new();

}

