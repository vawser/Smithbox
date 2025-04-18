// Automatically Generated

namespace HKLib.hk2018;

public interface hkaiDynamicUserEdgeSetInfoBase : IHavokObject
{

    public class ClusterGraphEdgeAndCount : IHavokObject
    {
        public hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdge m_edge = new();

        public int m_userEdgeCount;

    }


    public class ClusterGraphEdge : IHavokObject
    {
        public uint m_startClusterKey;

        public uint m_endClusterKey;

    }


    public class SectionIdxPair : IHavokObject
    {
        public int m_aSectionIdx;

        public int m_bSectionIdx;

    }


}

