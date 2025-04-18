// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavMeshClearanceCache : hkReferencedObject
{
    public float m_clearanceCeiling;

    public float m_clearanceIntToRealMultiplier;

    public float m_clearanceRealToIntMultiplier;

    public List<uint> m_faceOffsets = new();

    public List<byte> m_edgePairClearances = new();

    public int m_unusedEdgePairElements;

    public List<hkaiNavMeshClearanceCache.McpDataInteger> m_mcpData = new();

    public List<byte> m_vertexClearances = new();

    public int m_uncalculatedFacesLowerBound;


    public class McpDataInteger : IHavokObject
    {
        public byte m_interpolant;

        public byte m_clearance;

    }


}

