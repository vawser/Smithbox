// Automatically Generated

namespace HKLib.hk2019;

public class hknpConvexHull : IHavokObject
{
    public List<hkFloat3> m_vertices = new();

    public List<Vector4> m_planes = new();

    public List<hknpConvexHull.Face> m_faces = new();

    public List<byte> m_indices = new();

    public List<hknpConvexHull.Edge> m_faceLinks = new();

    public List<hknpConvexHull.Edge> m_vertexEdges = new();


    public class Edge : IHavokObject
    {
        public ushort m_faceIndex;

        public byte m_edgeIndex;

    }

    public class Face : IHavokObject
    {
        public ushort m_firstIndex;

        public byte m_numIndices;

        public byte m_minHalfAngle;

    }

}
