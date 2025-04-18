// Automatically Generated

namespace HKLib.hk2018;

public class hkaiObstacleGenerator : hkReferencedObject
{
    public bool m_useSpheres;

    public bool m_useBoundaries;

    public bool m_clipBoundaries;

    public Matrix4x4 m_transform = new();

    public List<hkaiAvoidanceSolver.SphereObstacle> m_spheres = new();

    public List<hkaiAvoidanceSolver.BoundaryObstacle> m_boundaries = new();

    public ulong m_userData;

}

