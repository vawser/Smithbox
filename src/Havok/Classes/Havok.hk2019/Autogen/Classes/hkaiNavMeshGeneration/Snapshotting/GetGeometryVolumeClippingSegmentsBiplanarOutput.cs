// Automatically Generated

namespace Havok.hk2019.hkaiNavMeshGeneration.Snapshotting;

public class GetGeometryVolumeClippingSegmentsBiplanarOutput : IHavokObject
{
    public List<hkaiNavMeshGeneration.Snapshotting.GetGeometryVolumeClippingSegmentsBiplanarOutput.Segment> m_segments = new();


    public class Segment : IHavokObject
    {
        public hkaiClipper.BiplanarInnerTypes.Vec2 m_a = new();

        public hkaiClipper.BiplanarInnerTypes.Vec2 m_b = new();

        public hkaiClipper.BiplanarInnerTypes.LineEqn m_lineEqn = new();

    }

}
