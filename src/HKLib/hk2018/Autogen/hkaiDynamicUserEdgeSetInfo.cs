// Automatically Generated

namespace HKLib.hk2018;

public class hkaiDynamicUserEdgeSetInfo : hkaiDynamicUserEdgeSetInfoBase
{
    public hkHandle<uint> m_setId = new();

    public hkHashMap<int, hkaiDynamicUserEdgeSetInfo.Section> m_sections = new();


    public class ExternalEdges : hkReferencedObject
    {
        public hkaiDynamicUserEdgeSetInfoBase.SectionIdxPair m_sectionIndices = new();

        public hkHashSet<hkaiDynamicUserEdgeSetInfo.UserEdgePair> m_edges = new();

        public hkHashMap<hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdge, int> m_clusterGraphEdges = new();

    }


    public class Section : IHavokObject
    {
        public hkHashSet<int> m_faceIndices = new();

        public hkHashMap<hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdge, int> m_clusterGraphEdges = new();

        public hkHashMap<int, hkaiDynamicUserEdgeSetInfo.ExternalEdges?> m_externalEdges = new();

    }


    public class UserEdgePair : IHavokObject
    {
        public uint m_aFaceKey;

        public uint m_bFaceKey;

    }


}

