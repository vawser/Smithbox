// Automatically Generated

namespace HKLib.hk2018;

public class hkaiDirectedGraphInstanceData : hkReferencedObject
{
    public uint m_sectionUid;

    public int m_runtimeId;

    public hkaiDirectedGraphExplicitCost? m_originalGraph;

    public List<int> m_nodeMap = new();

    public List<hkaiDirectedGraphExplicitCost.Node> m_instancedNodes = new();

    public List<hkaiDirectedGraphExplicitCost.Edge> m_ownedEdges = new();

    public List<uint> m_ownedEdgeData = new();

    public List<int> m_userEdgeCount = new();

    public List<hkaiDirectedGraphInstanceData.FreeBlockList> m_freeEdgeBlocks = new();


    public class FreeBlockList : IHavokObject
    {
        public List<int> m_blocks = new();

    }


}

