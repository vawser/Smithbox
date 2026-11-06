// Automatically Generated

namespace Havok.hk2014;

public class hkaiDirectedGraphInstance : hkReferencedObject
{
    public uint m_sectionUid;

    public int m_runtimeId;

    public hkaiDirectedGraphExplicitCost? m_originalGraph;

    public List<int> m_nodeMap = new();

    public List<hkaiDirectedGraphExplicitCost.Node> m_instancedNodes = new();

    public List<hkaiDirectedGraphExplicitCost.Edge> m_ownedEdges = new();

    public List<uint> m_ownedEdgeData = new();

    public List<int> m_userEdgeCount = new();

    public List<hkaiDirectedGraphInstanceFreeBlockList> m_freeEdgeBlocks = new();

    public Matrix4x4 m_transform = new();

}
