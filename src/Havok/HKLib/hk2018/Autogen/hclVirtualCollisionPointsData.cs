// Automatically Generated

namespace HKLib.hk2018;

public class hclVirtualCollisionPointsData : IHavokObject
{
    public List<hclVirtualCollisionPointsData.Block> m_blocks = new();

    public ushort m_numVCPoints;

    public List<ushort> m_landscapeParticlesBlockIndex = new();

    public ushort m_numLandscapeVCPoints;

    public List<float> m_edgeBarycentricsDictionary = new();

    public List<hclVirtualCollisionPointsData.BarycentricDictionaryEntry> m_edgeDictionaryEntries = new();

    public List<hclVirtualCollisionPointsData.BarycentricPair> m_triangleBarycentricsDictionary = new();

    public List<hclVirtualCollisionPointsData.BarycentricDictionaryEntry> m_triangleDictionaryEntries = new();

    public List<hclVirtualCollisionPointsData.EdgeFanSection> m_edges = new();

    public List<hclVirtualCollisionPointsData.EdgeFan> m_edgeFans = new();

    public List<hclVirtualCollisionPointsData.TriangleFanSection> m_triangles = new();

    public List<hclVirtualCollisionPointsData.TriangleFan> m_triangleFans = new();

    public List<hclVirtualCollisionPointsData.EdgeFanSection> m_edgesLandscape = new();

    public List<hclVirtualCollisionPointsData.EdgeFanLandscape> m_edgeFansLandscape = new();

    public List<hclVirtualCollisionPointsData.TriangleFanSection> m_trianglesLandscape = new();

    public List<hclVirtualCollisionPointsData.TriangleFanLandscape> m_triangleFansLandscape = new();

    public List<ushort> m_edgeFanIndices = new();

    public List<ushort> m_triangleFanIndices = new();

    public List<ushort> m_edgeFanIndicesLandscape = new();

    public List<ushort> m_triangleFanIndicesLandscape = new();


    public class BarycentricPair : IHavokObject
    {
        public float m_u;

        public float m_v;

    }


    public class EdgeFanLandscape : IHavokObject
    {
        public ushort m_realParticleIndex;

        public ushort m_edgeStartIndex;

        public ushort m_vcpStartIndex;

        public byte m_numEdges;

    }


    public class EdgeFan : IHavokObject
    {
        public ushort m_realParticleIndex;

        public ushort m_edgeStartIndex;

        public byte m_numEdges;

    }


    public class EdgeFanSection : IHavokObject
    {
        public ushort m_oppositeRealParticleIndex;

        public ushort m_barycentricDictionaryIndex;

    }


    public class TriangleFanLandscape : IHavokObject
    {
        public ushort m_realParticleIndex;

        public ushort m_triangleStartIndex;

        public ushort m_vcpStartIndex;

        public byte m_numTriangles;

    }


    public class TriangleFan : IHavokObject
    {
        public ushort m_realParticleIndex;

        public ushort m_vcpStartIndex;

        public byte m_numTriangles;

    }


    public class TriangleFanSection : IHavokObject
    {
        public readonly ushort[] m_oppositeRealParticleIndices = new ushort[2];

        public ushort m_barycentricDictionaryIndex;

    }


    public class BarycentricDictionaryEntry : IHavokObject
    {
        public ushort m_startingBarycentricIndex;

        public byte m_numBarycentrics;

    }


    public class Block : IHavokObject
    {
        public float m_safeDisplacementRadius;

        public ushort m_startingVCPIndex;

        public byte m_numVCPs;

    }


}

