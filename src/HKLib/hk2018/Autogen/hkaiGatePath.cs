// Automatically Generated

namespace HKLib.hk2018;

public class hkaiGatePath : hkReferencedObject
{
    public List<hkaiGatePath.PathGate> m_gates = new();

    public Vector4 m_startPoint = new();

    public hkaiGatePathUtil.ExponentialSchedule m_schedule = new();

    public bool m_needsInitialSmooth;


    [Flags]
    public enum BoundarySegmentBits : int
    {
        BOUNDARYSEGMENT_MINV = 1,
        BOUNDARYSEGMENT_MAXU = 2,
        BOUNDARYSEGMENT_MAXV = 4,
        BOUNDARYSEGMENT_MINU = 8,
        BOUNDARYSEGMENT_ALL = 15
    }

    public class PathGate : IHavokObject
    {
        public Vector4 m_crossingPoint = new();

        public hkaiGatePathUtil.Gate m_gate = new();

        public hkaiGatePath.BoundarySegmentBits m_boundarySegments;

        public uint m_cellKey;

        public int m_edgeIndex;

    }


    public class SmoothOptions : IHavokObject
    {
        public int m_minRounds;

        public int m_maxRounds;

        public int m_initialMinRounds;

        public int m_initialMaxRounds;

        public float m_quiescenceDistance;

        public float m_quiescenceSinAngle;

    }


    public class TraversalState : IHavokObject
    {
        public Vector4 m_curPos = new();

        public int m_curCellIdx;

    }


}

