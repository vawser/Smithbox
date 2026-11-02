// Automatically Generated

namespace HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting;

public class GetGeometryVolumeClippingSegmentsBasisQuadriplanarOutput : IHavokObject
{
    public List<hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBasisQuadriplanarOutput.Segment> m_segments = new();


    public class Segment : IHavokObject
    {
        public hkaiClipper.BasisQuadriplanarInnerTypes.Vec2 m_a = new();

        public hkaiClipper.BasisQuadriplanarInnerTypes.Vec2 m_b = new();

        public hkaiClipper.BasisQuadriplanarInnerTypes.LineEqn m_lineEqn = new();

    }

}
