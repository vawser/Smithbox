// Automatically Generated

namespace HKLib.hk2018;

public class hkaiDirectedGraphExplicitCost : hkReferencedObject
{
    public List<Vector4> m_positions = new();

    public List<hkaiDirectedGraphExplicitCost.Node> m_nodes = new();

    public List<hkaiDirectedGraphExplicitCost.Edge> m_edges = new();

    public List<uint> m_nodeData = new();

    public List<uint> m_edgeData = new();

    public int m_nodeDataStriding;

    public int m_edgeDataStriding;

    public List<hkaiAnnotatedStreamingSet> m_streamingSets = new();


    [Flags]
    public enum EdgeBits : int
    {
        EDGE_IS_USER = 2,
        EDGE_EXTERNAL_OPPOSITE = 64
    }

    public class Edge : IHavokObject
    {
        public float m_cost;

        public hkaiDirectedGraphExplicitCost.EdgeBits m_flags;

        public uint m_target;

    }


    public class Node : IHavokObject
    {
        public int m_startEdgeIndex;

        public int m_numEdges;

    }


}

