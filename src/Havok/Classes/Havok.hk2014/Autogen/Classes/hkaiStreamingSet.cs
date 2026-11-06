// Automatically Generated

namespace Havok.hk2014;

public class hkaiStreamingSet : IHavokObject
{
    public uint m_thisUid;

    public uint m_oppositeUid;

    public List<hkaiStreamingSet.NavMeshConnection> m_meshConnections = new();

    public List<hkaiStreamingSet.GraphConnection> m_graphConnections = new();

    public List<hkaiStreamingSet.VolumeConnection> m_volumeConnections = new();


    public class NavMeshConnection : IHavokObject
    {
        public int m_faceIndex;

        public int m_edgeIndex;

        public int m_oppositeFaceIndex;

        public int m_oppositeEdgeIndex;

    }

    public class GraphConnection : IHavokObject
    {
        public int m_nodeIndex;

        public int m_oppositeNodeIndex;

        public uint m_edgeData;

        public float m_edgeCost;

    }

    public class VolumeConnection : IHavokObject
    {
        public int m_cellIndex;

        public int m_oppositeCellIndex;

    }

}
