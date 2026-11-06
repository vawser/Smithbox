// Automatically Generated

namespace Havok.hk2014;

public partial class hkaiNavMesh : hkReferencedObject
{
    public List<hkaiNavMesh.Face> m_faces = new();

    public List<hkaiNavMesh.Edge> m_edges = new();

    public List<Vector4> m_vertices = new();

    public List<hkaiStreamingSet> m_streamingSets = new();

    public List<int> m_faceData = new();

    public List<int> m_edgeData = new();

    public int m_faceDataStriding;

    public int m_edgeDataStriding;

    public byte m_flags;

    public hkAabb m_aabb = new();

    public float m_erosionRadius;

    public ulong m_userData;


    public class Face : IHavokObject
    {
        public int m_startEdgeIndex;

        public int m_startUserEdgeIndex;

        public short m_numEdges;

        public short m_numUserEdges;

        public short m_clusterIndex;

        public ushort m_padding;

    }

    public class Edge : IHavokObject
    {
        public int m_a;

        public int m_b;

        public uint m_oppositeEdge;

        public uint m_oppositeFace;

        public hkaiNavMesh.EdgeFlagBits m_flags;

        public float m_userEdgeCost;

    }

    [Flags]
    public enum EdgeFlagBits : int
    {
        EDGE_SILHOUETTE = 1,
        EDGE_RETRIANGULATED = 2,
        EDGE_ORIGINAL = 4,
        OPPOSITE_EDGE_UNLOADED_UNUSED = 8,
        EDGE_USER = 16,
        EDGE_BLOCKED = 32,
        EDGE_EXTERNAL_OPPOSITE = 64
    }

}
