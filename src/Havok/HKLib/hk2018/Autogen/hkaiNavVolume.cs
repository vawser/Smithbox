// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavVolume : hkReferencedObject
{
    public List<hkaiNavVolume.Cell> m_cells = new();

    public List<hkaiNavVolume.Edge> m_edges = new();

    public List<hkaiNavVolume.UserEdgeInfo> m_userEdgeInfos = new();

    public List<int> m_userEdgeData = new();

    public List<hkaiAnnotatedStreamingSet> m_streamingSets = new();

    public hkAabb m_aabb = new();

    public Vector4 m_quantizationScale = new();

    public Vector4 m_quantizationOffset = new();

    public readonly int[] m_res = new int[3];

    public int m_userEdgeDataStriding;

    public ulong m_userData;


    [Flags]
    public enum CellEdgeFlagBits : int
    {
        EDGE_OWNED = 8,
        EDGE_USER = 16,
        EDGE_EXTERNAL_OPPOSITE = 64
    }

    public class UserEdgeInfo : IHavokObject
    {
        public readonly ushort[] m_entryOrigin = new ushort[3];

        public ushort m_uExtent;

        public readonly ushort[] m_exitOrigin = new ushort[3];

        public ushort m_vExtent;

        public hkaiNavVolume.AxisPermutation m_entryAxisPermutation = new();

        public hkaiNavVolume.AxisPermutation m_exitAxisPermutation = new();

        public float m_cost;

    }


    public class AxisPermutation : IHavokObject
    {
        public byte m_data;

    }


    public class Edge : IHavokObject
    {
        public ushort m_userEdgeInfoIndex;

        public hkaiNavVolume.CellEdgeFlagBits m_flags;

        public uint m_oppositeCell;

    }


    public class Cell : IHavokObject
    {
        public readonly ushort[] m_min = new ushort[3];

        public short m_numEdges;

        public readonly ushort[] m_max = new ushort[3];

        public int m_startEdgeIndex;

        public int m_data;

    }


}

