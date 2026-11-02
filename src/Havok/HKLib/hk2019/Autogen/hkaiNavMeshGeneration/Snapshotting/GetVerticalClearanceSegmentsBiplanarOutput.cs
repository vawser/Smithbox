// Automatically Generated

namespace HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting;

public class GetVerticalClearanceSegmentsBiplanarOutput : IHavokObject
{
    public List<hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBiplanarOutput.Segment> m_segments = new();


    public class Segment : IHavokObject
    {
        public hkaiClipper.BiplanarInnerTypes.Vec2 m_a = new();

        public hkaiClipper.BiplanarInnerTypes.Vec2 m_b = new();

        public hkaiClipper.BiplanarInnerTypes.LineEqn m_lineEqn = new();

        public int m_fromExtrusion;

        public int m_toExtrusion;

        public bool m_blocksConnection;

    }

}
