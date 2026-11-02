// Automatically Generated

namespace HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting;

public class GeometryFacesIntersectXyPrismInput : IHavokObject
{
    public hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismInput.XyPrism m_prism = new();

    public hkAabb m_prismAabb = new();


    public class XyPrism : IHavokObject
    {
        public List<hkaiClipper.QuadriplanarInnerTypes.LineEqn> m_planes = new();

        public List<hkaiClipper.QuadriplanarInnerTypes.Vec2> m_points = new();

        public List<byte> m_minZ = new();

        public List<byte> m_maxZ = new();

    }

}
