// Automatically Generated

namespace Havok.hk2014;

public class hkaiNavVolume : hkReferencedObject
{
    public List<hkaiNavVolume.Cell> m_cells = new();

    public List<hkaiNavVolume.Edge> m_edges = new();

    public List<hkaiStreamingSet> m_streamingSets = new();

    public hkAabb m_aabb = new();

    public Vector4 m_quantizationScale = new();

    public Vector4 m_quantizationOffset = new();

    public readonly int[] m_res = new int[3];

    public ulong m_userData;


    public class Cell : IHavokObject
    {
        public readonly ushort[] m_min = new ushort[3];

        public short m_numEdges;

        public readonly ushort[] m_max = new ushort[3];

        public int m_startEdgeIndex;

        public int m_data;

    }

    public class Edge : IHavokObject
    {
        public hkaiNavVolume.CellEdgeFlagBits m_flags;

        public uint m_oppositeCell;

    }

    [Flags]
    public enum CellEdgeFlagBits : int
    {
        EDGE_EXTERNAL_OPPOSITE = 64
    }

}
