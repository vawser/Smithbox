// Automatically Generated

namespace HKLib.hk2018;

public class hkaiStreamingSet : hkReferencedObject
{
    public uint m_aSectionUid;

    public uint m_bSectionUid;

    public List<hkaiStreamingSet.NavMeshConnection> m_meshConnections = new();

    public List<hkaiStreamingSet.GraphConnection> m_graphConnections = new();

    public List<hkaiStreamingSet.VolumeConnection> m_volumeConnections = new();

    public List<hkAabb> m_aConnectionAabbs = new();

    public List<hkAabb> m_bConnectionAabbs = new();


    public class VolumeConnection : IHavokObject
    {
        public int m_aCellIndex;

        public int m_bCellIndex;

    }


    public class GraphConnection : IHavokObject
    {
        public int m_aNodeIndex;

        public int m_bNodeIndex;

        public uint m_aEdgeData;

        public uint m_bEdgeData;

        public float m_aEdgeCost;

        public float m_bEdgeCost;

    }


    public class NavMeshConnection : IHavokObject
    {
        public hkaiFaceEdgeIndexPair m_aFaceEdgeIndex = new();

        public hkaiFaceEdgeIndexPair m_bFaceEdgeIndex = new();

    }


}

