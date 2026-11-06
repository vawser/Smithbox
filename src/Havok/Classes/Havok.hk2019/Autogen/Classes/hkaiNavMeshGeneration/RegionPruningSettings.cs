// Automatically Generated

namespace Havok.hk2019.hkaiNavMeshGeneration;

public class RegionPruningSettings : IHavokObject
{
    public float m_minRegionArea;

    public float m_minDistanceToSeedPoints;

    public List<Vector4> m_regionSeedPoints = new();

    public List<hkaiNavMeshGeneration.RegionPruningSettings.RegionConnection> m_regionConnections = new();

    public hkAabb m_pruningAabb = new();


    public class RegionConnection : IHavokObject
    {
        public Vector4 m_a = new();

        public Vector4 m_b = new();

    }

}
